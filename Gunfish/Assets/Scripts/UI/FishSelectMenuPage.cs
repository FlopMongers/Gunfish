using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class FishSelectMenuPage : IMenuPage {
    private MenuPageContext menuContext;
    public List<GunfishData> fishes;
    
    private List<GunfishData> displayedFishes;
    private List<int> displayedFishIndices;

    private List<VisualElement> fishImages;
    private List<Label> fishLabels;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;

        fishImages = new List<VisualElement>();

        for (int i = 0; i < PlayerManager.instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.instance.PlayerInputs[i];
            int playerIndex = i;
            playerInput.currentActionMap.FindAction("Navigate").performed += (InputAction.CallbackContext context) => OnNavigate(context, playerIndex);
            playerInput.currentActionMap.FindAction("Submit").performed += (InputAction.CallbackContext context) => OnSubmit(context, playerIndex);
        
            var image = menuContext.document.rootVisualElement
                .Q<VisualElement>($"FishSelector{playerIndex + 1}")
                .Q<VisualElement>("fish-image");
            
            fishImages.Add(image);
        }
        
        displayedFishes = new List<GunfishData>();
        displayedFishIndices = new List<int>();

        fishes = GameManager.instance.GunfishList;

        if (fishes.Count < 1) {
            throw new UnityException("GameManager.Gunfish_List must contain at least one fish for the game to function.");
        }

        for (int i = 0; i < PlayerManager.instance.PlayerInputs.Count; i++) {
            displayedFishes.Add(fishes[0]);
            displayedFishIndices.Add(0);
        }

        displayedFishIndices.ForEach(displayedFishIndex => {
            DisplayFish(displayedFishIndex, fishes[displayedFishIndex]);
        });
    }

    public void OnDisable(MenuPageContext context) {
        for (int i = 0; i < PlayerManager.instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.instance.PlayerInputs[i];
            int playerIndex = i;
            playerInput.currentActionMap.FindAction("Navigate").performed -= (InputAction.CallbackContext context) => OnNavigate(context, playerIndex);
            playerInput.currentActionMap.FindAction("Submit").performed -= (InputAction.CallbackContext context) => OnSubmit(context, playerIndex);
        }
    }

    public void OnUpdate(MenuPageContext context) {

    }

    private void OnNavigate(InputAction.CallbackContext context, int deviceIndex) {
        var direction = context.ReadValue<Vector2>();
        // Joystick movement should only be registered if it's a full flick
        if (direction.magnitude < 0.9f || context.canceled) {
            return;
        }

        // Horizontal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            if (direction.x > 0) {
                IncrementFish(deviceIndex);
            } else {
                DecrementFish(deviceIndex);
            }
        }
    }

    private void OnSubmit(InputAction.CallbackContext context, int deviceIndex) {
        LevelManager.instance.LoadLevel("Player Loading");
    }

    private void IncrementFish(int deviceIndex) {
        // Increments before modulus
        displayedFishIndices[deviceIndex] = (++displayedFishIndices[deviceIndex]) % fishes.Count;
        DisplayFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DecrementFish(int deviceIndex) {
        // Decrements before comparison
        if (--displayedFishIndices[deviceIndex] < 0) {
            displayedFishIndices[deviceIndex] += fishes.Count;
        }
        DisplayFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DisplayFish(int deviceIndex, GunfishData fish) {
        var material = fish.spriteMat;
        var texture = material.mainTexture as Texture2D;
        
        fishImages[deviceIndex].style.backgroundImage = texture;
    }
}
