using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameModeSelectMenuPage : MenuPage {
    private MenuPageContext menuContext;
    private List<GameMode> gameModes;
    private GameMode displayedGameMode;
    private int displayedGameModeIndex;

    [SerializeField] private TMP_Text gameModeName;
    [SerializeField] private Image gameModeImage;

    public override void OnPageStart(MenuPageContext context) {
        base.OnPageStart(context);
        menuContext = context;

        foreach (var playerInput in PlayerManager.Instance.PlayerInputs) {
            if (!playerInput)
                continue;
            playerInput.currentActionMap.FindAction("Navigate").performed += OnNavigate;
            playerInput.currentActionMap.FindAction("Submit").performed += OnSubmit;
            playerInput.currentActionMap.FindAction("Cancel").performed += OnCancel;
        }

        displayedGameModeIndex = 0;
        gameModes = GameManager.Instance.GameModeList.gameModes;
        if (gameModes.Count > 0) {
            DisplayGameMode(gameModes[displayedGameModeIndex]);
        }
    }

    public override void OnPageStop(MenuPageContext context) {
        foreach (var playerInput in PlayerManager.Instance.PlayerInputs) {
            if (!playerInput)
                continue;
            playerInput.currentActionMap.FindAction("Navigate").performed -= OnNavigate;
            playerInput.currentActionMap.FindAction("Submit").performed -= OnSubmit;
        }
        base.OnPageStop(context);
    }

    private void OnNavigate(InputAction.CallbackContext context) {
        var direction = context.ReadValue<Vector2>();
        // Joystick movement should only be registered if it's a full flick
        if (direction.magnitude < 0.9f) {
            return;
        }

        // Horizontal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            if (direction.x > 0) {
                IncrementGameMode();
            }
            else {
                DecrementGameMode();
            }
        }
    }

    private void OnCancel(InputAction.CallbackContext context) {
        menuContext.menu.SetState(MenuState.Splash, MenuDirection.Right);
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        GameManager.Instance.SetSelectedGameMode(displayedGameMode.gameModeType);
        menuContext.menu.SetState(MenuState.FishSelect);
    }

    private void IncrementGameMode() {
        // Increments before modulus
        displayedGameModeIndex = (++displayedGameModeIndex) % gameModes.Count;
        DisplayGameMode(gameModes[displayedGameModeIndex]);
    }

    private void DecrementGameMode() {
        // Decrements before comparison
        if (--displayedGameModeIndex < 0) {
            displayedGameModeIndex += gameModes.Count;
        }
        DisplayGameMode(gameModes[displayedGameModeIndex]);
    }

    private void DisplayGameMode(GameMode gameMode) {
        displayedGameMode = gameMode;
        gameModeImage.sprite = gameMode.image;
        gameModeName.text = gameMode.name;
    }
}
