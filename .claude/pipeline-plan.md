---
max_iterations: 3
---

## Goal

Add a new, independently-toggleable Editor dev override — `IgnoreRequiredPlayerCount` — to `DevConfigOverride` that, when active, makes any number of ready players (>=1) sufficient to start a match in `FishSelectMenuPage`, regardless of the current `GameMode.requiredPlayerCount`, for any game mode. This override must be separate from the existing generic `debug`/`DebugOverride` flag: it must be independently toggleable in the Dev Config window and independently checkable in code, while still composing (OR'ing) with the existing `debug`-driven bypass in `AllPlayersReady()` so either flag alone is sufficient to bypass the required-count gate. The `gameModeNote` UI text display logic (which always shows the real `requiredPlayerCount` values) must not change.

## Scope

- `Gunfish/Assets/Scripts/Managers/DevConfigOverride.cs` — add a new EditorPrefs-backed key, property, and `TryGet` accessor for the override, following the exact pattern of `DebugKey`/`DebugOverride`/`TryGetDebug`.
- `Gunfish/Assets/Scripts/Managers/GameManager.cs` — add a new public `ignoreRequiredPlayerCount` property (with private serialized backing field) mirroring the existing `debug` property exactly, and register a new debug-overlay entry in `RegisterDevConfigOverrideDebugEntries()`.
- `Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs` — extend the boolean gating logic in `AllPlayersReady()` (lines 255-277) to OR the new override's bypass condition together with the existing `debugCountAllowed` bypass; optionally extend the existing `OnPageStart()` `DebugRegistrar.Track` condition (line 55) to also fire when the new override is active. No change to the `gameModeNote` text-building logic (lines 40-51).
- `Gunfish/Assets/Scripts/Editor/DevConfigWindow.cs` — add a new `EditorGUILayout.Toggle` bound to the new `DevConfigOverride` property, placed after the existing `Debug Player Count` slider (after line 37), inside the existing `DisabledScope` block.

## Implementation Steps

### 1. `Gunfish/Assets/Scripts/Managers/DevConfigOverride.cs`

This file is entirely wrapped in `#if UNITY_EDITOR ... #endif` (lines 1 and 77). All additions go inside that region, no new `#if` guards needed.

- Add a new const key alongside the existing keys (after line 10, `private const string DebugPlayerCountKey = "Gunfish.Dev.DebugPlayerCount";`):
  ```csharp
  private const string IgnoreRequiredPlayerCountKey = "Gunfish.Dev.IgnoreRequiredPlayerCount";
  ```
  (Verified unique: no existing key string matches; confirmed via grep no prior use of `IgnoreRequiredPlayerCount` anywhere in `Assets/`.)

- Add a new public static property alongside `DebugOverride` (after the `DebugPlayerCountOverride` property, i.e. after line 35's closing brace):
  ```csharp
  public static bool IgnoreRequiredPlayerCountOverride {
      get => EditorPrefs.GetBool(IgnoreRequiredPlayerCountKey, false);
      set => EditorPrefs.SetBool(IgnoreRequiredPlayerCountKey, value);
  }
  ```

- Add a new `TryGet` method alongside `TryGetDebug`/`TryGetDebugPlayerCount` (after line 55's closing brace, before `private static T LoadAsset<T>`):
  ```csharp
  public static bool TryGetIgnoreRequiredPlayerCount(out bool ignoreRequiredPlayerCount) {
      ignoreRequiredPlayerCount = IgnoreRequiredPlayerCountOverride;
      return Enabled;
  }
  ```

### 2. `Gunfish/Assets/Scripts/Managers/GameManager.cs`

Mirror the `debug` property pattern exactly (lines 20-30), since `FishSelectMenuPage.cs` is a non-editor file that ships in builds and must not reference `DevConfigOverride` (an editor-only type) directly. This keeps the `#if UNITY_EDITOR` guard localized to `GameManager.cs`, consistent with how `debug`, `debugPlayerCount`, `GameModeList`, and `GunfishDataList` already work.

- Add a new serialized backing field + public property, placed after the `debugPlayerCount` property block (after line 42's closing brace, before `public bool useSavedVolumes = false;` on line 44):
  ```csharp
  [SerializeField]
  private bool _ignoreRequiredPlayerCount = false;
  public bool ignoreRequiredPlayerCount {
      get {
  #if UNITY_EDITOR
          if (DevConfigOverride.TryGetIgnoreRequiredPlayerCount(out var ignoreRequiredPlayerCountOverride)) return ignoreRequiredPlayerCountOverride;
  #endif
          return _ignoreRequiredPlayerCount;
      }
  }
  ```
  Note: unlike `_debug`/`_debugPlayerCount`, this new backing field has no prior serialized name, so no `[FormerlySerializedAs(...)]` attribute is needed.

- In `RegisterDevConfigOverrideDebugEntries()` (lines 90-99, inside the `#if UNITY_EDITOR` block that spans lines 89-100), add a new tracked entry after the existing `DevConfigOverride.DebugPlayerCount` entry (after line 94, before the `DevConfigOverride.GameModeList` entry):
  ```csharp
  DebugRegistrar.Track("DevConfigOverride.IgnoreRequiredPlayerCount", () =>
      DevConfigOverride.TryGetIgnoreRequiredPlayerCount(out var i) ? $"OVERRIDDEN -> {i}" : "inactive");
  ```

### 3. `Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs`

Edit `AllPlayersReady()` (currently lines 255-277). Replace the single `debugCountAllowed` local with two locals that are OR'd into a combined `bypassAllowed` flag, and use `bypassAllowed` everywhere `debugCountAllowed` was previously used. This is a pure boolean-composition change — the `allowedPlayerCounts`/`gameModeNote` tween logic (lines 268-274) and the `hasNoSelecting`/`readyPlayerCount` counting loop (lines 256-265) are untouched.

Current (lines 267-276):
```csharp
        bool debugCountAllowed = GameManager.Instance.debug == true && readyPlayerCount >= 1;
        if (allowedPlayerCounts.Contains(readyPlayerCount) == false && !debugCountAllowed) {
            // tween gamemode note for emphasis
            gameModeNote.transform.DOKill();
            gameModeNote.transform.localScale = Vector3.one;
            gameModeNote.transform.DOScale(new Vector3(1.1f, 1.1f, 1), 0.2f).SetLoops(4, LoopType.Yoyo);
            return false;
        }

        return hasNoSelecting && (debugCountAllowed || GameManager.Instance.currentGameMode.requiredPlayerCount.Contains(readyPlayerCount));
```

New:
```csharp
        bool debugCountAllowed = GameManager.Instance.debug == true && readyPlayerCount >= 1;
        bool ignoreRequiredCountAllowed = GameManager.Instance.ignoreRequiredPlayerCount == true && readyPlayerCount >= 1;
        bool bypassAllowed = debugCountAllowed || ignoreRequiredCountAllowed;
        if (allowedPlayerCounts.Contains(readyPlayerCount) == false && !bypassAllowed) {
            // tween gamemode note for emphasis
            gameModeNote.transform.DOKill();
            gameModeNote.transform.localScale = Vector3.one;
            gameModeNote.transform.DOScale(new Vector3(1.1f, 1.1f, 1), 0.2f).SetLoops(4, LoopType.Yoyo);
            return false;
        }

        return hasNoSelecting && (bypassAllowed || GameManager.Instance.currentGameMode.requiredPlayerCount.Contains(readyPlayerCount));
```

Also update the `OnPageStart()` debug-overlay registration (currently lines 55-59) so the on-screen note reflects either override being active, not just `debug`:

Current (lines 55-59):
```csharp
        if (GameManager.Instance.debug) {
            DebugRegistrar.Track("FishSelectMenuPage.ReadyPlayerGate", () =>
                $"OVERRIDDEN -> allow start with >=1 ready player " +
                $"(prod requires one of [{string.Join(", ", GameManager.Instance.currentGameMode.requiredPlayerCount)}])");
        }
```

New:
```csharp
        if (GameManager.Instance.debug || GameManager.Instance.ignoreRequiredPlayerCount) {
            DebugRegistrar.Track("FishSelectMenuPage.ReadyPlayerGate", () =>
                $"OVERRIDDEN -> allow start with >=1 ready player " +
                $"(prod requires one of [{string.Join(", ", GameManager.Instance.currentGameMode.requiredPlayerCount)}])");
        }
```

Do not modify `gameModeNote.text` construction (lines 40-51) — that must keep displaying the real `requiredPlayerCount` values regardless of override state.

`FishSelectMenuPage.cs` must not gain any reference to `DevConfigOverride` or any `#if UNITY_EDITOR` block — it only calls the new `GameManager.Instance.ignoreRequiredPlayerCount` public property, exactly like it already does for `GameManager.Instance.debug`.

### 4. `Gunfish/Assets/Scripts/Editor/DevConfigWindow.cs`

Inside the existing `DisabledScope` block (lines 20-38), add a new toggle after the `Debug Player Count` slider (after line 37's closing `);`, still before the `}` that closes the `using` block on line 38):

```csharp
            EditorGUILayout.Space();
            var ignoreRequiredPlayerCountLabel = new GUIContent(
                "Ignore Required Player Count",
                "When enabled, any number of ready players (>=1) is sufficient to start a match, regardless of the current game mode's required player count.");
            DevConfigOverride.IgnoreRequiredPlayerCountOverride = EditorGUILayout.Toggle(
                ignoreRequiredPlayerCountLabel, DevConfigOverride.IgnoreRequiredPlayerCountOverride);
```

## Verification Criteria

All checks are static/grep/read-based — no Unity Editor GUI is used.

1. **New members exist with correct signatures** (`DevConfigOverride.cs`):
   - `grep -n "IgnoreRequiredPlayerCountKey" Gunfish/Assets/Scripts/Managers/DevConfigOverride.cs` returns exactly 2 matches: the `private const string` declaration and its one usage inside `IgnoreRequiredPlayerCountOverride`.
   - `grep -n "Gunfish.Dev.IgnoreRequiredPlayerCount" Gunfish/Assets/Scripts/Managers/DevConfigOverride.cs` returns exactly 1 match (the key string literal), and this exact string does not appear anywhere else in the file (i.e., it does not collide with `Gunfish.Dev.Debug`, `Gunfish.Dev.DebugPlayerCount`, `Gunfish.Dev.Enabled`, `Gunfish.Dev.GameModeListGuid`, `Gunfish.Dev.GunfishDataListGuid`).
   - `grep -n "public static bool IgnoreRequiredPlayerCountOverride" Gunfish/Assets/Scripts/Managers/DevConfigOverride.cs` returns exactly 1 match.
   - `grep -n "public static bool TryGetIgnoreRequiredPlayerCount(out bool ignoreRequiredPlayerCount)" Gunfish/Assets/Scripts/Managers/DevConfigOverride.cs` returns exactly 1 match.
   - Read the full file and confirm it still begins with `#if UNITY_EDITOR` on line 1 and ends with `#endif` on the last line, with no additional `#if`/`#endif` pairs introduced (file should have exactly one `#if` and one `#endif`, both unchanged in position other than line-count shift from the new additions).

2. **GameManager.cs property mirrors `debug`**:
   - `grep -n "public bool ignoreRequiredPlayerCount" Gunfish/Assets/Scripts/Managers/GameManager.cs` returns exactly 1 match.
   - `grep -n "_ignoreRequiredPlayerCount" Gunfish/Assets/Scripts/Managers/GameManager.cs` returns exactly 3 matches: the field declaration, the `#if UNITY_EDITOR`-guarded override check line (via `DevConfigOverride.TryGetIgnoreRequiredPlayerCount`), and the `return _ignoreRequiredPlayerCount;` fallback.
   - Read the `ignoreRequiredPlayerCount` property block and confirm it has a matched `#if UNITY_EDITOR` / `#endif` pair inside the getter (same shape as the `debug` property at lines 23-30), and that the property's braces balance (one `get { ... }` with matching braces, wrapped in the property's own braces).
   - `grep -c "#if UNITY_EDITOR" Gunfish/Assets/Scripts/Managers/GameManager.cs` equals `grep -c "#endif" Gunfish/Assets/Scripts/Managers/GameManager.cs` (balanced guards across the whole file after the edit; expect count to have increased from 2 to 3 pairs — one for `debug`, one for `debugPlayerCount`/`GameModeList`/`GunfishDataList` getters plus `RegisterDevConfigOverrideDebugEntries`, one new for `ignoreRequiredPlayerCount` — exact pre-edit baseline should be established by running the same grep before editing and confirming pair-count increases by exactly 1).
   - `grep -n "DevConfigOverride.IgnoreRequiredPlayerCount" Gunfish/Assets/Scripts/Managers/GameManager.cs` returns exactly 2 matches: one in the `ignoreRequiredPlayerCount` getter, one in `RegisterDevConfigOverrideDebugEntries()`.
   - `grep -n "DebugRegistrar.Track(\"DevConfigOverride.IgnoreRequiredPlayerCount\"" Gunfish/Assets/Scripts/Managers/GameManager.cs` returns exactly 1 match, located between the `RegisterDevConfigOverrideDebugEntries()` opening brace and its closing `#endif`.

3. **FishSelectMenuPage.cs does not reference `DevConfigOverride` or add `#if UNITY_EDITOR`**:
   - `grep -n "DevConfigOverride" Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs` returns 0 matches.
   - `grep -n "UNITY_EDITOR" Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs` returns 0 matches.
   - `grep -n "GameManager.Instance.ignoreRequiredPlayerCount" Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs` returns exactly 2 matches: one in the `OnPageStart()` overlay-registration condition, one in `AllPlayersReady()`.

4. **`AllPlayersReady()` boolean composition is correct**:
   - Read `AllPlayersReady()` in `Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs` (post-edit, expect roughly lines 255-279) and confirm:
     - A line reads `bool debugCountAllowed = GameManager.Instance.debug == true && readyPlayerCount >= 1;`
     - A line reads `bool ignoreRequiredCountAllowed = GameManager.Instance.ignoreRequiredPlayerCount == true && readyPlayerCount >= 1;`
     - A line reads `bool bypassAllowed = debugCountAllowed || ignoreRequiredCountAllowed;`
     - The early-return guard condition reads `if (allowedPlayerCounts.Contains(readyPlayerCount) == false && !bypassAllowed) {`
     - The final return statement reads `return hasNoSelecting && (bypassAllowed || GameManager.Instance.currentGameMode.requiredPlayerCount.Contains(readyPlayerCount));`
   - `grep -n "debugCountAllowed" Gunfish/Assets/Scripts/UI/FishSelectMenuPage.cs` shows no remaining bare (non-`bypassAllowed`-composed) usage in the guard/return lines — i.e. `debugCountAllowed` only appears in its own declaration and in the `bypassAllowed` assignment, not standalone in the `if` or `return` lines.
   - The `gameModeNote.text` construction block (the `if (allowedPlayerCounts.Count == 1) { ... } else { ... }` loop near the top of `OnPageStart()`) is byte-for-byte unchanged from the pre-edit version.

5. **DevConfigWindow.cs toggle**:
   - `grep -n "IgnoreRequiredPlayerCountOverride" Gunfish/Assets/Scripts/Editor/DevConfigWindow.cs` returns exactly 2 matches (both inside the same `EditorGUILayout.Toggle(...)` assignment statement — get and set through the same expression).
   - Read the file and confirm the new toggle block sits inside the `using (new EditorGUI.DisabledScope(!DevConfigOverride.Enabled)) { ... }` block (i.e. before its closing brace, after the `Debug Player Count` `IntSlider` call).
   - Confirm the file's total `{`/`}` count is balanced (no stray brace introduced) — e.g. via a brace-count check (`grep -o "{" file | wc -l` equals `grep -o "}" file | wc -l`).

6. **Cross-file consistency**:
   - The exact string `"Gunfish.Dev.IgnoreRequiredPlayerCount"` appears in exactly one file (`DevConfigOverride.cs`) — confirm via `grep -rn "Gunfish.Dev.IgnoreRequiredPlayerCount" Gunfish/Assets/Scripts/` returning exactly 1 match total.
   - `grep -rn "IgnoreRequiredPlayerCount" Gunfish/Assets/Scripts/` returns matches only in `DevConfigOverride.cs`, `GameManager.cs`, `FishSelectMenuPage.cs`, and `DevConfigWindow.cs` — no other files reference the new symbol.
