using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SplashMenuPage : MenuPage {
    private MenuPageContext menuContext;
    private bool isLoadingNextMenu;
    private List<PlayerInput> wiredPlayerInputs;

    public override void OnPageStart(MenuPageContext context) {
        base.OnPageStart(context);
        menuContext = context;
        isLoadingNextMenu = false;
        ArduinoManager.Instance.playAttractors = true;

        wiredPlayerInputs = new List<PlayerInput>(new PlayerInput[PlayerManager.Instance.PlayerInputs.Count]);

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            WireSlot(i);
        }

        PlayerManager.Instance.OnSlotJoined += WireSlot;
        PlayerManager.Instance.OnSlotLeft += UnwireSlot;
    }

    public override void OnPageStop(MenuPageContext context) {
        PlayerManager.Instance.OnSlotJoined -= WireSlot;
        PlayerManager.Instance.OnSlotLeft -= UnwireSlot;

        for (int i = 0; i < wiredPlayerInputs.Count; i++) {
            UnwireSlot(i);
        }
        base.OnPageStop(context);
    }

    private void WireSlot(int playerIndex) {
        var playerInput = PlayerManager.Instance.PlayerInputs[playerIndex];
        if (playerInput == null) return;

        wiredPlayerInputs[playerIndex] = playerInput;
        playerInput.currentActionMap.FindAction("Any").performed += OnAnyKey;
    }

    private void UnwireSlot(int playerIndex) {
        var playerInput = wiredPlayerInputs[playerIndex];
        if (playerInput != null) {
            playerInput.currentActionMap.FindAction("Any").performed -= OnAnyKey;
        }
        wiredPlayerInputs[playerIndex] = null;
    }

    private void OnAnyKey(InputAction.CallbackContext context) {
        if (isLoadingNextMenu == false) {
            isLoadingNextMenu = true;
            ArduinoManager.Instance.playAttractors = false;
            FX_Spawner.Instance.SpawnFX(FXType.TitleScreenStartFX, Camera.main.transform.position, Quaternion.identity);
            DOTween.Sequence().AppendInterval(1).AppendCallback(LoadNextMenu);
        }
    }

    private void LoadNextMenu() {
        menuContext.menu.SetState(MenuState.GameModeSelect);
    }
}