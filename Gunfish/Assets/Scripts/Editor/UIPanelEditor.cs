using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIPanel))]
[CanEditMultipleObjects]
public class UIPanelEditor : ImageEditor {
    private SerializedProperty borderColor;
    private SerializedProperty cornerRadius;
    private SerializedProperty borderWidth;
    private SerializedProperty edgeSoftness;
    private SerializedProperty outlineOnly;

    protected override void OnEnable() {
        base.OnEnable();
        borderColor = serializedObject.FindProperty("borderColor");
        cornerRadius = serializedObject.FindProperty("cornerRadius");
        borderWidth = serializedObject.FindProperty("borderWidth");
        edgeSoftness = serializedObject.FindProperty("edgeSoftness");
        outlineOnly = serializedObject.FindProperty("outlineOnly");
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("UI Panel (Gunfish)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(borderColor);
        EditorGUILayout.PropertyField(cornerRadius);
        EditorGUILayout.PropertyField(borderWidth);
        EditorGUILayout.PropertyField(edgeSoftness);
        EditorGUILayout.PropertyField(outlineOnly);
        serializedObject.ApplyModifiedProperties();
    }
}
