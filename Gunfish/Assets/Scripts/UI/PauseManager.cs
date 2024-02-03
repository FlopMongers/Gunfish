using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseManager : Singleton<PauseManager> {
    public AudioMixer audioMixer;
    public bool paused = false;
    Animator anim;

    int pausePriority;

    public void Start() {
        anim = GetComponent<Animator>();
    }

    public void Update() {
        if (GameModeManager.Instance?.matchManagerInstance != null && Input.GetButtonDown("Pause"))
            Pause();
        if (paused == true && Input.GetButtonDown("Quit"))
            MainMenu();
    }

    public void MainMenu() {
        if (paused == true) {
            Pause();
        }
        else {
            PauseTime(1, 1);
        }
        GameModeManager.Instance.matchManagerInstance.ENDITALL();
    }


    public void Pause() {
        paused = !paused;
        PauseTime((paused) ? 0 : 1, 1);
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