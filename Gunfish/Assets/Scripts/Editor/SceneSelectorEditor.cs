using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneSelectorEditor : Editor {
    [MenuItem("Tools/Scenes/Load Next Build Scene %T")]
    public static void LoadNextBuildScene() {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        var index = EditorSceneManager.GetActiveScene().buildIndex;
        var size = scenes.Length;
        var nextIndex = (index + 1) % size;
        var nextScene = scenes[nextIndex];
        EditorSceneManager.OpenScene(nextScene.path);
    }

    [MenuItem("Tools/Scenes/Load Previous Build Scene %#T")]
    public static void LoadPreviousBuildScene() {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }
        var scenes = EditorBuildSettings.scenes;
        var index = EditorSceneManager.GetActiveScene().buildIndex;
        var size = scenes.Length;
        var nextIndex = index == 0 ? size - 1 : index - 1;
        var nextScene = scenes[nextIndex];
        EditorSceneManager.OpenScene(nextScene.path);
    }

    [MenuItem("Tools/Scenes/Load Main Menu Scene %&T")]
    public static void LoadMainMenuBuildScene() {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            return;
        }
        var scenes = EditorBuildSettings.scenes;
        var nextScene = scenes[0];
        EditorSceneManager.OpenScene(nextScene.path);
    }
}
