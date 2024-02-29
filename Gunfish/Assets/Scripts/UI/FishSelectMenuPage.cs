using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class FishSelectMenuPage : MenuPage {
    private MenuPageContext menuContext;
    [SerializeField] private List<FishSelectPanel> fishSelectPanels;
    private List<int> gunfishIndices;

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

    public override void OnPageStart(MenuPageContext context) {
        base.OnPageStart(context);
        MarqueeManager.Instance.PlayRandomQuip(QuipType.FishSelection);
        menuContext = context;

        print("onPageStart");

        playerActions = new List<PlayerAction>();
        gunfishIndices = new List<int>();
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            int playerIndex = i;
            var playerInput = PlayerManager.Instance.PlayerInputs[playerIndex];
            var fishSelectPanel = fishSelectPanels[playerIndex];
            gunfishIndices.Add(0);
            print($"{fishSelectPanel} - {playerIndex}");

            PlayerAction playerAction = new PlayerAction(
                (InputAction.CallbackContext context) => OnNavigate(context, playerIndex),
                (InputAction.CallbackContext context) => OnSubmit(context, playerIndex),
                (InputAction.CallbackContext context) => OnCancel(context, playerIndex)
            );
            playerActions.Add(playerAction);
            playerInput.currentActionMap.FindAction("Navigate").performed += playerAction.navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed += playerAction.submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed += playerAction.cancelPerformed;

            var color = PlayerManager.Instance.playerColors[playerIndex];

            fishSelectPanel.Initialize();
            fishSelectPanel.SetColor(color);
            fishSelectPanel.SetState(FishSelectPanel.State.Inactive);
        }

        Fade();
        DOTween.Sequence().AppendInterval(0.01f).AppendCallback(Unfade);
    }

    public override void OnPageStop(MenuPageContext context) {
        ArduinoManager.Instance.playAttractors = false;

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.Instance.PlayerInputs[i];
            playerInput.currentActionMap.FindAction("Navigate").performed -= playerActions[i].navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed -= playerActions[i].submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed -= playerActions[i].cancelPerformed;
        }
        base.OnPageStop(context);
    }

    private void OnNavigate(InputAction.CallbackContext context, int deviceIndex) {
        if (fishSelectPanels[deviceIndex].state != FishSelectPanel.State.Selecting || context.canceled) {
            return;
        }

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

    private void OnSubmit(InputAction.CallbackContext context, int playerIndex) {
        var fishSelectPanel = fishSelectPanels[playerIndex];
        print($"{fishSelectPanel} - {playerIndex}");
        switch (fishSelectPanel.state) {
            case FishSelectPanel.State.Inactive:
                PlayerManager.Instance.Players[playerIndex].Active = false;
                SetFish(playerIndex, GameManager.Instance.GunfishDataList.gunfishes[0]);
                CancelGameStartCountdown();
                fishSelectPanel.SetState(FishSelectPanel.State.Selecting);
                break;
            case FishSelectPanel.State.Selecting:
                PlayerManager.Instance.Players[playerIndex].Active = true;
                fishSelectPanel.SetState(FishSelectPanel.State.Confirmed);
                if (AllPlayersReady()) {
                    BeginGameStartCountdown();
                }
                break;
            case FishSelectPanel.State.Confirmed:
                if (AllPlayersReady()) {
                    BeginGameStartCountdown();
                }
                break;
        }
    }

    private void OnCancel(InputAction.CallbackContext context, int playerIndex) {
        var fishSelectPanel = fishSelectPanels[playerIndex];
        switch (fishSelectPanel.state) {
            case FishSelectPanel.State.Selecting:
                PlayerManager.Instance.Players[playerIndex].Active = false;
                fishSelectPanel.SetState(FishSelectPanel.State.Inactive);
                break;
            case FishSelectPanel.State.Confirmed:
                PlayerManager.Instance.Players[playerIndex].Active = false;
                CancelGameStartCountdown();
                fishSelectPanel.SetState(FishSelectPanel.State.Selecting);
                break;
        }
    }

    private void IncrementFish(int deviceIndex) {
        var currentIndex = gunfishIndices[deviceIndex];
        var fishCount = GameManager.Instance.GunfishDataList.gunfishes.Count;
        gunfishIndices[deviceIndex] = (currentIndex + 1) % fishCount;
        SetFish(deviceIndex, GameManager.Instance.GunfishDataList.gunfishes[gunfishIndices[deviceIndex]]);
    }

    private void DecrementFish(int deviceIndex) {
        var currentIndex = gunfishIndices[deviceIndex];
        var fishCount = GameManager.Instance.GunfishDataList.gunfishes.Count;
        if (currentIndex == 0) {
            gunfishIndices[deviceIndex] = currentIndex + fishCount - 1;
        } else {
            gunfishIndices[deviceIndex] = currentIndex - 1;
        }
        SetFish(deviceIndex, GameManager.Instance.GunfishDataList.gunfishes[gunfishIndices[deviceIndex]]);
    }

    private void SetFish(int deviceIndex, GunfishData fish) {
        fishSelectPanels[deviceIndex].SetFishImage(fish.sprite);
        PlayerManager.Instance.SetPlayerFish(deviceIndex, fish);
    }

    private void Unfade() {
        //menuContext.document.rootVisualElement.Q("MenuContainer").RemoveFromClassList("faded");
    }

    private void Fade() {
        //menuContext.document.rootVisualElement.Q("MenuContainer").AddToClassList("faded");
    }

    private void BeginGameStartCountdown() {
        if (activeGameStartCountdown != null && activeGameStartCountdown.active) {
            return;
        }
        CancelGameStartCountdown();
        activeGameStartCountdown = DOTween.Sequence().AppendInterval(2).OnComplete(() => {
            OnPageStop(menuContext);
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

    private bool AllPlayersReady() {
        bool hasNoSelecting = true;
        int readyPlayerCount = 0;
        var requiredPlayersToStart = GameManager.Instance.debug ? 1 : 2;
        foreach (var fishSelectPanel in fishSelectPanels) {
            if (fishSelectPanel.state == FishSelectPanel.State.Confirmed) {
                readyPlayerCount++;
            }
            if (fishSelectPanel.state == FishSelectPanel.State.Selecting) {
                hasNoSelecting = false;
            }
        }
        return hasNoSelecting && readyPlayerCount >= requiredPlayersToStart;
    }
}