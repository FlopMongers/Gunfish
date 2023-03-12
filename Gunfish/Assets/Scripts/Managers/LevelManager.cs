using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelManager: Singleton<LevelManager> {
    private List<Scene> levels;
    private int nextLevelIndex;

    public GameEvent FinishLoadLevel_Event;

    public void SetLevelList(List<Scene> levels, bool randomize) {
        this.levels = levels;
        nextLevelIndex = 0;

        if (randomize) {
            this.levels = this.levels.OrderBy(level => Random.value).ToList();
        }
    }

    public void LoadMainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadNextLevel() {
        if (nextLevelIndex < levels.Count) {
            // TODO, add actual async loading with UI and stuff
            SceneManager.LoadScene(levels[nextLevelIndex].name);
            FinishLoadLevel_Event?.Invoke();
        } else {
            LoadStats();
        }
    }

    public void LoadStats() {
        SceneManager.LoadScene("Stats");
    }
}
