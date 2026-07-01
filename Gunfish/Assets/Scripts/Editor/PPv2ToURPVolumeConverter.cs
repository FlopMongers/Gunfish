using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PPv2 = UnityEngine.Rendering.PostProcessing;

public class PPv2ToURPVolumeConverter : Editor {
    [MenuItem("Tools/URP Migration/Convert Selected PostProcessProfile To VolumeProfile")]
    public static void ConvertSelectedProfile() {
        var ppProfile = Selection.activeObject as PPv2.PostProcessProfile;
        if (ppProfile == null) {
            Debug.LogError("[PPv2ToURPVolumeConverter] Select a PostProcessProfile asset in the Project window first.");
            return;
        }

        var sourcePath = AssetDatabase.GetAssetPath(ppProfile);
        var destPath = Path.Combine(Path.GetDirectoryName(sourcePath), Path.GetFileNameWithoutExtension(sourcePath) + "_URP.asset").Replace('\\', '/');

        var volumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        var report = new StringBuilder();

        if (ppProfile.TryGetSettings(out PPv2.Vignette ppVignette) && ppVignette.active) {
            var vignette = volumeProfile.Add<Vignette>(true);
            vignette.color.Override(ppVignette.color.value);
            vignette.intensity.Override(ppVignette.intensity.value);
            vignette.smoothness.Override(ppVignette.smoothness.value);
            report.AppendLine($"  Vignette: intensity {ppVignette.intensity.value}");
        }

        // PPv2 diffusion (1-10) and URP scatter (0-1) measure different things (blur diffusion vs.
        // scatter amount) - this is a starting ballpark, not an equivalence. Retune by eye.
        if (ppProfile.TryGetSettings(out PPv2.Bloom ppBloom) && ppBloom.active) {
            var bloom = volumeProfile.Add<Bloom>(true);
            bloom.intensity.Override(ppBloom.intensity.value);
            bloom.scatter.Override(Mathf.Clamp01(ppBloom.diffusion.value / 10f));
            if (ppBloom.dirtTexture.value != null) {
                bloom.dirtTexture.Override(ppBloom.dirtTexture.value);
                bloom.dirtIntensity.Override(ppBloom.dirtIntensity.value);
            }
            report.AppendLine($"  Bloom: intensity {ppBloom.intensity.value}, diffusion {ppBloom.diffusion.value} -> scatter (retune by eye)");
        }

        if (ppProfile.TryGetSettings(out PPv2.ColorGrading ppColorGrading) && ppColorGrading.active) {
            var colorAdjustments = volumeProfile.Add<ColorAdjustments>(true);
            colorAdjustments.hueShift.Override(ppColorGrading.hueShift.value);
            colorAdjustments.saturation.Override(ppColorGrading.saturation.value);
            colorAdjustments.contrast.Override(ppColorGrading.contrast.value);
            report.AppendLine($"  Color Grading -> Color Adjustments: hueShift {ppColorGrading.hueShift.value}, saturation {ppColorGrading.saturation.value}, contrast {ppColorGrading.contrast.value}");
        }

        // PPv2 intensity is a -100..100 percentage; URP intensity is -1..1.
        if (ppProfile.TryGetSettings(out PPv2.LensDistortion ppLensDistortion) && ppLensDistortion.active) {
            var lensDistortion = volumeProfile.Add<LensDistortion>(true);
            lensDistortion.intensity.Override(ppLensDistortion.intensity.value / 100f);
            report.AppendLine($"  Lens Distortion: intensity {ppLensDistortion.intensity.value} -> {ppLensDistortion.intensity.value / 100f} (verify sign convention)");
        }

        if (ppProfile.TryGetSettings(out PPv2.DepthOfField ppDepthOfField) && ppDepthOfField.active) {
            var dof = volumeProfile.Add<DepthOfField>(true);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(ppDepthOfField.focusDistance.value);
            dof.aperture.Override(Mathf.Clamp(ppDepthOfField.aperture.value, 1f, 32f));
            report.AppendLine($"  Depth of Field (Bokeh): focusDistance {ppDepthOfField.focusDistance.value}, aperture {ppDepthOfField.aperture.value}");
        }

        if (ppProfile.TryGetSettings(out PPv2.Grain ppGrain) && ppGrain.active) {
            var filmGrain = volumeProfile.Add<FilmGrain>(true);
            filmGrain.intensity.Override(ppGrain.intensity.value);
            report.AppendLine($"  Grain -> Film Grain: intensity {ppGrain.intensity.value}");
        }

        if (ppProfile.TryGetSettings(out PPv2.AutoExposure ppAutoExposure) && ppAutoExposure.active) {
            report.AppendLine("  AutoExposure: NOT converted - no direct URP Volume equivalent. Per URP_MIGRATION_PLAN.md, drop and retune Color Adjustments exposure by eye.");
        }

        AssetDatabase.CreateAsset(volumeProfile, destPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[PPv2ToURPVolumeConverter] Created '{destPath}' from '{sourcePath}':\n{report}");
        EditorGUIUtility.PingObject(volumeProfile);
    }
}
