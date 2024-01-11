using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelManager : PersistentSingleton<LevelManager> {
    public GameEvent OnFinishLoadLevel;
    public GameEvent OnStartPlay;

    // load level, set up callback
    // play veil anim
    // animation invokes load scene, which is async
    // async method call then tells anim to unveil anim
    // unveil invokes callback, if any

    // if level, callback invokes FinishLoadLevel_Event and starts countdown anim, which invokes StartPlay_Event

    Action nextCallback;
    string nextSceneName;
    string nextSkyboxSceneName;
    PlayerManager.InputMode nextInputMode;
    Animator anim;

    public override void Initialize() {
        base.Initialize();
        anim = GetComponent<Animator>();
    }

    public void LoadMainMenu(Action callback = null) {
        LoadScene("MainMenu", PlayerManager.InputMode.UI, null, callback);
    }

    public void LoadStats(Action callback = null) {
        LoadScene("Stats", PlayerManager.InputMode.EndLevel, null, callback);
    }

    public void LoadLevel(string levelName, string skyboxSceneName) {
        LoadScene(levelName, PlayerManager.InputMode.Player, skyboxSceneName, FinishLoadLevel);
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

    void LoadScene(string sceneName, PlayerManager.InputMode inputMode, string skyboxSceneName = null, Action callback = null) {
        nextSceneName = sceneName;
        nextSkyboxSceneName = skyboxSceneName;
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
        var sceneLoadMode = LoadSceneMode.Single;

        if (nextSkyboxSceneName != null) {
            sceneLoadMode = LoadSceneMode.Additive;
            var skyboxOp = SceneManager.LoadSceneAsync(nextSkyboxSceneName);
            while (skyboxOp.isDone == false) {
                yield return new WaitForEndOfFrame();
            }
        }
        
        var sceneOp = SceneManager.LoadSceneAsync(sceneName, sceneLoadMode);
        while (sceneOp.isDone == false) {
            yield return new WaitForEndOfFrame();
        }

        if (nextSkyboxSceneName != null) {
            SkyboxCamera.Instance.RegisterCamera(Camera.main);
        }

        anim.SetBool("veil", false);
    }

    // unveil anim invokes this
    public void InvokeCallback() {
        PlayerManager.Instance.SetInputMode(nextInputMode);
        nextCallback?.Invoke();
    }
}