using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MenuUIThemeApplier {
    private static readonly HashSet<string> ChromeAllowlist = new HashSet<string> {
        "InnerPanel", "Outline", "TopPanel", "MiddlePanel", "BottomPanel", "DetailsPanel"
    };

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
}
