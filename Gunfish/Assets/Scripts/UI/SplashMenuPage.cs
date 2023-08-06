using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine;

public class SplashMenuPage : IMenuPage {
    private MenuPageContext menuContext;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;

        foreach (var playerInput in PlayerManager.instance.PlayerInputs) {
            if (playerInput) {
                playerInput.currentActionMap.FindAction("Any").performed += OnAnyKey;
            }
        }
    }

    public void OnDisable(MenuPageContext context) {
        foreach (var playerInput in PlayerManager.instance.PlayerInputs) {
            if (playerInput) {
                playerInput.currentActionMap.FindAction("Any").performed -= OnAnyKey;
            }
        }
    }

    public void OnUpdate(MenuPageContext context) {
        
    }

    private void OnAnyKey(InputAction.CallbackContext context) {
        menuContext.menu.SetState(MenuState.GameModeSelect);
    }
}
