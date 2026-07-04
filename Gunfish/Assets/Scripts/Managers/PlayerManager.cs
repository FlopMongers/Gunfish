using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : PersistentSingleton<PlayerManager> {
    public List<Color> playerColors;

    public List<Player> Players;
    public List<GunfishData> PlayerFish;
    public List<PlayerInput> PlayerInputs;

    public event Action<int> OnSlotJoined;
    public event Action<int> OnSlotLeft;

    private IPlayerJoinStrategy strategy;

    public override void Initialize() {
        base.Initialize();

#if GUNFISH_ARCADE
        strategy = new ArcadeJoinStrategy();
#else
        strategy = new OnlineJoinStrategy();
#endif
        strategy.Initialize(this);

        SetInputMode(InputMode.UI);
    }

    public void OnPlayerJoined(PlayerInput input) => strategy.OnPlayerJoined(input);

    public void OnPlayerLeft(PlayerInput input) => strategy.OnPlayerLeft(input);

    public void OnDeviceLost(Player player) => strategy.OnDeviceLost(player);

    public void OnDeviceRegained(Player player) => strategy.OnDeviceRegained(player);

    internal void NotifySlotJoined(int slot) => OnSlotJoined?.Invoke(slot);

    internal void NotifySlotLeft(int slot) => OnSlotLeft?.Invoke(slot);

    public void SetPlayerFish(int playerIndex, GunfishData data) {
        if (playerIndex < 0 || playerIndex >= PlayerFish.Count) {
            return;
        }
        PlayerFish[playerIndex] = data;
        Players[playerIndex].gunfishData = data;
        Players[playerIndex].Active = data != null;
    }

    public void SetInputMode(InputMode inputMode) {
        foreach (var playerInput in PlayerInputs) {
            playerInput?.SwitchCurrentActionMap(inputMode.ToString());
        }
    }

    public void OnGUI() => strategy.OnGUI();

    // Must be either Player or UI
    public enum InputMode {
        Player,
        UI,
        EndLevel,
        Null,
    }
}
