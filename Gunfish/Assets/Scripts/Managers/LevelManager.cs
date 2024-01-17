using System;
using System.Collections;
using System.Linq;
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
    string nextScenePath;
    string nextSkyboxScenePath;
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

    public void LoadLevel(string levelName, string skyboxScenePath) {
        LoadScene(levelName, PlayerManager.InputMode.Player, skyboxScenePath, FinishLoadLevel);
    }

    void FinishLoadLevel() {
        OnFinishLoadLevel?.Invoke();
        anim.SetTrigger("countdown");
        var sceneName = ScenePathToName(nextScenePath);
        MarqueeManager.Instance.PlayTitle(sceneName);
    }

    // countdown anim invokes this
    public void StartPlay() {
        OnStartPlay?.Invoke();
    }

    private void LoadScene(string scenePath, PlayerManager.InputMode inputMode, string skyboxScenePath = null, Action callback = null) {
        nextScenePath = scenePath;
        nextSkyboxScenePath = skyboxScenePath;
        nextCallback = callback;
        nextInputMode = inputMode;
        // disable controller
        PlayerManager.Instance.SetInputMode(PlayerManager.InputMode.Null);
        anim.SetBool("veil", true);
    }

    // veil anim invokes this
    public void LoadNextScene() {
        StartCoroutine(CoLoadScene(nextScenePath));
    }

    IEnumerator CoLoadScene(string sceneName) {
        var sceneLoadMode = LoadSceneMode.Single;

        if (nextSkyboxScenePath != null) {
            sceneLoadMode = LoadSceneMode.Additive;
            var skyboxOp = SceneManager.LoadSceneAsync(nextSkyboxScenePath);
            while (skyboxOp.isDone == false) {
                yield return new WaitForEndOfFrame();
            }
        }
        
        var sceneOp = SceneManager.LoadSceneAsync(sceneName, sceneLoadMode);
        while (sceneOp.isDone == false) {
            yield return new WaitForEndOfFrame();
        }

        if (nextSkyboxScenePath != null) {
            SkyboxCamera.Instance.RegisterCamera(Camera.main);
        }

        anim.SetBool("veil", false);
    }

    // unveil anim invokes this
    public void InvokeCallback() {
        PlayerManager.Instance.SetInputMode(nextInputMode);
        nextCallback?.Invoke();
    }

    // Assumes camel case naming convention for level names
    private string ScenePathToName(string scenePath) {
        return scenePath.Split("/").Last().Replace(".unity", "");
    }
}