using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public struct MenuPageContext {
    public MainMenu menu;
    public UIDocument document;
    public InputActionMap actionMap;
}

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour {

    private MenuPageContext context;

    private MenuState state;
    private IMenuPage page;

    [SerializeField] private VisualTreeAsset splash;
    [SerializeField] private VisualTreeAsset gameModeSelect;
    [SerializeField] private VisualTreeAsset gunfishSelect;

    private void Start() {
        InitMenuPageContext();
        PlayerManager.instance.SetInputMode(PlayerManager.InputMode.UI);
        SetState(MenuState.Splash);
    }

    private void InitMenuPageContext() {
        context = new MenuPageContext();

        context.menu = this;
        context.document = GetComponent<UIDocument>();
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

        if (page != null)
        {
            page.OnDisable(context);
        }

        if (state == MenuState.Splash) {
            context.document.visualTreeAsset = splash;
            page = new SplashMenuPage();

        } else if (state == MenuState.GameModeSelect) {
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
}
