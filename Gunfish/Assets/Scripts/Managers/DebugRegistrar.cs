using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebugRegistrar : PersistentSingleton<DebugRegistrar> {
    private readonly Dictionary<string, Func<string>> trackedValues = new();
    private readonly List<string> registrationOrder = new();

    private Canvas overlayCanvas;
    private TextMeshProUGUI overlayText;
    private bool overlayVisible;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
        if (InstanceExists) return;
        var go = new GameObject(nameof(DebugRegistrar));
        go.AddComponent<DebugRegistrar>();
    }

    public static void Track(string label, Func<string> valueGetter) {
        if (!InstanceExists || string.IsNullOrEmpty(label) || valueGetter == null) return;
        if (!Instance.trackedValues.ContainsKey(label)) {
            Instance.registrationOrder.Add(label);
        }
        Instance.trackedValues[label] = valueGetter;
    }

    public static void Untrack(string label) {
        if (!InstanceExists || string.IsNullOrEmpty(label)) return;
        Instance.trackedValues.Remove(label);
        Instance.registrationOrder.Remove(label);
    }

    private void Update() {
        bool debugActive = GameManager.InstanceExists && GameManager.Instance.debug;
        if (!debugActive) {
            if (overlayVisible) SetOverlayVisible(false);
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.f3Key.wasPressedThisFrame) {
            SetOverlayVisible(!overlayVisible);
        }

        if (keyboard != null && keyboard.nKey.wasPressedThisFrame) {
            GameModeManager.Instance?.matchManagerInstance?.OnTimerFinish();
        }

        if (overlayVisible) {
            RefreshOverlayText();
        }
    }

    private void SetOverlayVisible(bool visible) {
        overlayVisible = visible;
        if (visible) EnsureOverlayCreated();
        if (overlayCanvas != null) overlayCanvas.gameObject.SetActive(visible);
    }

    private void EnsureOverlayCreated() {
        if (overlayCanvas != null) return;

        var canvasGO = new GameObject("DebugRegistrarOverlayCanvas");
        canvasGO.transform.SetParent(transform, false);
        overlayCanvas = canvasGO.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = short.MaxValue;
        canvasGO.AddComponent<CanvasScaler>();

        var textGO = new GameObject("DebugRegistrarOverlayText");
        textGO.transform.SetParent(canvasGO.transform, false);
        overlayText = textGO.AddComponent<TextMeshProUGUI>();
        overlayText.font = TMP_Settings.defaultFontAsset;
        overlayText.fontSize = 22;
        overlayText.color = Color.green;
        overlayText.alignment = TextAlignmentOptions.TopLeft;
        overlayText.raycastTarget = false;
        overlayText.textWrappingMode = TextWrappingModes.Normal;

        var rt = overlayText.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0.55f, 1f);
        rt.offsetMin = new Vector2(16f, 16f);
        rt.offsetMax = new Vector2(-16f, -16f);
    }

    private void RefreshOverlayText() {
        var sb = new StringBuilder();
        sb.AppendLine("<b>DEBUG REGISTRAR</b>  (F3 to hide)");
        sb.AppendLine($"GameManager.debug=true  debugPlayerCount={GameManager.Instance.debugPlayerCount}");
        sb.AppendLine("--------------------------------");
        if (registrationOrder.Count == 0) {
            sb.AppendLine("(no overrides currently registered)");
        }
        foreach (var label in registrationOrder) {
            string value;
            try {
                value = trackedValues[label]?.Invoke() ?? "<null>";
            } catch (Exception e) {
                value = $"<error: {e.Message}>";
            }
            sb.AppendLine($"{label}: {value}");
        }
        overlayText.text = sb.ToString();
    }
}
