using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Scene List", menuName = "Scriptable Objects/Scene List")]
public class SceneList : ScriptableObject {

    [Header("Serialized Scenes (ReadOnly)")]
    public List<string> sceneNames;

#if UNITY_EDITOR
    [Header("Scenes (Editor Only)")]
    public List<UnityEditor.SceneAsset> scenes;

    private void OnValidate() {
        sceneNames = new List<string>();
        scenes?.ForEach(scene => {
            sceneNames.Add(scene.name);
        });
    }
#endif
}
