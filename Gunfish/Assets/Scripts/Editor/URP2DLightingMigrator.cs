using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class URP2DLightingMigrator : Editor {
    [MenuItem("Tools/URP Migration/Convert Lighting In Open Scene")]
    public static void ConvertOpenScene() {
        var scene = EditorSceneManager.GetActiveScene();
        var report = new StringBuilder();

        foreach (var root in scene.GetRootGameObjects()) {
            ConvertHierarchy(root, report);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[URP2DLightingMigrator] Converted lighting in scene '{scene.name}':\n{report}");
    }

    [MenuItem("Tools/URP Migration/Convert Lighting In Selected Prefab")]
    public static void ConvertSelectedPrefab() {
        var go = Selection.activeGameObject;
        if (go == null) {
            Debug.LogError("[URP2DLightingMigrator] Select a prefab asset in the Project window first.");
            return;
        }

        var path = AssetDatabase.GetAssetPath(go);
        if (string.IsNullOrEmpty(path)) {
            Debug.LogError("[URP2DLightingMigrator] Selected object is not a prefab asset.");
            return;
        }

        var report = new StringBuilder();
        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path)) {
            ConvertHierarchy(editingScope.prefabContentsRoot, report);
        }

        Debug.Log($"[URP2DLightingMigrator] Converted lighting in prefab '{path}' (saved immediately, review via git diff):\n{report}");
    }

    private static void ConvertHierarchy(GameObject root, StringBuilder report) {
        foreach (var legacyLight in root.GetComponentsInChildren<FunkyCode.Light2D>(true)) {
            ConvertLight(legacyLight, report);
        }

        foreach (var legacyCollider in root.GetComponentsInChildren<FunkyCode.LightCollider2D>(true)) {
            ConvertCollider(legacyCollider, report);
        }
    }

    // Field mapping verified against Assets/Plugins/FunkyCode/SmartLighting2D/Components/Lightmap/Light2D.cs.
    // falloff and lightStrength don't have a guaranteed 1:1 scale against URP's equivalents -
    // flagged in the report for a visual pass, not assumed correct.
    private static void ConvertLight(FunkyCode.Light2D legacy, StringBuilder report) {
        var go = legacy.gameObject;

        var urpLight = go.GetComponent<Light2D>();
        if (urpLight == null) {
            urpLight = go.AddComponent<Light2D>();
        }

        urpLight.lightType = Light2D.LightType.Point;
        urpLight.color = legacy.color;
        urpLight.pointLightOuterRadius = legacy.size;
        urpLight.pointLightInnerRadius = legacy.coreSize;
        urpLight.falloffIntensity = Mathf.Clamp01(legacy.falloff);
        urpLight.intensity = legacy.lightStrength > 0f ? legacy.lightStrength : 1f;

        legacy.enabled = false;

        report.AppendLine($"  Light2D @ {GetPath(go)}: size {legacy.size} -> outerRadius, coreSize {legacy.coreSize} -> innerRadius, falloff {legacy.falloff} -> falloffIntensity (verify by eye), lightStrength {legacy.lightStrength} -> intensity (verify by eye)");
    }

    // Field mapping verified against Assets/Plugins/FunkyCode/SmartLighting2D/Components/LightCollider/LightCollider2D.cs.
    // ShadowCaster2D has no equivalent for MeshRenderer/SkinnedMeshRenderer shadow types - those are
    // logged for manual handling rather than silently skipped.
    private static void ConvertCollider(FunkyCode.LightCollider2D legacy, StringBuilder report) {
        var go = legacy.gameObject;
        var path = GetPath(go);

        if (legacy.shadowType == FunkyCode.LightCollider2D.ShadowType.None) {
            return;
        }

        if (legacy.shadowType == FunkyCode.LightCollider2D.ShadowType.MeshRenderer ||
            legacy.shadowType == FunkyCode.LightCollider2D.ShadowType.SkinnedMeshRenderer) {
            report.AppendLine($"  SKIPPED LightCollider2D @ {path}: shadowType={legacy.shadowType} has no ShadowCaster2D equivalent, handle manually.");
            return;
        }

        var shadowCaster = go.GetComponent<ShadowCaster2D>();
        if (shadowCaster == null) {
            shadowCaster = go.AddComponent<ShadowCaster2D>();
        }

        legacy.enabled = false;

        report.AppendLine($"  LightCollider2D @ {path}: shadowType={legacy.shadowType} -> added ShadowCaster2D (derives shape from sprite/collider, verify silhouette).");
    }

    [MenuItem("Tools/URP Migration/Remove Legacy SmartLighting2D Components In Open Scene")]
    public static void RemoveLegacyInOpenScene() {
        var scene = EditorSceneManager.GetActiveScene();
        int removed = 0;

        foreach (var root in scene.GetRootGameObjects()) {
            removed += RemoveDisabledLegacyComponents(root);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[URP2DLightingMigrator] Removed {removed} disabled legacy SmartLighting2D component(s) from scene '{scene.name}'.");
    }

    [MenuItem("Tools/URP Migration/Remove Legacy SmartLighting2D Components In Selected Prefab")]
    public static void RemoveLegacyInSelectedPrefab() {
        var go = Selection.activeGameObject;
        if (go == null) {
            Debug.LogError("[URP2DLightingMigrator] Select a prefab asset in the Project window first.");
            return;
        }

        var path = AssetDatabase.GetAssetPath(go);
        if (string.IsNullOrEmpty(path)) {
            Debug.LogError("[URP2DLightingMigrator] Selected object is not a prefab asset.");
            return;
        }

        int removed;
        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path)) {
            removed = RemoveDisabledLegacyComponents(editingScope.prefabContentsRoot);
        }

        Debug.Log($"[URP2DLightingMigrator] Removed {removed} disabled legacy SmartLighting2D component(s) from prefab '{path}'.");
    }

    // Only removes components already disabled by the conversion pass above - never touches
    // still-enabled legacy components, so running this can't silently delete un-migrated lighting.
    private static int RemoveDisabledLegacyComponents(GameObject root) {
        int removed = 0;

        foreach (var legacy in root.GetComponentsInChildren<FunkyCode.Light2D>(true)) {
            if (!legacy.enabled) {
                Object.DestroyImmediate(legacy, true);
                removed++;
            }
        }

        foreach (var legacy in root.GetComponentsInChildren<FunkyCode.LightCollider2D>(true)) {
            if (!legacy.enabled) {
                Object.DestroyImmediate(legacy, true);
                removed++;
            }
        }

        return removed;
    }

    private static string GetPath(GameObject go) {
        var path = go.name;
        var current = go.transform.parent;
        while (current != null) {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
}
