using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public struct MenuPageContext {
    public MainMenu menu;
    public InputActionMap actionMap;
    public GameObject pageObject;
}

public class MainMenu : Singleton<MainMenu> {

    private MenuPageContext context;

    private MenuState state;
    private MenuPage currentPage;

    [SerializeField] private AudioClip uiSound;

    [SerializeField] private SplashMenuPage splashMenuPage;
    [SerializeField] private GameModeSelectMenuPage gameModeSelectMenuPage;
    [SerializeField] private FishSelectMenuPage fishSelectMenuPage;

    public override void Initialize() {
        base.Initialize();
        InitializeMenu();
    }

    public void InitializeMenu() {
        context = new MenuPageContext();

        context.menu = this;
        SetState(MenuState.Splash);
    }

    public void SetState(MenuState state) {
        if (this.state == state) {
            return;
        }

        currentPage?.OnPageStop(context);

        if (state == MenuState.Splash) {
            currentPage = splashMenuPage;
        } else if (state == MenuState.GameModeSelect) {
            currentPage = gameModeSelectMenuPage;
        } else if (state == MenuState.FishSelect) {
            currentPage = fishSelectMenuPage;
        }

        currentPage?.OnPageStart(context);
        this.state = state;
    }

    public void PlayBloop() {
        GetComponent<AudioSource>()?.PlayOneShot(uiSound);
    }
}