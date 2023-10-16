using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseManager : Singleton<PauseManager> {
    public AudioMixer audioMixer;
    bool paused = false;
    Animator anim;

    int pausePriority;

    public void Start() {
        anim = GetComponent<Animator>();
    }

    public void Update() {
        if (Input.GetButtonDown("Pause"))
            Pause();
    }

    public void MainMenu() {
        PauseTime(1, 1);
        LevelManager.instance?.LoadMainMenu();
    }


    public void Pause() {
        paused = !paused;
        PauseTime((paused) ? 1 : 0, 1);
        anim.SetBool("Pause", paused);
        audioMixer.SetFloat("MasterLowpass", (paused) ? 500f : 22000f);
    }

    public void PauseTime(int pause, int priority = 0) {
        if (priority < pausePriority)
            return;
        pausePriority = (pause == 0) ? 0 : priority;
        Time.timeScale = pause;
    }
}