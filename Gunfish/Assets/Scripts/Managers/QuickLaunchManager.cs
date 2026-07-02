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
