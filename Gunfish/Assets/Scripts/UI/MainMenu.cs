using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public struct MenuPageContext {
    public MainMenu menu;
    public UIDocument document;
    public List<InputDevice> devices;
    public List<InputActionMap> actionMaps;
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
        SetState(MenuState.Splash);
    }

    private void InitMenuPageContext() {
        context = new MenuPageContext();

        context.menu = this;
        context.document = GetComponent<UIDocument>();
        context.devices = PlayerManager.instance.PlayerDevices;
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
