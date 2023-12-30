using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Gunfish))]
public class GunfishEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Gunfish script = (Gunfish)target;
        if (GUILayout.Button("Garbulate")) {
            script.Garbulate();
        }
    }
}