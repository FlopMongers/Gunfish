using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SplashMenuPage : IMenuPage {
    private MenuPageContext menuContext;
    private bool isLoadingNextMenu;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;
        isLoadingNextMenu = false;
        ArduinoManager.Instance.playAttractors = true;

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
        if (isLoadingNextMenu == false)
        {
            isLoadingNextMenu = true;
            FX_Spawner.Instance.SpawnFX(FXType.TitleScreenStartFX, Camera.main.transform.position, Quaternion.identity);
            Fade();
            DOTween.Sequence().AppendInterval(1).AppendCallback(LoadNextMenu);
        }
    }

    private void LoadNextMenu() {
        GameManager.Instance.SetSelectedGameMode(GameManager.Instance.defaultGameMode);
        menuContext.menu.SetState(MenuState.GunfishSelect);
    }

    private void Unfade() {
        menuContext.document.rootVisualElement.Q("MenuContainer").RemoveFromClassList("faded");
    }

    private void Fade() {
        menuContext.document.rootVisualElement.Q("MenuContainer").AddToClassList("faded");
    }
}