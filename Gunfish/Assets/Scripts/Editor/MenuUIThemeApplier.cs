using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MenuUIThemeApplier {
    private static readonly HashSet<string> ChromeAllowlist = new HashSet<string> {
        "InnerPanel", "Outline", "TopPanel", "MiddlePanel", "BottomPanel", "DetailsPanel"
    };

    private static readonly string[] PageRootNames = { "SplashPage", "GameModeSelectPage", "FishSelectPage" };
    private const string MainMenuCanvasPath = "Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab";

    [MenuItem("Tools/Gunfish/UI Theme/Convert Chrome In FishSelectPanel Prefab")]
    public static void ConvertFishSelectPanel() {
        ConvertChromeInPrefab("Assets/Resources/Prefabs/UI/Menu/FishSelectPanel.prefab");
    }

    [MenuItem("Tools/Gunfish/UI Theme/Convert Chrome In MainMenuCanvas Prefab")]
    public static void ConvertMainMenuCanvas() {
        ConvertChromeInPrefab("Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab");
    }

    public static void ConvertChromeInPrefab(string prefabPath) {
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try {
            int converted = 0, skipped = 0;
            var images = root.GetComponentsInChildren<Image>(true);
            foreach (var image in images) {
                var go = image.gameObject;
                if (!ChromeAllowlist.Contains(go.name)) {
                    skipped++;
                    continue;
                }
                if (image is UIPanel) {
                    skipped++;
                    continue; // already converted
                }

                Object.DestroyImmediate(image);
                var panel = go.AddComponent<UIPanel>();
                panel.color = UITheme.Surface700;
                panel.BorderColor = Color.clear;
                panel.CornerRadius = 16f;
                panel.BorderWidth = 0f;
                converted++;
                Debug.Log($"MenuUIThemeApplier: converted '{go.name}' in {prefabPath} to UIPanel.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
            if (!success) {
                Debug.LogError($"MenuUIThemeApplier: failed to save {prefabPath}");
            } else {
                Debug.Log($"MenuUIThemeApplier: {prefabPath} done. Converted {converted}, skipped {skipped}.");
            }
        } finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/Gunfish/UI Theme/Apply Page Title Spacing")]
    public static void ApplyPageSpacing() {
        var root = PrefabUtility.LoadPrefabContents(MainMenuCanvasPath);
        try {
            foreach (var pageName in PageRootNames) {
                var pageTransform = FindDeepChild(root.transform, pageName);
                if (pageTransform == null) {
                    Debug.LogError($"MenuUIThemeApplier: could not find page root '{pageName}' in {MainMenuCanvasPath}");
                    continue;
                }
                var pageGo = pageTransform.gameObject;
                var layout = pageGo.GetComponent<VerticalLayoutGroup>();
                if (layout == null) {
                    layout = pageGo.AddComponent<VerticalLayoutGroup>();
                }
                layout.spacing = UISpacing.LG;
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                Debug.Log($"MenuUIThemeApplier: applied VerticalLayoutGroup (spacing {UISpacing.LG}) to '{pageName}'.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, MainMenuCanvasPath, out bool success);
            if (!success) Debug.LogError($"MenuUIThemeApplier: failed to save {MainMenuCanvasPath}");
        } finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform FindDeepChild(Transform parent, string name) {
        if (parent.name == name) return parent;
        foreach (Transform child in parent) {
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    [MenuItem("Tools/Gunfish/UI Theme/Apply FishSelect Row Spacing")]
    public static void ApplyFishSelectRowSpacing() {
        var root = PrefabUtility.LoadPrefabContents(MainMenuCanvasPath);
        try {
            var panels = root.GetComponentsInChildren<FishSelectPanel>(true);
            if (panels.Length == 0) {
                Debug.LogError($"MenuUIThemeApplier: no FishSelectPanel instances found in {MainMenuCanvasPath}");
                return;
            }

            var parent = panels[0].transform.parent;
            foreach (var panel in panels) {
                if (panel.transform.parent != parent) {
                    Debug.LogError("MenuUIThemeApplier: FishSelectPanel instances don't share a common parent; skipping row spacing.");
                    return;
                }
            }

            var parentGo = parent.gameObject;
            var layout = parentGo.GetComponent<HorizontalLayoutGroup>();
            if (layout == null) {
                layout = parentGo.AddComponent<HorizontalLayoutGroup>();
            }
            layout.spacing = UISpacing.XL;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            Debug.Log($"MenuUIThemeApplier: applied HorizontalLayoutGroup (spacing {UISpacing.XL}) to '{parentGo.name}' ({panels.Length} panels).");

            PrefabUtility.SaveAsPrefabAsset(root, MainMenuCanvasPath, out bool success);
            if (!success) Debug.LogError($"MenuUIThemeApplier: failed to save {MainMenuCanvasPath}");
        } finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
