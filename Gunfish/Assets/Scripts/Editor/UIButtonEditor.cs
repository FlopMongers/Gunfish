using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIButton))]
[CanEditMultipleObjects]
public class UIButtonEditor : ButtonEditor {
    private SerializedProperty label;

    protected override void OnEnable() {
        base.OnEnable();
        label = serializedObject.FindProperty("label");
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(label);
        serializedObject.ApplyModifiedProperties();
    }
}
