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
        LoadScene(levelName);
           

        FinishLoadLevel_Event?.Invoke();
        // TODO: Marquee goes here
        StartPlay_Event?.Invoke();
    }

    private void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}
