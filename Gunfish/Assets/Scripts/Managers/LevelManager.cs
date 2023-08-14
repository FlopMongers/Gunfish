using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class LevelManager: PersistentSingleton<LevelManager> {
    public GameEvent FinishLoadLevel_Event;
    public GameEvent StartPlay_Event;

    public void LoadMainMenu() {
        LoadScene("MainMenu");
    }

    public void LoadStats() {
        StartCoroutine(CoLoadLevel("Stats", MatchManager.instance.ShowStats));
    }

    public void LoadLevel(string levelName) {
        StartCoroutine(CoLoadLevel(levelName, OnFinishLoadLevel));
    }

    IEnumerator CoLoadLevel(string levelName, Action callback = null)
    {
        var op = LoadScene(levelName);
        while (op.isDone == false)
        {
            yield return null;
        }
        if (callback != null)
        {
            callback();
        }
    }

    void OnFinishLoadLevel()
    {
        FinishLoadLevel_Event?.Invoke();
        // TODO: Marquee goes here
        StartPlay_Event?.Invoke();
    }

    private AsyncOperation LoadScene(string sceneName) {
        return SceneManager.LoadSceneAsync(sceneName);
    }
}
