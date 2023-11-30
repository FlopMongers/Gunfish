using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelManager : PersistentSingleton<LevelManager> {
    public GameEvent OnFinishLoadLevel;
    public GameEvent OnStartPlay;
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

    public override void Initialize() {
        base.Initialize();
        anim = GetComponent<Animator>();
    }

    public void LoadMainMenu() {
        LoadScene("MainMenu", PlayerManager.InputMode.UI);
    }

    public void LoadStats() {
        LoadScene("Stats", PlayerManager.InputMode.EndLevel, MatchManager.Instance.ShowStats);
    }

    public void LoadLevel(string levelName) {
        LoadScene(levelName, PlayerManager.InputMode.Player, FinishLoadLevel);

    }

    void FinishLoadLevel() {
        // set new input mode here?
        OnFinishLoadLevel?.Invoke();
        anim.SetTrigger("countdown");
    }

    // countdown anim invokes this
    public void StartPlay() {
        OnStartPlay?.Invoke();
    }

    void LoadScene(string sceneName, PlayerManager.InputMode inputMode, Action callback = null) {
        nextSceneName = sceneName;
        nextCallback = callback;
        nextInputMode = inputMode;
        // disable controller
        PlayerManager.Instance.SetInputMode(PlayerManager.InputMode.Null);
        anim.SetBool("veil", true);
    }

    // veil anim invokes this
    public void LoadNextScene() {
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

        SkyboxCamera.Instance.RegisterCamera(Camera.main);

        anim.SetBool("veil", false);
    }

    // unveil anim invokes this
    public void InvokeCallback() {
        PlayerManager.Instance.SetInputMode(nextInputMode);
        nextCallback?.Invoke();
    }
}