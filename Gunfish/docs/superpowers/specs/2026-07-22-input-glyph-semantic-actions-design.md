# Input Glyph Semantic Actions — Design

## Background

The original design (`2026-07-20-input-glyph-icons-design.md`) resolved `ControlSchemeIcon`
glyphs by walking a real `InputAction`'s bindings and picking the first control whose
binding group matched the player's active control scheme. That approach only works when
the semantic thing you want an icon for IS a real, single-control-bindable `InputAction`
defined in `Gunfish.inputactions`.

In practice, the icon set this project actually needs is a fixed, small, well-known set
of five semantic prompts: `Select`, `Back`, `Menu`, `Move`, `Fire`. Three of these
(`Select`, `Back`, `Menu`) have no dedicated `InputAction` anywhere in the asset —
`UI.Submit`/`UI.Cancel` are the closest analogues, and there is nothing for `Menu` at
all. `Move` is fundamentally a 2-axis composite (WASD/arrow cluster, left analog stick,
physical joystick) that can't be reduced to "the bare name of one control" the way a
single button binding can — the best a binding-walker could do is arbitrarily pick one
axis's control name (e.g. `"leftStick"`), which isn't a distinct glyph concept.

Building a binding-walker that partially works for 2 of 5 actions and needs bespoke
exceptions for the other 3 is more code and more fragile than declaring the fixed table
directly. This is a fixed 5-icon HUD/menu prompt set, not a rebindable-and-reflected-live
system — that was already a non-goal in the original design, and remains true here: no
rebinding UI exists or is planned.

## Goals

- A fixed `PlayerAction { Select, Back, Menu, Move, Fire }` enum — the complete,
  closed set of semantic icons this project needs.
- A static, hardcoded per-`(action, scheme)` sprite table baked directly into
  `InputGlyphSet`/`InputGlyphDatabase` — no `InputAction`/binding indirection anywhere
  in the resolution path.
- `Move` resolved as one whole composite glyph per scheme (WASD/arrow cluster on
  keyboard, left analog stick on gamepad, the physical joystick on cabinet), not a
  single-control lookup.
- Cabinet's shared physical fire button resolves both `Select` and `Fire` to the same
  sprite — a data fact (both entries point at the same file), not a code branch.
- Cabinet's nonexistent physical "Menu" button is handled by an explicit, well-commented
  code-level override in `InputGlyphDatabase.GetSprite` that substitutes the keyboard's
  `Menu` glyph — never the generic fallback sprite.
- Never render a blank icon: unmapped `(action, scheme)` pairs fall back to a configured
  default sprite (except the cabinet-Menu case, which has its own explicit override).

## Non-goals

- Rebinding UI / persistence. This remains a fixed-prompt HUD/menu icon system, not a
  rebind-reflecting one — same non-goal as the original design.
- Cleaning up or changing `Gunfish.inputactions` itself. This design deliberately
  decouples glyph resolution from that asset entirely.
- Brand-specific gamepad art beyond the single Xbox-sourced Kenney set already in use.
- Any change to `PlatformConfig`, `ArduinoManager`, or the `Joystick`/`Gamepad`/
  `Keyboard&Mouse` control scheme definitions themselves.

## Architecture

```
ControlSchemeIcon.UpdateIcon(PlayerInput input)
        │  calls, using its serialized `PlayerAction action` field directly
        ▼
InputGlyphDatabase.GetSprite(PlayerAction action, string controlScheme)
        │  resolves via
        ▼
InputGlyphSet (Keyboard / Gamepad / Cabinet) . TryGetSprite(PlayerAction action, out Sprite sprite)
```

No `InputAction`, no `player.input.actions.FindAction(...)`, no binding-walk — the enum
value IS the lookup key, end to end.

### `PlayerAction` enum

Declared in `ControlSchemeIcon.cs` (unchanged location from the original design):

```csharp
public enum PlayerAction {
    Select,
    Back,
    Menu,
    Move,
    Fire,
}
```

Unity serializes enum fields as their underlying int in declaration order
(`Select=0, Back=1, Menu=2, Move=3, Fire=4`) — see Known limitation below for why this
matters to already-serialized data.

### `InputGlyphSet.Entry`

```csharp
[System.Serializable]
public struct Entry {
    public PlayerAction action;
    public Sprite glyph;
}
```

`TryGetSprite(PlayerAction action, out Sprite sprite)` replaces the old
`TryGetSprite(string controlName, out Sprite sprite)` — same linear-scan-a-list shape,
just keyed by the enum instead of a string.

### `InputGlyphDatabase.GetSprite`

```csharp
public Sprite GetSprite(PlayerAction action, string controlScheme) {
    // Explicit override: the cabinet's physical front panel has no "Menu" button.
    // Per design, Menu on cabinet always shows the KEYBOARD glyph (Esc) — never the
    // generic fallback sprite. cabinetGlyphs deliberately has no Menu entry seeded
    // (see InputGlyphSetSeeder.CabinetEntries) so this branch is the only path that
    // can ever produce a cabinet-scheme Menu sprite.
    if (controlScheme == "Joystick" && action == PlayerAction.Menu) {
        if (keyboardGlyphs != null && keyboardGlyphs.TryGetSprite(PlayerAction.Menu, out var keyboardMenuSprite))
            return keyboardMenuSprite;
        return fallbackGlyph;
    }

    InputGlyphSet set = controlScheme switch {
        "Keyboard&Mouse" => keyboardGlyphs,
        "Gamepad" => gamepadGlyphs,
        "Joystick" => cabinetGlyphs,
        _ => null,
    };
    if (set == null) return fallbackGlyph;

    return set.TryGetSprite(action, out var sprite) ? sprite : fallbackGlyph;
}
```

The cabinet-Menu override is checked *before* the scheme switch so it can't be
short-circuited by `cabinetGlyphs` being null or non-null — it's scheme+action specific,
independent of what's in `cabinetGlyphs`. The seeder reinforces this by deliberately NOT
adding a `Menu` row to `CabinetEntries` at all (4 cabinet entries, not 5) — the absence
is intentional and self-documenting, not an oversight masked by generic-fallback logic.

`using System.Linq;` and `using UnityEngine.InputSystem;` are both dropped from this
file — no binding-walking, no `InputAction`/`InputBinding` types referenced anywhere.

### Kenney file mapping (all 14 entries)

| Action | Keyboard | Gamepad (Xbox Series) | Cabinet (Generic) |
|---|---|---|---|
| `Select` | `keyboard_enter.png` | `xbox_button_a.png` | `generic_button_trigger_a.png` |
| `Back` | `keyboard_backspace.png` | `xbox_button_b.png` | `generic_button_trigger_b.png` |
| `Menu` | `keyboard_escape.png` | `xbox_button_start.png` | *(none — falls back to keyboard `Menu` via code override)* |
| `Move` | `keyboard_arrows_all.png` | `xbox_stick_l.png` | `generic_joystick_red.png` |
| `Fire` | `keyboard_space.png` | `xbox_rt.png` | `generic_button_trigger_a.png` *(same file as `Select`)* |

Fallback (unchanged): `generic_button.png`.

`keyboard_arrows_all.png` is the best available all-directions composite in the pack —
there is no lettered "WASD cluster" icon, only arrow-key-cluster icons. `xbox_stick_l.png`
is a whole-stick icon with no direction baked in, correct for a 2-axis composite.
Cabinet's `Select` and `Fire` intentionally share `generic_button_trigger_a.png`: the
cabinet's single physical front-panel button is dual-purpose (confirm/shoot).

## Known limitation

`Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab` has two pre-existing
`ControlSchemeIcon` components serialized with the *old* enum's raw int values
(`action: 3`, old `Respawn`, and `action: 1`, old `Fire`). Since Unity serializes enum
fields as their underlying int, reordering the enum to `Select=0, Back=1, Menu=2,
Move=3, Fire=4` silently re-targets those two components to `Move` and `Back`
respectively, with no visible error. This is not fixed here — see the companion plan
doc's Known limitation note for the deferred remediation.

## Testing

No automated test suite in this project — manual verification in-editor and in build:

- Select `KeyboardGlyphs.asset`: confirm `entries` has 5 rows with `Action` values
  `Select/Back/Menu/Move/Fire`, each showing a non-empty `Glyph` thumbnail.
- Select `CabinetGlyphs.asset`: confirm 4 rows (no `Menu` row present) and that the
  `Select` and `Fire` rows' `Glyph` thumbnails are visibly the same sprite.
- Select `InputGlyphDatabase.asset`: confirm all three set references and
  `Fallback Glyph` are assigned (not `None`).
- With a `ControlSchemeIcon` temporarily set to `action = Menu` on a test `Image` (or a
  scratch Editor script calling `InputGlyphDatabase.GetSprite(PlayerAction.Menu,
  "Joystick")` directly), confirm the returned sprite is the keyboard `Escape` glyph, not
  `fallbackGlyph` and not null.
- Play with keyboard only: shoot icon shows `keyboard_space.png`'s sprite.
- Join a second player with a gamepad: that player's `Fire` icon shows `xbox_rt.png`'s
  sprite, independently of player 1's keyboard icon.
- Cabinet hardware: `Select`/`Fire` icons both show `generic_button_trigger_a.png`'s
  sprite; a `Menu` icon (if wired anywhere for cabinet) shows the keyboard `Escape`
  glyph, never blank and never the generic fallback.
