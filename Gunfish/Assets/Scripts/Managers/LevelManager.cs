using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using static UnityEngine.Application;

[System.Serializable]
public class LevelManager: PersistentSingleton<LevelManager> {
    public GameEvent FinishLoadLevel_Event;
    public GameEvent StartPlay_Event;
    public string skyboxScene;


    // load level, set up callback
    // play veil anim
    // animation invokes load scene, which is async
    // async method call then tells anim to unveil anim
    // unveil invokes callback, if any

    // if level, callback invokes FinishLoadLevel_Event and starts countdown anim, which invokes StartPlay_Event

    Action nextCallback;
    string nextSceneName;
    PlayerManager.InputMode nextInputMode;
    Animator anim;

    private void Start() 
    {
        anim = GetComponent<Animator>();
    }

    public void LoadMainMenu() 
    {
        LoadScene("MainMenu", PlayerManager.InputMode.UI);
    }

    public void LoadStats() 
    {
        LoadScene("Stats", PlayerManager.InputMode.EndLevel, MatchManager.instance.ShowStats);
    }

    public void LoadLevel(string levelName) 
    {
        LoadScene(levelName, PlayerManager.InputMode.Player, FinishLoadLevel);

    }

    void FinishLoadLevel() 
    {
        // set new input mode here?
        FinishLoadLevel_Event?.Invoke();
        anim.SetTrigger("countdown");
    }

    // countdown anim invokes this
    public void StartPlay() 
    {
        StartPlay_Event?.Invoke();
    }

    void LoadScene(string sceneName, PlayerManager.InputMode inputMode, Action callback = null) 
    {
        nextSceneName = sceneName;
        nextCallback = callback;
        nextInputMode = inputMode;
        // disable controller
        PlayerManager.instance.SetInputMode(PlayerManager.InputMode.Null);
        anim.SetBool("veil", true);
    }

    // veil anim invokes this
    public void LoadNextScene() 
    {
        StartCoroutine(CoLoadScene(nextSceneName));
    }

    IEnumerator CoLoadScene(string sceneName) {
        var op2 = SceneManager.LoadSceneAsync(skyboxScene);
        while (op2.isDone == false) {
            yield return null;
        }
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (op.isDone == false) {
            yield return null;
        }
        anim.SetBool("veil", false);
    }

    // unveil anim invokes this
    public void InvokeCallback() 
    {
        PlayerManager.instance.SetInputMode(nextInputMode);
        nextCallback?.Invoke();
    }
}
