using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor {
    float previewT = 0f;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MovingPlatform platform = (MovingPlatform)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Path Preview", EditorStyles.boldLabel);

        if (Application.isPlaying) {
            EditorGUILayout.HelpBox("Path preview is only available outside Play mode.", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox("Scrubbing moves this object's actual transform. Reset to Start (or set the slider back to 0) before saving the scene or entering Play mode.", MessageType.Warning);

        EditorGUI.BeginChangeCheck();
        previewT = EditorGUILayout.Slider("Preview Position", previewT, 0f, 1f);
        if (EditorGUI.EndChangeCheck()) {
            ApplyPreview(platform, previewT);
        }

        if (GUILayout.Button("Reset To Start")) {
            previewT = 0f;
            ApplyPreview(platform, previewT);
        }
    }

    void ApplyPreview(MovingPlatform platform, float t) {
        Vector3? pos = platform.EvaluatePathPosition(t);
        if (!pos.HasValue) return;
        Undo.RecordObject(platform.transform, "Preview Moving Platform Path");
        platform.transform.position = pos.Value;
    }
}
