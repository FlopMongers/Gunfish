# Quick Launch Debug Tool Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an F4 in-game wizard (Controllers → Game Mode → Fish → Level) that performs a full clean restart into a chosen test configuration from anywhere in the game, gated behind `GameManager.debug`.

**Architecture:** A single new runtime singleton, `QuickLaunchManager` (`PersistentSingleton<T>`, self-bootstrapped like the existing `DebugRegistrar`), owns a 4-step state machine and renders it via a runtime-constructed `Canvas` + `TextMeshProUGUI` overlay — no new prefabs or scenes. Keyboard-only navigation via direct `Keyboard.current` polling, matching `DebugRegistrar`'s existing pattern. Submitting the wizard reuses the existing production match-start path (`GameManager.InitializeGame`), which gets one new optional parameter (`forcedLevels`) so a single chosen level can bypass the normal random level selection.

**Tech Stack:** Unity 6000.5.0f1, C#, Unity New Input System 1.7.0 (`Keyboard.current`), TextMeshPro 3.0.8, existing `Singleton`/`PersistentSingleton` pattern (`Assets/Scripts/Utils/`).

## Global Constraints

- This project has **no automated test suite** (Unity Test Framework is present but unused, per `CLAUDE.md`). There is no CLI command that compiles or runs the game. Every "verify" step in this plan is a **manual check**: open the project in the Unity Editor, confirm the Console shows zero compile errors, then enter Play Mode and observe the described behavior.
- **Never hand-author `.meta` files.** After each task that creates a new `.cs` file, the file will have no `.meta` file until the Unity Editor is opened once and imports it. This is expected — do not create `.meta` files by hand. The task steps note when a `.meta` file should appear and be staged before committing.
- Only use Unity's New Input System (`Keyboard.current`), never `Input.GetKey` — matches `DebugRegistrar`, `PauseManager`, `LifePreserver`.
- Only use TextMeshPro, never Unity's built-in `Text` component.
- `QuickLaunchManager` must be completely inert (no F4 response, no overlay) whenever `GameManager.Instance.debug == false`, mirroring `DebugRegistrar`'s existing F3 gating exactly.
- No new prefabs or scene edits — all UI is constructed at runtime in code, following `DebugRegistrar.EnsureOverlayCreated()`.

---

## File Structure

- **Modify** `Assets/Scripts/Managers/GameModeManager.cs` — `InitializeGameMode` gains an optional `forcedLevels` parameter.
- **Modify** `Assets/Scripts/Managers/GameManager.cs` — `InitializeGame` gains an optional `forcedLevels` parameter, threaded through to `GameModeManager.InitializeGameMode`.
- **Create** `Assets/Scripts/Managers/QuickLaunchManager.cs` — the entire feature: bootstrap, F4 toggle, 4-step wizard state machine, runtime overlay UI, submit sequence, remember-last-picks.

No other files are touched. No automated tests exist to create.

---

### Task 1: Thread `forcedLevels` through the match-start path

**Files:**
- Modify: `Assets/Scripts/Managers/GameModeManager.cs:18-19`
- Modify: `Assets/Scripts/Managers/GameManager.cs:121-126`

**Interfaces:**
- Consumes: nothing new.
- Produces: `GameManager.InitializeGame(List<string> forcedLevels = null)` — later consumed by `QuickLaunchManager.Submit()` in Task 3, called as `GameManager.Instance.InitializeGame(new List<string> { pickedLevelPath })`.

- [ ] **Step 1: Modify `GameModeManager.InitializeGameMode` to accept an optional forced level list**

In `Assets/Scripts/Managers/GameModeManager.cs`, replace:

```csharp
    public void InitializeGameMode(GameMode gameMode, List<Player> players) {
        levels = SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch);
```

with:

```csharp
    public void InitializeGameMode(GameMode gameMode, List<Player> players, List<string> forcedLevels = null) {
        levels = forcedLevels ?? SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch);
```

Everything else in the method is unchanged.

- [ ] **Step 2: Modify `GameManager.InitializeGame` to accept and forward the same parameter**

In `Assets/Scripts/Managers/GameManager.cs`, replace:

```csharp
    public void InitializeGame() {
        // Spawn match manager
        // Get all active players
        GameModeManager.Instance.InitializeGameMode(selectedGameMode, PlayerManager.Instance.Players);
        MusicManager.Instance.PlayTrackSet(TrackSetLabel.Gameplay);
    }
```

with:

```csharp
    public void InitializeGame(List<string> forcedLevels = null) {
        // Spawn match manager
        // Get all active players
        GameModeManager.Instance.InitializeGameMode(selectedGameMode, PlayerManager.Instance.Players, forcedLevels);
        MusicManager.Instance.PlayTrackSet(TrackSetLabel.Gameplay);
    }
```

- [ ] **Step 3: Verify no other call sites break**

There are exactly two existing call sites, both unaffected because the new parameter defaults to `null`:
- `Assets/Scripts/UI/FishSelectMenuPage.cs:210` — `GameManager.Instance.InitializeGame();`
- The `GameModeManager.InitializeGameMode` call inside `GameManager.InitializeGame` itself (just modified above).

Confirm this by searching the codebase for `InitializeGameMode(` and `.InitializeGame()` — both should show only the two sites above.

- [ ] **Step 4: Manual verify — compile check**

Open the project in the Unity Editor (or bring it to focus if already open) and check the Console window. Expected: zero compile errors.

- [ ] **Step 5: Manual verify — existing flow unaffected**

Press Play, go through the normal Main Menu → Game Mode Select → Fish Select flow, start a match. Expected: behaves exactly as before (random level selection via `SelectLevels`, using `roundsPerMatch` levels) — this is the `forcedLevels == null` path, unchanged from current behavior.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Managers/GameModeManager.cs Assets/Scripts/Managers/GameManager.cs
git commit -m "feat: allow InitializeGame to force a specific level list"
```

---

### Task 2: `QuickLaunchManager` skeleton — bootstrap, F4 toggle, placeholder overlay

**Files:**
- Create: `Assets/Scripts/Managers/QuickLaunchManager.cs`

**Interfaces:**
- Consumes: `GameManager.InstanceExists`, `GameManager.Instance.debug` (existing, from `GameManager.cs`).
- Produces: `QuickLaunchManager` class (a `PersistentSingleton<QuickLaunchManager>`) with private fields `wizardOpen`, `overlayCanvas`, `overlayText`, and private methods `OpenWizard()`, `CloseWizard()`, `EnsureOverlayCreated()` — all replaced/extended in Task 3, so their exact bodies here don't need to be preserved by Task 3, only their names and general roles.

- [ ] **Step 1: Create the skeleton file**

Create `Assets/Scripts/Managers/QuickLaunchManager.cs`:

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuickLaunchManager : PersistentSingleton<QuickLaunchManager> {
    private bool wizardOpen;
    private Canvas overlayCanvas;
    private TextMeshProUGUI overlayText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
        if (InstanceExists) return;
        var go = new GameObject(nameof(QuickLaunchManager));
        go.AddComponent<QuickLaunchManager>();
    }

    private void Update() {
        bool debugActive = GameManager.InstanceExists && GameManager.Instance.debug;
        if (!debugActive) {
            if (wizardOpen) CloseWizard();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.f4Key.wasPressedThisFrame) {
            if (wizardOpen) CloseWizard();
            else OpenWizard();
        }
    }

    private void OpenWizard() {
        wizardOpen = true;
        EnsureOverlayCreated();
        overlayCanvas.gameObject.SetActive(true);
        overlayText.text = "<b>QUICK LAUNCH</b>  (F4 to close)\n(wizard steps not yet implemented)";
    }

    private void CloseWizard() {
        wizardOpen = false;
        if (overlayCanvas != null) overlayCanvas.gameObject.SetActive(false);
    }

    private void EnsureOverlayCreated() {
        if (overlayCanvas != null) return;

        var canvasGO = new GameObject("QuickLaunchOverlayCanvas");
        canvasGO.transform.SetParent(transform, false);
        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = short.MaxValue - 1;
        canvasGO.AddComponent<CanvasScaler>();

        var textGO = new GameObject("QuickLaunchOverlayText");
        textGO.transform.SetParent(canvasGO.transform, false);
        overlayText = textGO.AddComponent<TextMeshProUGUI>();
        overlayText.font = TMP_Settings.defaultFontAsset;
        overlayText.fontSize = 22;
        overlayText.color = Color.cyan;
        overlayText.alignment = TextAlignmentOptions.TopLeft;
        overlayText.raycastTarget = false;
        overlayText.enableWordWrapping = true;

        var rt = overlayText.rectTransform;
        rt.anchorMin = new Vector2(0.45f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(16f, 16f);
        rt.offsetMax = new Vector2(-16f, -16f);
    }
}
```

Note the overlay is anchored to the right half of the screen (`anchorMin.x = 0.45`) so it doesn't overlap `DebugRegistrar`'s F3 overlay, which occupies the left 55%.

- [ ] **Step 2: Let Unity generate the `.meta` file**

Open (or focus) the Unity Editor and let it import the new script. A `QuickLaunchManager.cs.meta` file will appear next to it. Do not create this file by hand.

- [ ] **Step 3: Manual verify — compile check**

Check the Console window. Expected: zero compile errors.

- [ ] **Step 4: Manual verify — F4 gating**

Press Play. With `GameManager.debug` false (default/production value), press F4 — expected: nothing happens, no overlay. Enable debug mode (via the `Tools > Gunfish > Dev Config` window, checking "Enable Dev Overrides" and "Debug"), press F4 again — expected: the placeholder overlay appears on the right side of the screen; press F4 again — expected: it disappears.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Managers/QuickLaunchManager.cs Assets/Scripts/Managers/QuickLaunchManager.cs.meta
git commit -m "feat: add QuickLaunchManager skeleton with F4-gated overlay"
```

---

### Task 3: Full 4-step wizard — navigation, rendering, submit, remember-last-picks

**Files:**
- Modify: `Assets/Scripts/Managers/QuickLaunchManager.cs` (full rewrite of the file created in Task 2)

**Interfaces:**
- Consumes:
  - `GameManager.Instance.GameModeList.gameModes` (`List<GameMode>`), `.GunfishDataList.gunfishes` (`List<GunfishData>`), `.debug` (`bool`) — from `Assets/Scripts/Managers/GameManager.cs`.
  - `GameManager.Instance.SetSelectedGameMode(GameMode)`, `.ResetGame()`, `.InitializeGame(List<string> forcedLevels = null)` (the last from Task 1) — from `GameManager.cs`.
  - `GameModeManager.InstanceExists`, `GameModeManager.Instance.matchManagerInstance` (`IMatchManager`, null when no match running) — from `Assets/Scripts/Managers/GameModeManager.cs`.
  - `PlayerManager.InstanceExists`, `PlayerManager.Instance.PlayerInputs` (`List<PlayerInput>`), `.SetPlayerFish(int playerIndex, GunfishData data)` — from `Assets/Scripts/Managers/PlayerManager.cs`. Note `SetPlayerFish` already sets `Players[playerIndex].Active = data != null`, so passing `null` deactivates a player and passing a fish activates them — this is how the Controllers step's active/inactive toggle takes effect.
  - `GameMode.levels` (`SceneList`), `GameMode.gameModeType` (`GameModeType` enum), `GameMode.name` (inherited from `ScriptableObject`/`UnityEngine.Object`) — from `Assets/Scripts/ScriptableObjects/GameMode.cs`.
  - `SceneList.sceneNames` (`List<string>`, full asset paths) — from `Assets/Scripts/ScriptableObjects/SceneList.cs`.
  - `GunfishData.name` (inherited) — from `Assets/Scripts/Player/Gunfish/Fish/GunfishData.cs`.
- Produces: no new public API — this task is self-contained within `QuickLaunchManager`.

**Control scheme (implemented exactly as follows — do not deviate):**

| Key | Effect |
|---|---|
| F4 | Toggle wizard open/closed. Opening resets to the Controllers step, pre-filled with the last submitted picks (if any). |
| Escape | Close the wizard without submitting. In-progress edits are discarded; the last *submitted* picks are unaffected. |
| Backspace | Go back one step (no-op on the Controllers step). |
| Up / Down | Move the highlighted row within the current step's list. |
| Enter / Space | Controllers step: toggle the highlighted controller active/inactive. Game Mode step: pick the highlighted mode and advance to Fish. Fish step: advance to Level (always valid — see below). Level step: pick the highlighted level **and submit**. |
| Left / Right (Fish step only) | Cycle the highlighted controller's fish choice backward/forward. |
| Right (Controllers step only) | Advance to Game Mode step (only if ≥1 controller is active). |

The Fish step is always valid to advance from because entering it (via picking a Game Mode) auto-assigns every active controller a fish — either its remembered choice from the last submission, or `gunfishes[0]` as a default, matching the same default-to-first-fish behavior `FishSelectMenuPage` already uses.

- [ ] **Step 1: Replace the entire contents of `QuickLaunchManager.cs`**

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuickLaunchManager : PersistentSingleton<QuickLaunchManager> {
    private enum Step { Controllers, GameMode, Fish, Level }

    private bool wizardOpen;
    private Step currentStep;
    private int cursorIndex;

    private HashSet<int> activeControllers = new();
    private GameMode pickedGameMode;
    private Dictionary<int, GunfishData> fishByController = new();
    private string pickedLevelPath;

    private HashSet<int> lastActiveControllers = new();
    private GameMode lastGameMode;
    private Dictionary<int, GunfishData> lastFishByController = new();
    private string lastLevelPath;
    private bool hasLastSubmission;

    private Canvas overlayCanvas;
    private TextMeshProUGUI overlayText;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
        if (InstanceExists) return;
        var go = new GameObject(nameof(QuickLaunchManager));
        go.AddComponent<QuickLaunchManager>();
    }

    private void Update() {
        bool debugActive = GameManager.InstanceExists && GameManager.Instance.debug;
        if (!debugActive) {
            if (wizardOpen) CloseWizard();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.f4Key.wasPressedThisFrame) {
            if (wizardOpen) CloseWizard();
            else OpenWizard();
            return;
        }

        if (!wizardOpen) return;

        if (keyboard.escapeKey.wasPressedThisFrame) {
            CloseWizard();
            return;
        }

        if (keyboard.backspaceKey.wasPressedThisFrame) {
            StepBack();
        } else if (keyboard.upArrowKey.wasPressedThisFrame) {
            MoveCursor(-1);
        } else if (keyboard.downArrowKey.wasPressedThisFrame) {
            MoveCursor(1);
        } else if (currentStep == Step.Fish && keyboard.leftArrowKey.wasPressedThisFrame) {
            CycleFish(-1);
        } else if (currentStep == Step.Fish && keyboard.rightArrowKey.wasPressedThisFrame) {
            CycleFish(1);
        } else if (currentStep == Step.Controllers && keyboard.rightArrowKey.wasPressedThisFrame) {
            AdvanceFromControllers();
        } else if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) {
            HandleConfirm();
        }

        RefreshOverlayText();
    }

    private void OpenWizard() {
        wizardOpen = true;
        currentStep = Step.Controllers;
        cursorIndex = 0;
        activeControllers = hasLastSubmission
            ? new HashSet<int>(lastActiveControllers.Where(IsValidControllerIndex))
            : new HashSet<int>();
        pickedGameMode = hasLastSubmission ? lastGameMode : null;
        fishByController = hasLastSubmission
            ? new Dictionary<int, GunfishData>(lastFishByController)
            : new Dictionary<int, GunfishData>();
        pickedLevelPath = hasLastSubmission ? lastLevelPath : null;
        EnsureOverlayCreated();
        overlayCanvas.gameObject.SetActive(true);
        RefreshOverlayText();
    }

    private void CloseWizard() {
        wizardOpen = false;
        if (overlayCanvas != null) overlayCanvas.gameObject.SetActive(false);
    }

    private bool IsValidControllerIndex(int index) {
        return PlayerManager.InstanceExists && index >= 0 && index < PlayerManager.Instance.PlayerInputs.Count;
    }

    private int ControllerCount() {
        return PlayerManager.InstanceExists ? PlayerManager.Instance.PlayerInputs.Count : 0;
    }

    private void MoveCursor(int delta) {
        int count = currentStep switch {
            Step.Controllers => ControllerCount(),
            Step.GameMode => GameManager.InstanceExists && GameManager.Instance.GameModeList != null
                ? GameManager.Instance.GameModeList.gameModes.Count : 0,
            Step.Fish => activeControllers.Count,
            Step.Level => pickedGameMode != null && pickedGameMode.levels != null
                ? pickedGameMode.levels.sceneNames.Count : 0,
            _ => 0
        };
        if (count <= 0) { cursorIndex = 0; return; }
        cursorIndex = ((cursorIndex + delta) % count + count) % count;
    }

    private void HandleConfirm() {
        switch (currentStep) {
            case Step.Controllers: ToggleControllerAtCursor(); break;
            case Step.GameMode: PickGameModeAtCursor(); break;
            case Step.Fish: AdvanceFromFish(); break;
            case Step.Level: PickLevelAtCursorAndSubmit(); break;
        }
    }

    private void ToggleControllerAtCursor() {
        if (!IsValidControllerIndex(cursorIndex)) return;
        if (!activeControllers.Remove(cursorIndex)) {
            activeControllers.Add(cursorIndex);
        }
    }

    private void AdvanceFromControllers() {
        if (activeControllers.Count == 0) return;
        currentStep = Step.GameMode;
        if (GameManager.InstanceExists && GameManager.Instance.GameModeList != null && pickedGameMode != null) {
            int existingIndex = GameManager.Instance.GameModeList.gameModes.IndexOf(pickedGameMode);
            cursorIndex = existingIndex >= 0 ? existingIndex : 0;
        } else {
            cursorIndex = 0;
        }
    }

    private void PickGameModeAtCursor() {
        if (!GameManager.InstanceExists || GameManager.Instance.GameModeList == null) return;
        var modes = GameManager.Instance.GameModeList.gameModes;
        if (cursorIndex < 0 || cursorIndex >= modes.Count) return;
        pickedGameMode = modes[cursorIndex];

        if (GameManager.Instance.GunfishDataList != null && GameManager.Instance.GunfishDataList.gunfishes.Count > 0) {
            var defaultFish = GameManager.Instance.GunfishDataList.gunfishes[0];
            var updated = new Dictionary<int, GunfishData>();
            foreach (var index in activeControllers) {
                updated[index] = fishByController.TryGetValue(index, out var existing) && existing != null
                    ? existing : defaultFish;
            }
            fishByController = updated;
        }

        currentStep = Step.Fish;
        cursorIndex = 0;
    }

    private void CycleFish(int delta) {
        if (!GameManager.InstanceExists || GameManager.Instance.GunfishDataList == null) return;
        var fishes = GameManager.Instance.GunfishDataList.gunfishes;
        if (fishes.Count == 0) return;
        var orderedControllers = activeControllers.OrderBy(index => index).ToList();
        if (cursorIndex < 0 || cursorIndex >= orderedControllers.Count) return;
        int controllerIndex = orderedControllers[cursorIndex];
        var current = fishByController.TryGetValue(controllerIndex, out var existing) ? existing : fishes[0];
        int fishIndex = fishes.IndexOf(current);
        if (fishIndex < 0) fishIndex = 0;
        fishIndex = ((fishIndex + delta) % fishes.Count + fishes.Count) % fishes.Count;
        fishByController[controllerIndex] = fishes[fishIndex];
    }

    private void AdvanceFromFish() {
        if (!GameManager.InstanceExists || GameManager.Instance.GunfishDataList == null
            || GameManager.Instance.GunfishDataList.gunfishes.Count == 0) return;
        if (pickedGameMode == null || pickedGameMode.levels == null || pickedGameMode.levels.sceneNames.Count == 0) return;

        currentStep = Step.Level;
        int existingIndex = pickedLevelPath != null ? pickedGameMode.levels.sceneNames.IndexOf(pickedLevelPath) : -1;
        cursorIndex = existingIndex >= 0 ? existingIndex : 0;
    }

    private void PickLevelAtCursorAndSubmit() {
        if (pickedGameMode == null || pickedGameMode.levels == null) return;
        var scenes = pickedGameMode.levels.sceneNames;
        if (cursorIndex < 0 || cursorIndex >= scenes.Count) return;
        pickedLevelPath = scenes[cursorIndex];
        Submit();
    }

    private void StepBack() {
        switch (currentStep) {
            case Step.GameMode: currentStep = Step.Controllers; break;
            case Step.Fish: currentStep = Step.GameMode; break;
            case Step.Level: currentStep = Step.Fish; break;
        }
        cursorIndex = 0;
    }

    private void Submit() {
        if (activeControllers.Count == 0 || pickedGameMode == null || string.IsNullOrEmpty(pickedLevelPath)) return;
        if (!PlayerManager.InstanceExists || !GameManager.InstanceExists) return;

        if (GameModeManager.InstanceExists && GameModeManager.Instance.matchManagerInstance != null) {
            GameManager.Instance.ResetGame();
        }

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            var fish = activeControllers.Contains(i) && fishByController.TryGetValue(i, out var chosen) ? chosen : null;
            PlayerManager.Instance.SetPlayerFish(i, fish);
        }

        GameManager.Instance.SetSelectedGameMode(pickedGameMode);
        GameManager.Instance.InitializeGame(new List<string> { pickedLevelPath });

        lastActiveControllers = new HashSet<int>(activeControllers);
        lastGameMode = pickedGameMode;
        lastFishByController = new Dictionary<int, GunfishData>(fishByController);
        lastLevelPath = pickedLevelPath;
        hasLastSubmission = true;

        CloseWizard();
    }

    private void EnsureOverlayCreated() {
        if (overlayCanvas != null) return;

        var canvasGO = new GameObject("QuickLaunchOverlayCanvas");
        canvasGO.transform.SetParent(transform, false);
        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = short.MaxValue - 1;
        canvasGO.AddComponent<CanvasScaler>();

        var textGO = new GameObject("QuickLaunchOverlayText");
        textGO.transform.SetParent(canvasGO.transform, false);
        overlayText = textGO.AddComponent<TextMeshProUGUI>();
        overlayText.font = TMP_Settings.defaultFontAsset;
        overlayText.fontSize = 22;
        overlayText.color = Color.cyan;
        overlayText.alignment = TextAlignmentOptions.TopLeft;
        overlayText.raycastTarget = false;
        overlayText.enableWordWrapping = true;

        var rt = overlayText.rectTransform;
        rt.anchorMin = new Vector2(0.45f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(16f, 16f);
        rt.offsetMax = new Vector2(-16f, -16f);
    }

    private void RefreshOverlayText() {
        var sb = new StringBuilder();
        sb.AppendLine("<b>QUICK LAUNCH</b>  (F4 to close, Esc to cancel)");
        sb.AppendLine("--------------------------------");
        AppendBreadcrumbs(sb);
        sb.AppendLine();

        switch (currentStep) {
            case Step.Controllers: RenderControllersStep(sb); break;
            case Step.GameMode: RenderGameModeStep(sb); break;
            case Step.Fish: RenderFishStep(sb); break;
            case Step.Level: RenderLevelStep(sb); break;
        }

        overlayText.text = sb.ToString();
    }

    private void AppendBreadcrumbs(StringBuilder sb) {
        if (currentStep > Step.Controllers) {
            sb.AppendLine($"Controllers: {activeControllers.Count} active");
        }
        if (currentStep > Step.GameMode && pickedGameMode != null) {
            sb.AppendLine($"Game Mode: {pickedGameMode.name}");
        }
        if (currentStep > Step.Fish) {
            sb.AppendLine($"Fish: {fishByController.Count} assigned");
        }
    }

    private void RenderControllersStep(StringBuilder sb) {
        sb.AppendLine("Step 1/4 - Controllers  (Up/Down move, Enter toggle, Right advance)");
        if (!PlayerManager.InstanceExists || PlayerManager.Instance.PlayerInputs.Count == 0) {
            sb.AppendLine("(no players connected)");
            return;
        }
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            string cursor = i == cursorIndex ? ">" : " ";
            string check = activeControllers.Contains(i) ? "[x]" : "[ ]";
            string device = PlayerManager.Instance.PlayerInputs[i].devices.Count > 0
                ? PlayerManager.Instance.PlayerInputs[i].devices[0].displayName
                : "Unknown Device";
            sb.AppendLine($"{cursor} {check} Player {i + 1} ({device})");
        }
        if (activeControllers.Count == 0) {
            sb.AppendLine("Select at least 1 controller to advance.");
        }
    }

    private void RenderGameModeStep(StringBuilder sb) {
        sb.AppendLine("Step 2/4 - Game Mode  (Up/Down move, Enter pick)");
        if (!GameManager.InstanceExists || GameManager.Instance.GameModeList == null
            || GameManager.Instance.GameModeList.gameModes.Count == 0) {
            sb.AppendLine("(no game modes configured)");
            return;
        }
        var modes = GameManager.Instance.GameModeList.gameModes;
        for (int i = 0; i < modes.Count; i++) {
            string cursor = i == cursorIndex ? ">" : " ";
            sb.AppendLine($"{cursor} {modes[i].name} ({modes[i].gameModeType})");
        }
    }

    private void RenderFishStep(StringBuilder sb) {
        sb.AppendLine("Step 3/4 - Fish  (Up/Down move, Left/Right cycle fish, Enter advance)");
        if (!GameManager.InstanceExists || GameManager.Instance.GunfishDataList == null
            || GameManager.Instance.GunfishDataList.gunfishes.Count == 0) {
            sb.AppendLine("(no fish configured)");
            return;
        }
        var orderedControllers = activeControllers.OrderBy(index => index).ToList();
        for (int i = 0; i < orderedControllers.Count; i++) {
            int controllerIndex = orderedControllers[i];
            string cursor = i == cursorIndex ? ">" : " ";
            string fishName = fishByController.TryGetValue(controllerIndex, out var fish) && fish != null
                ? fish.name : "(none)";
            sb.AppendLine($"{cursor} Player {controllerIndex + 1}: {fishName}");
        }
    }

    private void RenderLevelStep(StringBuilder sb) {
        sb.AppendLine("Step 4/4 - Level  (Up/Down move, Enter SUBMIT)");
        if (pickedGameMode == null || pickedGameMode.levels == null || pickedGameMode.levels.sceneNames.Count == 0) {
            sb.AppendLine("(this game mode has no levels)");
            return;
        }
        var scenes = pickedGameMode.levels.sceneNames;
        for (int i = 0; i < scenes.Count; i++) {
            string cursor = i == cursorIndex ? ">" : " ";
            sb.AppendLine($"{cursor} {System.IO.Path.GetFileNameWithoutExtension(scenes[i])}");
        }
    }
}
```

- [ ] **Step 2: Manual verify — compile check**

Check the Console window. Expected: zero compile errors.

- [ ] **Step 3: Manual verify — Controllers step, no players connected**

Press Play, enable debug mode (Dev Config window), press F4 **before** any controller has joined. Expected: Controllers step shows "(no players connected)"; Right arrow does nothing (can't advance).

- [ ] **Step 4: Manual verify — full walkthrough**

Let controllers join normally (per the existing boot flow), press F4. Walk all 4 steps: toggle at least one controller active (Enter), advance (Right), pick a game mode (Enter — this auto-advances to Fish), optionally cycle a fish with Left/Right, advance (Enter), pick a level (Enter — this submits). Expected: the wizard closes and the game restarts directly into the chosen level with the chosen fish for each active controller, playing exactly one round (no automatic continuation into a second randomly-picked level).

- [ ] **Step 5: Manual verify — Backspace navigation**

Open F4 again, advance to the Fish step, press Backspace twice (back to Controllers). Expected: each Backspace moves back exactly one step; the previously toggled controllers/picked game mode are still remembered when moving forward again in the same open session.

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Managers/QuickLaunchManager.cs
git commit -m "feat: implement Quick Launch 4-step wizard with submit and remember-last-picks"
```

---

### Task 4: Manual QA pass against the full spec

**Files:** none (verification only; fix forward in this same task if a check fails).

**Interfaces:** none.

This task runs the complete manual test list from the design spec (`docs/superpowers/specs/2026-07-01-quick-launch-debug-tool-design.md`) end-to-end, including the two checks that need a running match already in progress (which Tasks 2–3 didn't set up).

- [ ] **Step 1: Debug-off safety**

With `GameManager.debug == false` (Dev Config window's "Enable Dev Overrides" unchecked, or debug toggle off), press F4 repeatedly during normal play. Expected: nothing happens at all — no overlay, no state changes. This must match `DebugRegistrar`'s F3 behavior under the same condition.

- [ ] **Step 2: Mid-match relaunch**

Start a normal match (or use Quick Launch once to get into one). While mid-match, press F4, pick a **different** game mode, fish, and level than currently running, submit. Expected: the running match tears down cleanly (check the Console for the existing `"Tearing down Gamemode"` log from `GameModeManager.TeardownGameMode`, and confirm no errors), then the new configuration starts fresh with no leftover state from the old match.

- [ ] **Step 3: Controller exclusion**

Open F4, toggle one connected controller's checkbox off (leaving at least one other on), walk through and submit. Expected: the excluded controller's player does not spawn and does not participate in the match, same as if they'd been left `Inactive` in the normal Fish Select screen.

- [ ] **Step 4: Remember-last-picks**

Immediately after a submit, press F4 again. Expected: all 4 steps are pre-filled with the exact picks from the last submit (same active controllers checked, same game mode highlighted, same fish per controller, cursor on the same previously-picked level) — pressing Right/Enter through all 4 steps without changing anything re-launches the identical configuration.

- [ ] **Step 5: Empty-config guards**

Temporarily clear the `GunfishDataList` field on the `GameManager` instance in the Inspector (do not save/commit this), open F4, advance to the Fish step. Expected: "(no fish configured)" is shown and advancing is blocked. Undo the change (revert the Inspector field) afterward — do not commit this temporary edit.

- [ ] **Step 6: No regressions to the existing debug tools**

Confirm the F3 `DebugRegistrar` overlay still works independently (toggles correctly, doesn't visually collide with the F4 overlay since they're anchored to opposite halves of the screen), and that the Dev Config window (`Tools > Gunfish > Dev Config`) still functions as before.

- [ ] **Step 7: Final commit (only if Step 5 required a code fix)**

If any of the above steps required a code change to pass, commit it now with a message describing the specific bug fixed. If all steps passed as implemented, there is nothing to commit for this task.
