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

        for (int i = 0; i < PlayerManager.instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.instance.PlayerInputs[i];
            Debug.Log($"Adding OnNavigate(context, {i})");
            playerInput.currentActionMap.FindAction("Navigate").performed += (InputAction.CallbackContext context) => OnNavigate(context, i);
            playerInput.currentActionMap.FindAction("Submit").performed += (InputAction.CallbackContext context) => OnSubmit(context, i);
        }
        
        displayedFishes = new List<GunfishData>();
        displayedFishIndices = new List<int>();

        fishes = GameManager.instance.Gunfish_List;

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
            playerInput.currentActionMap.FindAction("Navigate").performed -= (InputAction.CallbackContext context) => OnNavigate(context, i);
            playerInput.currentActionMap.FindAction("Submit").performed -= (InputAction.CallbackContext context) => OnSubmit(context, i);
        }
    }

    public void OnUpdate(MenuPageContext context) {

    }

    private void OnNavigate(InputAction.CallbackContext context, int deviceIndex) {
        var direction = context.ReadValue<Vector2>();
        // Joystick movement should only be registered if it's a full flick
        if (direction.magnitude < 0.9f) {
            return;
        }

        Debug.Log($"OnNavigate for {deviceIndex}");

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
        menuContext.menu.SetState(MenuState.GunfishSelect);
    }

    private void IncrementFish(int deviceIndex) {
        // Increments before modulus
        Debug.Log($"IncrementFish for {deviceIndex}");
        displayedFishIndices[deviceIndex] = (++displayedFishIndices[deviceIndex]) % fishes.Count;
        DisplayFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DecrementFish(int deviceIndex) {
        // Decrements before comparison
        Debug.Log($"DecrementFish for {deviceIndex}");
        if (--displayedFishIndices[deviceIndex] < 0) {
            displayedFishIndices[deviceIndex] += fishes.Count;
        }
        DisplayFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DisplayFish(int deviceIndex, GunfishData fish) {
        displayedFishes[deviceIndex] = fish;
    }
}
