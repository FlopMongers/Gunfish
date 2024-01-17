using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public struct MenuPageContext {
    public MainMenu menu;
    public UIDocument document;
    public InputActionMap actionMap;
}

[RequireComponent(typeof(UIDocument))]
public class MainMenu : Singleton<MainMenu> {

    private MenuPageContext context;

    private MenuState state;
    private IMenuPage page;

    [SerializeField] private AudioClip uiSound;

    [SerializeField] private VisualTreeAsset splash;
    [SerializeField] private VisualTreeAsset gameModeSelect;
    [SerializeField] private VisualTreeAsset gunfishSelect;

    public override void Initialize() {
        base.Initialize();
        InitializeMenu();
    }

    public void InitializeMenu() {
        context = new MenuPageContext();

        context.menu = this;
        context.document = GetComponent<UIDocument>();
        SetState(MenuState.Splash);
    }

    private void Update() {
        if (page != null) {
            page.OnUpdate(context);
        }
    }

    public void SetState(MenuState state) {
        if (this.state == state) {
            return;
        }

        if (page != null) {
            page.OnDisable(context);
        }

        if (state == MenuState.Splash) {
            context.document.visualTreeAsset = splash;
            page = new SplashMenuPage();

        }
        else if (state == MenuState.GameModeSelect) {
            context.document.visualTreeAsset = gameModeSelect;
            page = new GameModeSelectMenuPage();
        }
        else if (state == MenuState.GunfishSelect) {
            context.document.visualTreeAsset = gunfishSelect;
            page = new FishSelectMenuPage();
        }

        page.OnEnable(context);
        this.state = state;
    }

    public void PlayBloop() {
        GetComponent<AudioSource>()?.PlayOneShot(uiSound);
    }
}