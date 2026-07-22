using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PauseManager : Singleton<PauseManager> {
    public enum Page {
        MainPauseMenu,
        Settings,
    }

    [SerializeField] private GameObject MainPauseMenuPage;
    [SerializeField] private GameObject SettingsPage;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private bool paused = false;
    

    
    private Animator anim;
    private CanvasGroup canvasGroup;

    

    int pausePriority;

    public void Start() {
        MainPauseMenuPage.SetActive(true);
        SettingsPage.SetActive(false);

        GetComponent<Canvas>().enabled = true;
        
        
        anim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();


        SetInteractable(paused);
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
        SetInteractable(paused);
    }

    public void SetPageToMainPauseMenu() {
        SetActivePage(Page.MainPauseMenu);
    }
    
    public void SetPageToSettings() {
        SetActivePage(Page.Settings);
    }

    private void SetActivePage(Page page) {
        switch (page) {
            case Page.MainPauseMenu:
                MainPauseMenuPage.SetActive(true);
                SettingsPage.SetActive(false);
                break;
            case Page.Settings:
                MainPauseMenuPage.SetActive(false);
                SettingsPage.SetActive(true);
                break;
        }
    }

    void SetInteractable(bool interactable) {
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    public void PauseTime(int pause, int priority = 0) {
        if (priority < pausePriority)
            return;
        pausePriority = (pause == 0) ? 0 : priority;
        Time.timeScale = pause;
    }
}