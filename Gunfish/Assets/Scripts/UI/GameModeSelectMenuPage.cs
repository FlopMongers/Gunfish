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
        context.actionMap.FindAction("Navigate").performed += OnNavigate;

        gameModeImage = menuContext.document.rootVisualElement.Q<VisualElement>("gamemode-image");
        gameModeName = menuContext.document.rootVisualElement.Q<Label>("gamemode-name");
        backButton = menuContext.document.rootVisualElement.Q<Button>("back-button");
        nextButton = menuContext.document.rootVisualElement.Q<Button>("next-button");
        
        displayedGameModeIndex = 0;
        gameModes = GameManager.instance.GameMode_List;
        if (gameModes.Count > 0) {
            DisplayGameMode(gameModes[displayedGameModeIndex]);
        }
    }

    public void OnDisable(MenuPageContext context) {
        context.actionMap.FindAction("Navigate").performed -= OnNavigate;
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

    private void IncrementGameMode() {
        displayedGameModeIndex = (++displayedGameModeIndex) % gameModes.Count;
        DisplayGameMode(gameModes[displayedGameModeIndex]);
    }

    private void DecrementGameMode() {
        if (--displayedGameModeIndex < 0) {
            displayedGameModeIndex += gameModes.Count;
        }
        DisplayGameMode(gameModes[displayedGameModeIndex]);
    }

    private void DisplayGameMode(GameMode gameMode) {
        gameModeImage.style.backgroundImage = gameMode.image;
        gameModeName.text = gameMode.name;
    }
}
