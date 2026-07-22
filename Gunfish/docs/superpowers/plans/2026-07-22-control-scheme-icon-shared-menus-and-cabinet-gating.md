# Control Scheme Icon — Shared Menus & Cabinet Gating — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `ControlSchemeIcon.IconSource.LastActiveDevice`, backed by a new
`ActiveControlSchemeTracker` singleton, for shared/global menu screens with no single
per-icon `Player`; and rework `InputGlyphDatabase.GetSprite` to gate glyph-set
selection on `PlatformConfig.IsCabinet` instead of control-scheme-string identity, since
cabinet hardware may not reliably report as `"Joystick"`.

**Architecture:** See the companion design doc,
`2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md`.

## Global Constraints

- No automated test suite (confirmed in `CLAUDE.md`) -- every task's verification is a
  `dotnet build` compile check plus a manual in-Editor check.
- Never hand-edit prefab or scene files. `MainMenuCanvas.prefab` and
  `FishHealthUI.prefab` are out of scope. `GameManager.prefab` requires one manual,
  Editor-driven addition (Task 3) -- never edit its YAML directly.
- `IconSource.Player` mode and `Gunfish.cs`'s `Init(player)` call site must remain
  behaviorally identical (modulo the cabinet-gating fix in `GetSprite`, which applies
  uniformly to both modes).

## File Structure

- **Create** `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs` (Task 1).
- **Modify** `Assets/Scripts/UI/ControlSchemeIcon.cs` -- add `IconSource` mode (Task 2).
- **Modify** `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs` -- cabinet-gated
  `GetSprite` (Task 4).
- **Modify** `Assets/Scripts/Managers/GameManager.cs` -- initialize the tracker
  (Task 1).
- **Manual Editor step** -- add `ActiveControlSchemeTracker` GameObject to
  `Assets/Resources/Prefabs/GameManager.prefab` (Task 3).

---

### Task 1: `ActiveControlSchemeTracker.cs` + `GameManager.cs` wiring

**Files:**
- Create: `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs`
- Modify: `Assets/Scripts/Managers/GameManager.cs`

- [ ] **Step 1:** Create `ActiveControlSchemeTracker.cs` with the exact contents from
  the design doc / pipeline-plan Implementation Steps section 1.
- [ ] **Step 2:** In `GameManager.InitializePostRitualManagersCR()`, add
  `ActiveControlSchemeTracker.Instance.Initialize();` as the first line after the
  `yield return new WaitForEndOfFrame();`.
- [ ] **Step 3: Compile-check.** Run `dotnet build Assembly-CSharp.csproj` from
  `Gunfish/`. Expected: `0 Error(s)`. (Expected at this point: the build still succeeds
  even though nothing yet places the GameObject in the scene -- that failure mode is a
  *runtime* null-reference, not a compile error, and is covered by Task 3's manual
  verification.)
- [ ] **Step 4: Commit.**
  ```bash
  git add Assets/Scripts/Managers/ActiveControlSchemeTracker.cs Assets/Scripts/Managers/GameManager.cs
  git commit -m "feat: add ActiveControlSchemeTracker singleton for shared-menu control scheme icons"
  ```

---

### Task 2: `ControlSchemeIcon.cs` — dual-mode `IconSource`

**Files:**
- Modify: `Assets/Scripts/UI/ControlSchemeIcon.cs`

- [ ] **Step 1:** Replace the file with the exact contents from the pipeline-plan
  Implementation Steps section 2.
- [ ] **Step 2: Compile-check.** Run `dotnet build Assembly-CSharp.csproj`. Expected:
  `0 Error(s)`.
- [ ] **Step 3: Manual check -- `Player` mode unaffected.** Open
  `Assets/Resources/Prefabs/UI/FishHealthUI.prefab` (read-only inspection, do not save
  any change) and confirm its `ControlSchemeIcon` component still shows `Icon Source:
  Player` after this change (new field defaults correctly for pre-existing serialized
  data). Play the game with at least one joined player; confirm the fish HUD's control
  scheme icon still updates correctly on control-scheme change, exactly as before.
- [ ] **Step 4: Commit.**
  ```bash
  git add Assets/Scripts/UI/ControlSchemeIcon.cs
  git commit -m "feat: add ControlSchemeIcon.IconSource.LastActiveDevice for shared menu screens"
  ```

---

### Task 3: Manual Editor step — bootstrap the tracker in `GameManager.prefab`

**Files:**
- Manual Editor edit: `Assets/Resources/Prefabs/GameManager.prefab`

- [ ] **Step 1:** In the Unity Editor, open `GameManager.prefab` in Prefab Mode, add a
  new empty child GameObject named `ActiveControlSchemeTracker`, add the
  `ActiveControlSchemeTracker` component to it, and save the prefab. (See pipeline-plan
  Implementation Steps section 5 for the exact procedure.)
- [ ] **Step 2: Manual check -- singleton resolves at runtime.** Enter Play mode from
  `Assets/Scenes/MainMenu.unity` (or whichever scene bootstraps `GameManager.prefab`)
  and confirm no `NullReferenceException` is thrown referencing
  `ActiveControlSchemeTracker`. Add a temporary `Debug.Log(ActiveControlSchemeTracker.Instance.CurrentScheme)`
  if useful, confirm it logs `"Keyboard&Mouse"` before any input; press a key and
  confirm a `SchemeChanged` observer (or a temporary log in `OnAnyInput`) fires with the
  new scheme.
- [ ] **Step 3: Commit.**
  ```bash
  git add "Assets/Resources/Prefabs/GameManager.prefab"
  git commit -m "chore: bootstrap ActiveControlSchemeTracker alongside the other persistent managers"
  ```

---

### Task 4: `InputGlyphDatabase.cs` — cabinet-gated `GetSprite`

**Files:**
- Modify: `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs`

- [ ] **Step 1:** Replace `GetSprite` with the exact contents from the pipeline-plan
  Implementation Steps section 3.
- [ ] **Step 2: Compile-check.** Run `dotnet build Assembly-CSharp.csproj`. Expected:
  `0 Error(s)`.
- [ ] **Step 3: Manual check -- non-cabinet (default).** With
  `DevConfigOverride.Enabled` off (or set to a non-`ArcadeCabinet` simulated target),
  confirm `PlatformConfig.IsCabinet == false`, and that a keyboard player's icons show
  `keyboardGlyphs` sprites, a gamepad player's icons show `gamepadGlyphs` sprites, and
  no icon ever shows a `cabinetGlyphs` sprite.
- [ ] **Step 4: Manual check -- simulated cabinet.** Open `Tools > Gunfish > Dev
  Config`, enable dev overrides, and set the simulated build target to
  `ArcadeCabinet`. Confirm `PlatformConfig.IsCabinet == true` (e.g. via a temporary log
  or the Inspector debug view). With a keyboard connected in the Editor, confirm
  `Keyboard&Mouse`-scheme icons still show `keyboardGlyphs` (debug fallback) and that
  simulating any other reported scheme name resolves to `cabinetGlyphs`, except `Menu`,
  which resolves to the keyboard `Menu` glyph in both the `"Gamepad"`- and
  `"Joystick"`-reported cases. Revert the simulated build target back to `WindowsPC`
  (or disable dev overrides) when done.
- [ ] **Step 5: Manual check -- `Init(player)` call site.** Confirm
  `Assets/Scripts/Player/Gunfish/Fish/Gunfish.cs:387`
  (`controlSchemeIcon.Init(player);`) still compiles with no changes required, and that
  the fish HUD icon behaves identically off-cabinet to before this change.
- [ ] **Step 6: Commit.**
  ```bash
  git add Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs
  git commit -m "refactor: gate InputGlyphDatabase.GetSprite by PlatformConfig.IsCabinet instead of scheme-string identity"
  ```

---

### Task 5: Documentation

**Files:**
- Create: `docs/superpowers/specs/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md`
- Create: `docs/superpowers/plans/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating.md`

- [ ] **Step 1:** Create both files with the exact contents from the pipeline-plan
  Implementation Steps section 6.
- [ ] **Step 2: Commit.**
  ```bash
  git add docs/superpowers/specs/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md docs/superpowers/plans/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating.md
  git commit -m "docs: add design + plan for shared-menu control scheme icons and cabinet gating"
  ```

## Known limitations (carried forward, not introduced here)

- `MainMenuCanvas.prefab`'s two pre-existing `ControlSchemeIcon` components still have
  the stale `action` int values noted in the prior design doc's "Known limitation" --
  unaffected and unresolved by this plan. The user has stated they will manually flip
  these two components' `source` field to `LastActiveDevice` themselves once their
  in-progress prefab edits are done; this plan does not touch that prefab.
