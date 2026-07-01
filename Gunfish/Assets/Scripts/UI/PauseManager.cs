using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PauseManager : Singleton<PauseManager> {
    public AudioMixer audioMixer;
    public bool paused = false;
    Animator anim;

    int pausePriority;

    public void Start() {
        GetComponent<Canvas>().enabled = true;
        anim = GetComponent<Animator>();
    }

    public void Update() {
        // Migrated from the legacy Input Manager "Pause" (escape/p) and "Quit" (q) buttons.
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        if (GameModeManager.Instance?.matchManagerInstance != null
            && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            Pause();
        if (paused == true && keyboard.qKey.wasPressedThisFrame)
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
        Cursor.visible = paused;
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