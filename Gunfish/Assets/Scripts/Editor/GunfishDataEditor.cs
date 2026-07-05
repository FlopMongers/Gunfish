using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GunfishData))]
public class GunfishDataEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GunfishData data = (GunfishData)target;

        if (data.fishPrefab == null) {
            EditorGUILayout.HelpBox("This fish has never been baked. Click Garbulate to bake it into a prefab.", MessageType.Warning);
        } else if (GunfishPrefabBaker.ComputeBakeSnapshot(data) != data.bakedSnapshotJson) {
            EditorGUILayout.HelpBox("This fish's data has changed since it was last baked. Click Garbulate to re-bake before testing.", MessageType.Warning);
        }

        if (GUILayout.Button("Garbulate")) {
            GunfishPrefabBaker.BakeFish(data);
        }
    }
}
