using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scene List", menuName = "Scriptable Objects/Scene List")]
public class SceneList : ScriptableObject {

    [Header("Serialized Scenes (ReadOnly)")]
    public List<string> sceneNames;
    public string skyboxSceneName;

#if UNITY_EDITOR
    [Header("Scenes (Editor Only)")]
    public List<UnityEditor.SceneAsset> scenes;
    public UnityEditor.SceneAsset skyboxScene;
    private List<UnityEditor.EditorBuildSettingsScene> buildScenes;
    private void OnValidate() {
        sceneNames = new List<string>();
        buildScenes = new List<UnityEditor.EditorBuildSettingsScene> {
            new UnityEditor.EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new UnityEditor.EditorBuildSettingsScene("Assets/Scenes/Skybox.unity", true),
            new UnityEditor.EditorBuildSettingsScene("Assets/Scenes/Stats.unity", true)
        };
        scenes?.ForEach(scene => {
            var path = UnityEditor.AssetDatabase.GetAssetOrScenePath(scene);
            sceneNames.Add(path);
        });

        if (skyboxScene) {
            var path = UnityEditor.AssetDatabase.GetAssetOrScenePath(skyboxScene);
            skyboxSceneName = path;
        }

        var allLevelLists = Resources.LoadAll<SceneList>("ScriptableObjects/LevelLists").ToList();
        allLevelLists.ForEach(levelList => {
            levelList.sceneNames.ForEach(sceneName => {
                var buildScene = new UnityEditor.EditorBuildSettingsScene(sceneName, true);
                buildScenes.Add(buildScene);
            });
        });
        UnityEditor.EditorBuildSettings.scenes = buildScenes.ToArray();
    }
#endif
}
