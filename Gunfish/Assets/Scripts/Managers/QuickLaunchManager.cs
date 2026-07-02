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
