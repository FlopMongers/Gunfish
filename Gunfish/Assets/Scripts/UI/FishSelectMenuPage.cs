using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class FishSelectMenuPage : IMenuPage {
    private MenuPageContext menuContext;
    public List<GunfishData> fish;
    private GunfishData displayedFish;
    private int displayedFishIndex;

    private VisualElement gameModeImage;
    private Label gameModeName;
    private Button backButton;
    private Button nextButton;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;
        //context.actionMaps.FindAction("Navigate").performed += OnNavigate;
        //context.actionMaps.FindAction("Submit").performed += OnSubmit;

        gameModeImage = menuContext.document.rootVisualElement.Q<VisualElement>("gamemode-image");
        gameModeName = menuContext.document.rootVisualElement.Q<Label>("gamemode-name");
        backButton = menuContext.document.rootVisualElement.Q<Button>("back-button");
        nextButton = menuContext.document.rootVisualElement.Q<Button>("next-button");

        displayedFishIndex = 0;
        fish = GameManager.instance.Gunfish_List;
        if (fish.Count > 0) {
            DisplayFish(fish[displayedFishIndex]);
        }
    }

    public void OnDisable(MenuPageContext context) {
        //context.actionMaps.FindAction("Navigate").performed -= OnNavigate;
        //context.actionMaps.FindAction("Submit").performed -= OnSubmit;
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
                IncrementFish();
            } else {
                DecrementFish();
            }
        }
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        //GameManager.instance.selectedGameMode = displayedFish;
        menuContext.menu.SetState(MenuState.GunfishSelect);
    }

    private void IncrementFish() {
        // Increments before modulus
        displayedFishIndex = (++displayedFishIndex) % fish.Count;
        DisplayFish(fish[displayedFishIndex]);
    }

    private void DecrementFish() {
        // Decrements before comparison
        if (--displayedFishIndex < 0) {
            displayedFishIndex += fish.Count;
        }
        DisplayFish(fish[displayedFishIndex]);
    }

    private void DisplayFish(GunfishData fish) {
        displayedFish = fish;
        gameModeImage.style.backgroundImage = fish.spriteMat.mainTexture as Texture2D;
        gameModeName.text = fish.name;
    }
}
