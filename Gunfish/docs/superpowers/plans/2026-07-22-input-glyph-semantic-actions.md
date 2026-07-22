# Input Glyph Semantic Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rework `ControlSchemeIcon`'s glyph-resolution system away from dynamic `InputAction`-binding walking and onto a fixed, fully static semantic action set — `PlayerAction { Select, Back, Menu, Move, Fire }` — with a hardcoded per-`(action, scheme)` sprite table baked directly into `InputGlyphSet`/`InputGlyphDatabase`, replacing the old control-name-keyed lookup.

**Architecture:** `PlayerAction` is a fixed 5-value enum declared in `ControlSchemeIcon.cs`. `InputGlyphSet.Entry` is now keyed by `PlayerAction` instead of a control-name string. `InputGlyphDatabase.GetSprite(PlayerAction action, string controlScheme)` switches on `controlScheme` to pick a set and looks up the sprite directly by enum value — no `InputAction`/binding involved anywhere. An explicit early-return branch handles the cabinet's nonexistent physical "Menu" button by substituting the keyboard's `Menu` glyph. `InputGlyphSetSeeder` seeds exactly 5 keyboard + 5 gamepad + 4 cabinet entries (cabinet has no `Menu` row) from the Kenney "Input Prompts" pack already imported into the project.

**Tech Stack:** Unity 6000.5.0f1, C#, Unity New Input System 1.7.0 (referenced only for `PlayerInput` type, not for binding resolution), `UnityEditor.AssetDatabase` (Editor-only seeding).

## Global Constraints

- No automated test suite in this project (confirmed in `CLAUDE.md`) — every task's verification is a `dotnet build` compile check plus, where noted, a manual in-Editor check.
- Never hand-edit prefab or scene files. `MainMenuCanvas.prefab` and `FishHealthUI.prefab` are both explicitly out of scope for this plan — see Known limitation below.
- The `InputGlyphSet`/`InputGlyphDatabase`/`InputGlyphSetSeeder` `.asset` files are regenerated via the existing **Tools → Gunfish → Seed Input Glyph Sets** Editor menu command — not hand-authored, and not a prefab/scene file.
- Kenney's PNGs (`Assets/Resources/Sprites/UI/kenney_input_prompts/`) are imported with Sprite Mode "Multiple" — each file has exactly one sprite sub-asset, named `<filename>_0`, which is **not** the file's main asset. Always load them via `AssetDatabase.LoadAllAssetsAtPath(path)` filtered to `Sprite`, never `AssetDatabase.LoadAssetAtPath<Sprite>(path)` (returns `null` for these files).
- `PlatformConfig.IsCabinet` / `ArduinoManager` are not touched by this plan.

## File Structure

- **Modify** `Assets/Scripts/UI/ControlSchemeIcon.cs` — replace `PlayerAction` enum, drop the `InputAction` field/`FindAction` call (Task 1).
- **Modify** `Assets/Scripts/ScriptableObjects/InputGlyphSet.cs` — `Entry.controlName` (`string`) → `Entry.action` (`PlayerAction`) (Task 2).
- **Modify** `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs` — `GetSprite(InputAction, string)` → `GetSprite(PlayerAction, string)`, drop binding-walk, add cabinet-Menu override (Task 3).
- **Modify** `Assets/Scripts/Editor/InputGlyphSetSeeder.cs` — replace broad per-key/per-button tables with the fixed 5/5/4 `PlayerAction`-keyed tables (Task 4).
- **Regenerate** (via Editor menu command, not hand-authored) `Assets/Resources/ScriptableObjects/InputGlyphs/{KeyboardGlyphs,GamepadGlyphs,CabinetGlyphs,InputGlyphDatabase}.asset` (Task 4).

---

### Task 1: `ControlSchemeIcon.cs` — new `PlayerAction` enum, drop `InputAction`

**Files:**
- Modify: `Assets/Scripts/UI/ControlSchemeIcon.cs`

**Interfaces:**
- Consumes: `InputGlyphDatabase.GetSprite(PlayerAction, string)` (produced by Task 3).
- Produces: `PlayerAction` enum (`Select, Back, Menu, Move, Fire`) — consumed by Tasks 2, 3, 4.

- [ ] **Step 1: Replace the enum and drop `InputAction` usage**

Replace the entire file:
```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PlayerAction {
    Select,
    Back,
    Menu,
    Move,
    Fire,
}

public class ControlSchemeIcon : MonoBehaviour {
    [SerializeField] private Image targetImage;
    [SerializeField] private PlayerAction action = PlayerAction.Fire;
    [SerializeField] private InputGlyphDatabase glyphDatabase;

    private Player player;

    private void Awake() {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    public void Init(Player player) {
        Unhook();
        this.player = player;
        player.ControlSchemeChanged += UpdateIcon;
        UpdateIcon(player.input);
    }

    private void UpdateIcon(PlayerInput input) {
        targetImage.sprite = glyphDatabase.GetSprite(action, input.currentControlScheme);
    }

    private void OnDestroy() {
        Unhook();
    }

    private void Unhook() {
        if (player != null) player.ControlSchemeChanged -= UpdateIcon;
    }
}
```

- [ ] **Step 2: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/ControlSchemeIcon.cs
git commit -m "refactor: replace ControlSchemeIcon's InputAction resolution with a static PlayerAction enum"
```

---

### Task 2: `InputGlyphSet.cs` — key entries by `PlayerAction`

**Files:**
- Modify: `Assets/Scripts/ScriptableObjects/InputGlyphSet.cs`

**Interfaces:**
- Consumes: `PlayerAction` (produced by Task 1).
- Produces: `InputGlyphSet.Entry { PlayerAction action; Sprite glyph; }`, `TryGetSprite(PlayerAction, out Sprite)` — consumed by Task 3 (`InputGlyphDatabase`) and Task 4 (seeder).

- [ ] **Step 1: Replace the entire file**

```csharp
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Input Glyph Set", menuName = "Scriptable Objects/Input Glyph Set")]
public class InputGlyphSet : ScriptableObject {
    [System.Serializable]
    public struct Entry {
        public PlayerAction action;
        public Sprite glyph;
    }

    public List<Entry> entries = new List<Entry>();

    public bool TryGetSprite(PlayerAction action, out Sprite sprite) {
        foreach (var entry in entries) {
            if (entry.action == action) {
                sprite = entry.glyph;
                return true;
            }
        }
        sprite = null;
        return false;
    }
}
```

- [ ] **Step 2: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/ScriptableObjects/InputGlyphSet.cs
git commit -m "refactor: key InputGlyphSet.Entry by PlayerAction instead of control-name string"
```

---

### Task 3: `InputGlyphDatabase.cs` — static lookup + cabinet-Menu override

**Files:**
- Modify: `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs`

**Interfaces:**
- Consumes: `PlayerAction` (Task 1), `InputGlyphSet.TryGetSprite(PlayerAction, out Sprite)` (Task 2).
- Produces: `InputGlyphDatabase.GetSprite(PlayerAction, string)` — consumed by Task 1 (`ControlSchemeIcon`, already wired) and Task 4 (seeder, which populates the referenced sets).

- [ ] **Step 1: Replace the entire file**

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Input Glyph Database", menuName = "Scriptable Objects/Input Glyph Database")]
public class InputGlyphDatabase : ScriptableObject {
    public InputGlyphSet keyboardGlyphs;
    public InputGlyphSet gamepadGlyphs;
    public InputGlyphSet cabinetGlyphs;
    public Sprite fallbackGlyph;

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
}
```

- [ ] **Step 2: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs
git commit -m "refactor: drop InputAction binding-walk from InputGlyphDatabase.GetSprite; add cabinet-Menu override"
```

---

### Task 4: `InputGlyphSetSeeder.cs` — seed the fixed 5/5/4 table

**Files:**
- Modify: `Assets/Scripts/Editor/InputGlyphSetSeeder.cs`
- Regenerated by running the seeder (not hand-authored): `Assets/Resources/ScriptableObjects/InputGlyphs/{KeyboardGlyphs,GamepadGlyphs,CabinetGlyphs,InputGlyphDatabase}.asset`

**Interfaces:**
- Consumes: `PlayerAction` (Task 1), `InputGlyphSet.Entry` (Task 2), `InputGlyphDatabase` (Task 3).
- Produces: the four `.asset` files, consumed at runtime by `ControlSchemeIcon` wherever it's wired (`MainMenuCanvas.prefab` today; `FishHealthUI.prefab` wiring remains separately pending work).

- [ ] **Step 1: Confirm the Kenney source files exist at the expected paths**

Run (from `Gunfish/`):
```bash
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Keyboard & Mouse/Default/keyboard_enter.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Keyboard & Mouse/Default/keyboard_backspace.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Keyboard & Mouse/Default/keyboard_escape.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Keyboard & Mouse/Default/keyboard_arrows_all.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Keyboard & Mouse/Default/keyboard_space.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Xbox Series/Default/xbox_button_a.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Xbox Series/Default/xbox_button_b.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Xbox Series/Default/xbox_button_start.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Xbox Series/Default/xbox_stick_l.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Xbox Series/Default/xbox_rt.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Generic/Default/generic_button_trigger_a.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Generic/Default/generic_button_trigger_b.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Generic/Default/generic_joystick_red.png" && \
test -f "Assets/Resources/Sprites/UI/kenney_input_prompts/Generic/Default/generic_button.png" && \
echo OK
```
Expected: `OK`.

- [ ] **Step 2: Replace the seeder script**

```csharp
using System.IO;
using UnityEditor;
using UnityEngine;

public static class InputGlyphSetSeeder {
    private const string KenneyRoot = "Assets/Resources/Sprites/UI/kenney_input_prompts";
    private const string OutputRoot = "Assets/Resources/ScriptableObjects/InputGlyphs";

    // The fixed semantic action set. Move represents the entire 2-axis movement cluster
    // (WASD/arrows on keyboard, left analog stick on gamepad, the physical joystick on
    // cabinet) as a single composite glyph per scheme — a 2-axis control can't be reduced
    // to "one control's bare name" the way a single button binding can.
    private static readonly (PlayerAction action, string kenneyFile)[] KeyboardEntries = {
        (PlayerAction.Select, "keyboard_enter.png"),
        (PlayerAction.Back, "keyboard_backspace.png"),
        (PlayerAction.Menu, "keyboard_escape.png"),
        (PlayerAction.Move, "keyboard_arrows_all.png"),
        (PlayerAction.Fire, "keyboard_space.png"),
    };

    private static readonly (PlayerAction action, string kenneyFile)[] GamepadEntries = {
        (PlayerAction.Select, "xbox_button_a.png"),
        (PlayerAction.Back, "xbox_button_b.png"),
        (PlayerAction.Menu, "xbox_button_start.png"),
        (PlayerAction.Move, "xbox_stick_l.png"),
        (PlayerAction.Fire, "xbox_rt.png"),
    };

    // No Menu entry: the cabinet has no physical Menu button. InputGlyphDatabase.GetSprite
    // special-cases (Joystick, Menu) to explicitly fall back to the KEYBOARD Menu glyph —
    // see that method; do not add a Menu row here. Select and Fire intentionally share the
    // same sprite: the cabinet's single physical front-panel button is dual-purpose
    // (confirm/shoot).
    private static readonly (PlayerAction action, string kenneyFile)[] CabinetEntries = {
        (PlayerAction.Select, "generic_button_trigger_a.png"),
        (PlayerAction.Back, "generic_button_trigger_b.png"),
        (PlayerAction.Move, "generic_joystick_red.png"),
        (PlayerAction.Fire, "generic_button_trigger_a.png"),
    };

    [MenuItem("Tools/Gunfish/Seed Input Glyph Sets")]
    public static void Seed() {
        Directory.CreateDirectory(OutputRoot);

        var keyboardSet = SeedSet("KeyboardGlyphs", $"{KenneyRoot}/Keyboard & Mouse/Default", KeyboardEntries);
        var gamepadSet = SeedSet("GamepadGlyphs", $"{KenneyRoot}/Xbox Series/Default", GamepadEntries);
        var cabinetSet = SeedSet("CabinetGlyphs", $"{KenneyRoot}/Generic/Default", CabinetEntries);
        var fallback = LoadGlyphSprite($"{KenneyRoot}/Generic/Default/generic_button.png");

        var database = LoadOrCreateAsset<InputGlyphDatabase>($"{OutputRoot}/InputGlyphDatabase.asset");
        database.keyboardGlyphs = keyboardSet;
        database.gamepadGlyphs = gamepadSet;
        database.cabinetGlyphs = cabinetSet;
        database.fallbackGlyph = fallback;
        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();
        Debug.Log("Seeded Input Glyph Sets: " +
            $"{keyboardSet.entries.Count} keyboard, {gamepadSet.entries.Count} gamepad, {cabinetSet.entries.Count} cabinet entries.");
    }

    private static InputGlyphSet SeedSet(string assetName, string sourceFolder, (PlayerAction action, string kenneyFile)[] entries) {
        var set = LoadOrCreateAsset<InputGlyphSet>($"{OutputRoot}/{assetName}.asset");
        set.entries.Clear();
        foreach (var (action, kenneyFile) in entries) {
            var sprite = LoadGlyphSprite($"{sourceFolder}/{kenneyFile}");
            set.entries.Add(new InputGlyphSet.Entry { action = action, glyph = sprite });
        }
        EditorUtility.SetDirty(set);
        return set;
    }

    private static Sprite LoadGlyphSprite(string path) {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) {
            if (asset is Sprite sprite) return sprite;
        }
        Debug.LogError($"[InputGlyphSetSeeder] No sprite sub-asset found at {path}");
        return null;
    }

    private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
```

- [ ] **Step 3: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 4: Re-run the seeder**

In the Unity Editor: **Tools → Gunfish → Seed Input Glyph Sets**. Expected Console log:
`Seeded Input Glyph Sets: 5 keyboard, 5 gamepad, 4 cabinet entries.` This overwrites the
four existing assets, replacing their stale `controlName`-keyed data with the new
`PlayerAction`-keyed entries.

- [ ] **Step 5: Manual check — inspect entries**

Select `KeyboardGlyphs.asset`; confirm `entries` has 5 rows with `Action` values
`Select/Back/Menu/Move/Fire`, each showing a non-empty `Glyph` thumbnail. Select
`CabinetGlyphs.asset`; confirm 4 rows (no `Menu` row present) and that the `Select` and
`Fire` rows' `Glyph` thumbnails are visibly the same sprite. Select
`InputGlyphDatabase.asset`; confirm all three set references and `Fallback Glyph` are
assigned (not `None`).

- [ ] **Step 6: Manual check — cabinet Menu override**

With a `ControlSchemeIcon` temporarily set to `action = Menu` on a test `Image` in a
scratch scene/GameObject (or via a temporary log statement calling
`InputGlyphDatabase.GetSprite(PlayerAction.Menu, "Joystick")` from a scratch Editor
script), confirm the returned sprite is the keyboard `Escape` glyph, not `fallbackGlyph`
and not null.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/Editor/InputGlyphSetSeeder.cs "Assets/Resources/ScriptableObjects/InputGlyphs"
git commit -m "feat: reseed input glyph sets against the fixed 5-action semantic table"
```

---

## Known limitation

`Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab` already has two
`ControlSchemeIcon` components serialized with the *old* enum's raw int values
(`action: 3`, i.e. old `Respawn`, and `action: 1`, i.e. old `Fire`). Since Unity
serializes enum fields as their underlying int, reordering the enum to `Select=0,
Back=1, Menu=2, Move=3, Fire=4` silently re-targets those two components to `Move` and
`Back` respectively, with no visible error. This plan does **not** touch that prefab —
it has unrelated, in-progress, uncommitted hand-edits, and this plan does not depend on
its current state. Whoever finishes the `MainMenuCanvas` work will need to re-pick the
correct `Action` dropdown value for those two components once their edit is done — this
is deferred, out-of-scope cleanup, not a regression introduced silently: it is called out
here explicitly so it isn't lost.
