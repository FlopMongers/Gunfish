using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameModeSelectMenuPage : IMenuPage {
    private MenuPageContext menuContext;
    public List<GameMode> gameModes;
    private GameMode displayedGameMode;
    private int displayedGameModeIndex;

    private VisualElement gameModeImage;
    private Label gameModeName;
    private Button backButton;
    private Button nextButton;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;
        
        foreach (var playerInput in PlayerManager.instance.PlayerInputs) {
            if (!playerInput) continue;
            playerInput.currentActionMap.FindAction("Navigate").performed += OnNavigate;
            playerInput.currentActionMap.FindAction("Submit").performed += OnSubmit;
        }

        gameModeImage = menuContext.document.rootVisualElement.Q<VisualElement>("gamemode-image");
        gameModeName = menuContext.document.rootVisualElement.Q<Label>("gamemode-name");
        backButton = menuContext.document.rootVisualElement.Q<Button>("back-button");
        nextButton = menuContext.document.rootVisualElement.Q<Button>("next-button");
        
        displayedGameModeIndex = 0;
        gameModes = GameManager.instance.GameModeList;
        if (gameModes.Count > 0) {
            DisplayGameMode(gameModes[displayedGameModeIndex]);
        }
    }

    public void OnDisable(MenuPageContext context) {
        foreach (var playerInput in PlayerManager.instance.PlayerInputs) {
            if (!playerInput) continue;
            playerInput.currentActionMap.FindAction("Navigate").performed -= OnNavigate;
            playerInput.currentActionMap.FindAction("Submit").performed -= OnSubmit;
        }
    }

    public void OnUpdate(MenuPageContext context) {

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
            } else {
                DecrementGameMode();
            }
        } 
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        GameManager.instance.SetSelectedGameMode(displayedGameMode.gameModeType);
        menuContext.menu.SetState(MenuState.GunfishSelect);
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
        gameModeImage.style.backgroundImage = gameMode.image;
        gameModeName.text = gameMode.name;
    }
}
