using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelManager: PersistentSingleton<LevelManager> {
    public GameEvent FinishLoadLevel_Event;
    public GameEvent StartPlay_Event;

    public void LoadMainMenu() {
        LoadScene("MainMenu");
    }

    public void LoadStats() {
        LoadScene("Stats");
    }

    public void LoadLevel(string levelName) {
        StartCoroutine(CoLoadLevel(levelName));
    }

    IEnumerator CoLoadLevel(string levelName)
    {
        var op = LoadScene(levelName);
        while (op.isDone == false)
        {
            yield return null;
        }
        FinishLoadLevel_Event?.Invoke();
        // TODO: Marquee goes here
        StartPlay_Event?.Invoke();
    }

    private AsyncOperation LoadScene(string sceneName) {
        return SceneManager.LoadSceneAsync(sceneName);
    }
}
