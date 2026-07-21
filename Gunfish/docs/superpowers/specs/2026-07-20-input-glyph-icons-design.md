# Input Glyph Icons — Design

## Background

The HUD needs button-prompt icons (e.g. a "shoot" indicator) that automatically show
the correct art for whatever device a player is actually using — cabinet joystick/fire
button, keyboard, or gamepad — and update live if the player swaps devices mid-session.

Controls are meant to be fully customizable in the future (rebindable keys/buttons), so
icons can't be hardcoded per control scheme; they need to reflect whichever physical
control is actually bound to the action. Building the rebinding UI/persistence itself is
out of scope for this pass — see Non-goals — but the icon-resolution architecture is
built so rebinding can be added later with no rework.

`PlatformConfig.IsCabinet` (added in `2026-07-20-arcade-cabinet-platform-config-design.md`)
is not used here: the cabinet's USB joystick/button controller already registers as a
real Input System device under the existing `Joystick` control scheme, so
`PlayerInput.currentControlScheme` alone is sufficient to distinguish cabinet from
keyboard from gamepad. The cabinet's Arduino connection (`ArduinoManager`) is unrelated
to player input — it only drives an output prop (Big Mouth Billy Bass) — and plays no
role in this design.

Today, `Gunfish.inputactions` binds `Fire` to multiple controls per scheme (e.g. keyboard
has `leftShift`, `space`, `rightCtrl`, `numpad0`; gamepad has `rightTrigger` and an
ungrouped `buttonEast`; there's also a catch-all `<Keyboard>/anyKey` binding spanning all
groups). The icon resolver needs a deterministic rule for which one to display — see
Architecture.

## Goals

- A `ControlSchemeIcon` component that can sit on any HUD `Image` and display the correct
  button-prompt sprite for whichever physical control is currently bound to a named
  action (default: `Fire`), for the owning player's currently active device.
- Reusable across multiple prompts (shoot, and future ones like jump) by pointing the
  component at a different action name — no new per-prompt ScriptableObject assets
  required.
- Live updates when a player's device changes (keyboard ↔ gamepad ↔ cabinet, or a
  gamepad is unplugged/replugged), driven by the Input System's existing
  `PlayerInput` control-scheme-changed notification.
- Broad glyph coverage: keyboard set covers alphanumeric keys plus common specials
  (space, shift, ctrl, alt, enter, esc, tab, arrows) so future rebinds to any of those
  keys already have art; gamepad set covers the standard face
  buttons/shoulders/triggers/start/select/dpad/stick-press controls Unity's generic
  `<Gamepad>` layout exposes; cabinet set covers only its three physical controls
  (joystick, fire button, back button).
- Deterministic resolution when multiple bindings exist for one scheme: walk the
  action's bindings in asset order, use the first whose `groups` is empty or contains
  the active scheme's binding group name.
- Never render a blank icon: unmapped control paths or schemes with no matching binding
  fall back to a configured default sprite.

## Non-goals

- Building an interactive rebinding UI ("press any key to rebind") or persisting custom
  bindings (`PlayerPrefs`/save file). Icons will correctly reflect whatever the action's
  *current* effective binding is — including future rebind overrides once that system
  exists — but no rebind flow is built in this pass.
- Cleaning up the existing multi-binding-per-scheme setup in `Gunfish.inputactions`
  (e.g. reducing keyboard `Fire` to a single canonical key). The "first matching binding
  in asset order" rule works with the data as it stands today.
- Brand-specific gamepad art (Xbox vs. PlayStation glyphs). One gamepad glyph set is
  built against Unity's already-abstracted generic `<Gamepad>` layout (`buttonSouth`,
  `buttonEast`, etc.); per-brand art is a future enhancement if ever needed.
- Any change to `PlatformConfig`, `ArduinoManager`, or the `Joystick`/`Gamepad`/
  `Keyboard&Mouse` control scheme definitions themselves.

## Architecture

Four pieces:

```
PlayerInput device change
        │
        ▼
Player.OnControlsChanged(PlayerInput)   ← existing IDeviceController stub, currently empty
        │  raises
        ▼
Player.ControlSchemeChanged event (new)
        │
        ▼
ControlSchemeIcon.UpdateIcon(PlayerInput input)
        │  calls
        ▼
InputGlyphDatabase.GetSprite(action, input.currentControlScheme)
        │  resolves via
        ▼
InputGlyphSet (Keyboard / Gamepad / Cabinet) . GetSprite(controlName)
```

### `Player.OnControlsChanged` → `ControlSchemeChanged` event

`Player` already implements `IDeviceController.OnControlsChanged(PlayerInput input)` as
an empty stub — Unity's `PlayerInput` (Send Messages notification behavior) already
calls it on every device/control-scheme change, so no new wiring to `PlayerInput` itself
is needed. Fill the stub to raise a new event:

```csharp
public void OnControlsChanged(PlayerInput input) {
    ControlSchemeChanged?.Invoke(input);
}

public event ControlSchemeGameEvent ControlSchemeChanged;
```

`ControlSchemeGameEvent` is a new delegate in `GameEvents.cs`, matching the existing
`PlayerGameEvent(Player player)` style:

```csharp
public delegate void ControlSchemeGameEvent(PlayerInput input);
```

### `InputGlyphSet` (ScriptableObject)

One asset per device layout. A serialized list acting as a control-name → sprite map:

```csharp
[CreateAssetMenu(fileName = "New Input Glyph Set", menuName = "Scriptable Objects/Input Glyph Set")]
public class InputGlyphSet : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public string controlName; // e.g. "leftShift", "buttonEast", "trigger"
        public Sprite glyph;
    }

    public List<Entry> entries;

    public bool TryGetSprite(string controlName, out Sprite sprite) {
        foreach (var e in entries) {
            if (e.controlName == controlName) { sprite = e.glyph; return true; }
        }
        sprite = null;
        return false;
    }
}
```

Three assets: `KeyboardGlyphs.asset`, `GamepadGlyphs.asset`, `CabinetGlyphs.asset`.

### `InputGlyphDatabase` (ScriptableObject)

One shared asset referencing the three sets plus a fallback sprite, and owning the
resolution logic:

```csharp
[CreateAssetMenu(fileName = "New Input Glyph Database", menuName = "Scriptable Objects/Input Glyph Database")]
public class InputGlyphDatabase : ScriptableObject {
    public InputGlyphSet keyboardGlyphs;
    public InputGlyphSet gamepadGlyphs;
    public InputGlyphSet cabinetGlyphs;
    public Sprite fallbackGlyph;

    public Sprite GetSprite(InputAction action, string controlScheme) {
        var set = controlScheme switch {
            "Keyboard&Mouse" => keyboardGlyphs,
            "Gamepad" => gamepadGlyphs,
            "Joystick" => cabinetGlyphs,
            _ => null,
        };
        if (set == null) return fallbackGlyph;

        foreach (var binding in action.bindings) {
            if (binding.isComposite || binding.isPartOfComposite) continue;
            if (!string.IsNullOrEmpty(binding.groups) && !binding.groups.Split(';').Contains(controlScheme))
                continue;

            // "<Keyboard>/leftShift" -> "leftShift"
            var controlName = binding.effectivePath.Substring(binding.effectivePath.LastIndexOf('/') + 1);
            if (set.TryGetSprite(controlName, out var sprite))
                return sprite;
        }
        return fallbackGlyph;
    }
}
```

`InputGlyphSet` entries are keyed by this same bare control name (e.g. `"leftShift"`,
`"buttonEast"`, `"trigger"`) — whatever appears after the last `/` in the binding's
path — so authoring a new glyph entry is just reading the path off the binding in the
Input Actions editor.

### `ControlSchemeIcon` (MonoBehaviour)

Lives on the HUD `Image` (e.g. a child of the per-player HUD group alongside
`HealthUI`, following that component's `Init(gunfish)`-from-owner lifecycle):

```csharp
public class ControlSchemeIcon : MonoBehaviour {
    [SerializeField] private Image targetImage;
    [SerializeField] private string actionName = "Fire";
    [SerializeField] private InputGlyphDatabase glyphDatabase;

    private Player player;
    private InputAction action;

    void Awake() {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    public void Init(Player player) {
        Unhook();
        this.player = player;
        action = player.input.actions.FindAction(actionName);
        player.ControlSchemeChanged += UpdateIcon;
        UpdateIcon(player.input);
    }

    void UpdateIcon(PlayerInput input) {
        targetImage.sprite = glyphDatabase.GetSprite(action, input.currentControlScheme);
    }

    void OnDestroy() => Unhook();

    void Unhook() {
        if (player != null) player.ControlSchemeChanged -= UpdateIcon;
    }
}
```

Reusing this for a different prompt (e.g. a future "Jump" icon) is just a second
instance with `actionName = "Jump"` — no new SO assets needed per prompt, only new
glyph entries in the existing three sets if a control they use isn't covered yet.

## Error handling

- Unmapped control name for the resolved device (glyph set has no entry) → falls
  through to `InputGlyphDatabase.fallbackGlyph`.
- No binding matches the active control scheme at all → same fallback.
- Unrecognized control scheme string (e.g. `Touch`, `XR` — defined in the action asset
  but unused in play) → `GetSprite`'s `switch` falls to `fallbackGlyph`.
- `FindAction(actionName)` returning `null` (typo'd action name) is a configuration
  error, not a runtime condition to swallow silently — left as a null-reference at
  `Init` time so it's caught immediately in the Editor rather than masked.

## Testing

No automated test suite in this project — manual verification in-editor and in build:

- Play with keyboard only: shoot icon shows a keyboard glyph matching the first
  keyboard binding in asset order (`leftShift`).
- Join a second player with a gamepad: that player's icon shows a gamepad glyph
  (`rightTrigger`, the first matching Gamepad-grouped binding), independently of
  player 1's keyboard icon.
- Unplug and replug/swap the gamepad mid-session: icon updates live via
  `ControlSchemeChanged`, no stale sprite.
- Cabinet hardware (or `DevConfigOverride` simulated build target, if that also
  simulates device presence — verify during implementation): icon shows the cabinet
  fire-button glyph.
- Temporarily remove a glyph entry for a bound control: confirm it falls back to
  `fallbackGlyph` instead of an empty/missing sprite.
