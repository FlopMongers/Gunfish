using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour {
    private UIDocument document;
    private MenuState state;

    [SerializeField] private VisualTreeAsset splash;
    [SerializeField] private VisualTreeAsset gameModeSelect;
    [SerializeField] private VisualTreeAsset gunfishSelect;

    private void Start() {
        document = GetComponent<UIDocument>();
        SetState(MenuState.Splash);
    }

    private void Update() {
        if (this.state == MenuState.Splash) {
            SplashBehavior();
        } else if (this.state == MenuState.GameModeSelect) {
            GameModeSelectBehavior();
        } else if (this.state == MenuState.GunfishSelect) {
            GunfishSelectBehavior();
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
        this.state = state;
        print("State set to " + state.ToString());
        if (this.state == MenuState.Splash) {
            OnSplashEnter();
            document.visualTreeAsset = splash;

        } else if (this.state == MenuState.GameModeSelect) {
            OnGameModeSelectEnter();
            document.visualTreeAsset = gameModeSelect;
        } else if (this.state == MenuState.GunfishSelect) {
            OnGunfishSelectEnter();
            document.visualTreeAsset = gunfishSelect;
        }
    }

    private void OnSplashEnter() {
        InputSystem.onAnyButtonPress.CallOnce(control => {
            SetState(MenuState.GameModeSelect);
        });
    }

    private void OnGameModeSelectEnter() {

    }

    private void OnGunfishSelectEnter() {

    }


    private void SplashBehavior() {

    }

    private void GameModeSelectBehavior() {

    }

    private void GunfishSelectBehavior() {

    }
}

public enum MenuState {
    Splash,
    GameModeSelect,
    GunfishSelect,
}
