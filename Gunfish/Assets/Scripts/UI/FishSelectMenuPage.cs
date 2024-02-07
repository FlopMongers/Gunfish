using DG.Tweening;
using System.Collections.Generic;
using System.Drawing.Printing;
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
    private List<SelectorState> selectorState;
    private enum SelectorState {
        DISABLED,
        SELECTING,
        READY
    };

    private Sequence activeGameStartCountdown;

    protected struct PlayerAction {
        public System.Action<InputAction.CallbackContext> navigatePerformed;
        public System.Action<InputAction.CallbackContext> submitPerformed;
        public System.Action<InputAction.CallbackContext> cancelPerformed;

        // Constructor to initialize the struct fields
        public PlayerAction(
            System.Action<InputAction.CallbackContext> navigateAction,
            System.Action<InputAction.CallbackContext> submitAction,
            System.Action<InputAction.CallbackContext> cancelAction) {
            navigatePerformed = navigateAction;
            submitPerformed = submitAction;
            cancelPerformed = cancelAction;
        }
    }
    private List<PlayerAction> playerActions;

    public void OnEnable(MenuPageContext context) {
        MarqueeManager.Instance.PlayRandomQuip(QuipType.FishSelection);
        menuContext = context;

        fishImages = new List<VisualElement>();
        selectorState = new List<SelectorState>();
        playerActions = new List<PlayerAction>();
        //int testing = 0;
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            //Debug.Log(PlayerManager.Instance.PlayerInputs.Count);
            //Debug.Log(testing++);
            var playerInput = PlayerManager.Instance.PlayerInputs[i];
            int playerIndex = i;
            //Debug.Log(testing++);
            PlayerAction playerAction = new PlayerAction(
                (InputAction.CallbackContext context) => OnNavigate(context, playerIndex),
                (InputAction.CallbackContext context) => OnSubmit(context, playerIndex),
                (InputAction.CallbackContext context) => OnCancel(context, playerIndex)
            );
            //Debug.Log(testing++);
            playerActions.Add(playerAction);
            //Debug.Log(testing++);
            //Debug.Log("ActionMap: " + playerInput.currentActionMap);
            playerInput.currentActionMap.FindAction("Navigate").performed += playerAction.navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed += playerAction.submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed += playerAction.cancelPerformed;
            //Debug.Log(testing++);

            var fishSelector = menuContext.document.rootVisualElement.Q<VisualElement>($"FishSelector{playerIndex + 1}");

            //Debug.Log("Fish Selector: " + fishSelector.name);

            var color = PlayerManager.Instance.playerColors[playerIndex];

            fishSelector.Q<VisualElement>("back-button").style.unityBackgroundImageTintColor = new StyleColor(color);
            fishSelector.Q<VisualElement>("next-button").style.unityBackgroundImageTintColor = new StyleColor(color);

            var image = fishSelector.Q<VisualElement>("fish-image");

            fishImages.Add(image);
            selectorState.Add(SelectorState.DISABLED);
        }

        displayedFishes = new List<GunfishData>();
        displayedFishIndices = new List<int>();

        fishes = GameManager.Instance.GunfishDataList.gunfishes;

        if (fishes.Count < 1) {
            throw new UnityException("GameManager.Gunfish_List must contain at least one fish for the game to function.");
        }

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            displayedFishes.Add(null);
            displayedFishIndices.Add(0);
            SetFish(i, null);
            SetSelectorState(i, SelectorState.DISABLED);
        }

        Fade();
        DOTween.Sequence().AppendInterval(0.01f).AppendCallback(Unfade);
    }

    public void OnDisable(MenuPageContext context) {
        ArduinoManager.Instance.playAttractors = false;

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.Instance.PlayerInputs[i];
            playerInput.currentActionMap.FindAction("Navigate").performed -= playerActions[i].navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed -= playerActions[i].submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed -= playerActions[i].cancelPerformed;
        }
    }

    public void OnUpdate(MenuPageContext context) {

    }

    private void OnNavigate(InputAction.CallbackContext context, int deviceIndex) {
        if (selectorState[deviceIndex] != SelectorState.SELECTING || context.canceled)
            return;

        var direction = context.ReadValue<Vector2>();

        // Horizontal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            menuContext.menu.PlayBloop();
            if (direction.x > 0) {
                IncrementFish(deviceIndex);
            } else {
                DecrementFish(deviceIndex);
            }
        }
    }

    private void OnSubmit(InputAction.CallbackContext context, int deviceIndex) {
        switch (selectorState[deviceIndex]) {
        case SelectorState.DISABLED:
            PlayerManager.Instance.Players[deviceIndex].Active = false;
            SetFish(deviceIndex, GameManager.Instance.GunfishDataList.gunfishes[0]);
            CancelGameStartCountdown();
            SetSelectorState(deviceIndex, SelectorState.SELECTING);
            break;
        case SelectorState.SELECTING:
            PlayerManager.Instance.Players[deviceIndex].Active = true;
            SetSelectorState(deviceIndex, SelectorState.READY);
            if (isAllPlayersReady()) {
                BeginGameStartCountdown();
            }
            break;
        case SelectorState.READY:
            if (isAllPlayersReady()) {
                BeginGameStartCountdown();
            }
            break;
        }
    }

    private void OnCancel(InputAction.CallbackContext context, int deviceIndex) {
        switch (selectorState[deviceIndex]) {
        case SelectorState.SELECTING:
            PlayerManager.Instance.Players[deviceIndex].Active = false;
            SetSelectorState(deviceIndex, SelectorState.DISABLED);
            break;
        case SelectorState.READY:
            PlayerManager.Instance.Players[deviceIndex].Active = false;
            CancelGameStartCountdown();
            SetSelectorState(deviceIndex, SelectorState.SELECTING);
            break;
        }
    }

    private void IncrementFish(int deviceIndex) {
        // Increments before modulus
        displayedFishIndices[deviceIndex] = (++displayedFishIndices[deviceIndex]) % fishes.Count;
        SetFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DecrementFish(int deviceIndex) {
        // Decrements before comparison
        if (--displayedFishIndices[deviceIndex] < 0) {
            displayedFishIndices[deviceIndex] += fishes.Count;
        }
        SetFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void SetFish(int deviceIndex, GunfishData fish) {
        Texture2D texture = null;
        if (fish != null) {
            texture = fish.sprite.texture as Texture2D;
        }

        fishImages[deviceIndex].style.backgroundImage = texture;

        PlayerManager.Instance.SetPlayerFish(deviceIndex, fish);
    }

    private void SetSelectorState(int deviceIndex, SelectorState newState) {
        VisualElement fishSelector = menuContext.document.rootVisualElement.Q<VisualElement>($"FishSelector{deviceIndex + 1}");
        switch (newState) {
        case SelectorState.DISABLED:
            SetFish(deviceIndex, null);
            fishSelector.Q<VisualElement>("CancelHint").visible = false;
            fishSelector.Q<VisualElement>("ConfirmHint").visible = false;
            fishSelector.Q<VisualElement>("DisabledHint").visible = true;
            fishSelector.Q<VisualElement>("ReadyHint").visible = false;
            selectorState[deviceIndex] = newState;
            break;
        case SelectorState.SELECTING:
            fishSelector.Q<VisualElement>("CancelHint").visible = true;
            fishSelector.Q<VisualElement>("ConfirmHint").visible = true;
            fishSelector.Q<VisualElement>("DisabledHint").visible = false;
            fishSelector.Q<VisualElement>("ReadyHint").visible = false;
            fishSelector.Q<VisualElement>("back-button").RemoveFromClassList("ready");
            fishSelector.Q<VisualElement>("next-button").RemoveFromClassList("ready");
            selectorState[deviceIndex] = newState;
            break;
        case SelectorState.READY:
            fishSelector.Q<VisualElement>("CancelHint").visible = true;
            fishSelector.Q<VisualElement>("ConfirmHint").visible = false;
            fishSelector.Q<VisualElement>("DisabledHint").visible = false;
            fishSelector.Q<VisualElement>("ReadyHint").visible = true;
            fishSelector.Q<VisualElement>("back-button").AddToClassList("ready");
            fishSelector.Q<VisualElement>("next-button").AddToClassList("ready");
            selectorState[deviceIndex] = newState;
            break;
        }
    }

    private void Unfade() {
        menuContext.document.rootVisualElement.Q("MenuContainer").RemoveFromClassList("faded");
    }

    private void Fade() {
        menuContext.document.rootVisualElement.Q("MenuContainer").AddToClassList("faded");
    }

    private void BeginGameStartCountdown() {
        if (activeGameStartCountdown != null && activeGameStartCountdown.active) {
            return;
        }
        CancelGameStartCountdown();
        activeGameStartCountdown = DOTween.Sequence().AppendInterval(2).OnComplete(() => {
            OnDisable(menuContext);
            GameManager.Instance.InitializeGame();
        });
    }

    private void CancelGameStartCountdown() {
        if (activeGameStartCountdown != null) {
            Debug.Log("Cancelling game start");
            activeGameStartCountdown.Kill();
            activeGameStartCountdown = null;
        }
    }

    private bool isAllPlayersReady() {
        bool hasNoSelecting = true;
        int readyPlayerCount = 0;
        var requiredPlayersToStart = GameManager.Instance.debug ? 1 : 2;
        foreach (SelectorState state in selectorState) {
            if (state == SelectorState.READY) {
                readyPlayerCount++;
            }
            if (state == SelectorState.SELECTING) {
                hasNoSelecting = false;
            }
        }
        return hasNoSelecting && readyPlayerCount >= requiredPlayersToStart;
    }
}