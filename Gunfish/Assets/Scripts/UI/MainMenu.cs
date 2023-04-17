using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour {
    private UIDocument document;
    private MenuState state;
    private IMenuPage page;

    [SerializeField] private VisualTreeAsset splash;
    [SerializeField] private VisualTreeAsset gameModeSelect;
    [SerializeField] private VisualTreeAsset gunfishSelect;

    private void Start() {
        document = GetComponent<UIDocument>();
        SetState(MenuState.Splash);
    }

    private void Update() {
        if (page != null) {
            page.OnUpdate(document);
        }
    }

    public void OnNavigate(CallbackContext context) {
        // Vertical and horizontal may occur at the same time
        var direction = context.ReadValue<Vector2>();

        // Vertical navigation
        if (direction.y > 0) {
            NavigateUp();
        } else if (direction.y < 0) {
            NavigateDown();
        }

        // Horizontal navigation
        if (direction.x > 0) {
            NavigateRight();
        } else if (direction.x < 0) {
            NavigateLeft();
        }
    }

    private void NavigateUp() {
        print("Up!");
    }

    private void NavigateDown() {
        print("Down!");
    }

    private void NavigateLeft() {
        print("Left");

    }

    private void NavigateRight() {
        print("Right");
    }


    private void SetState(MenuState state) {
        if (this.state == state) {
            return;
        }

        if (page != null) {
            page.OnDisable(document);
        }

        if (state == MenuState.Splash) {
            page = new SplashMenuPage();
        } else if (state == MenuState.GameModeSelect) {
            page = new GameModeSelectMenuPage();
        } else if (state == MenuState.GunfishSelect) {
            page = new FishSelectMenuPage();
        }

        page.OnEnable(document);
    }
}
