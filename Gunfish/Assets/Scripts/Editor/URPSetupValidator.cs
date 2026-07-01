using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class URPSetupValidator : Editor {
    [MenuItem("Tools/URP Migration/Validate Pipeline Setup")]
    public static void ValidatePipelineSetup() {
        var globalPipeline = GraphicsSettings.currentRenderPipeline;
        if (globalPipeline == null) {
            Debug.LogError("[URP Validator] Graphics Settings has no Render Pipeline Asset assigned (Edit > Project Settings > Graphics).");
        } else {
            Debug.Log($"[URP Validator] Graphics Settings pipeline: {globalPipeline.name}");
        }

        var qualityLevels = QualitySettings.names;
        var activeLevel = QualitySettings.GetQualityLevel();
        bool anyMissing = false;

        for (int i = 0; i < qualityLevels.Length; i++) {
            QualitySettings.SetQualityLevel(i, false);
            var pipeline = QualitySettings.renderPipeline;

            if (pipeline == null) {
                Debug.LogWarning($"[URP Validator] Quality level '{qualityLevels[i]}' has no Render Pipeline Asset override (falls back to Graphics Settings, which is fine if that's assigned).");
                anyMissing = anyMissing || globalPipeline == null;
            } else {
                Debug.Log($"[URP Validator] Quality level '{qualityLevels[i]}' pipeline: {pipeline.name}");
            }
        }

        QualitySettings.SetQualityLevel(activeLevel, false);

        if (globalPipeline == null && anyMissing) {
            Debug.LogError("[URP Validator] FAIL: no pipeline resolved for one or more quality levels.");
        } else {
            Debug.Log("[URP Validator] PASS: a Render Pipeline Asset resolves for every quality level.");
        }
    }
}
