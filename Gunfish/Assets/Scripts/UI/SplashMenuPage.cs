using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SplashMenuPage : IMenuPage {
    private MenuPageContext menuContext;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;

        foreach (var playerInput in PlayerManager.Instance.PlayerInputs) {
            if (playerInput) {
                playerInput.currentActionMap.FindAction("Any").performed += OnAnyKey;
            }
        }
    }

    public void OnDisable(MenuPageContext context) {
        foreach (var playerInput in PlayerManager.Instance.PlayerInputs) {
            if (playerInput) {
                playerInput.currentActionMap.FindAction("Any").performed -= OnAnyKey;
            }
        }
    }

    public void OnUpdate(MenuPageContext context) {

    }

    private void OnAnyKey(InputAction.CallbackContext context) {
        GameManager.Instance.SetSelectedGameMode(GameModeType.DeathMatch);
        menuContext.menu.SetState(MenuState.GunfishSelect);
    }
}