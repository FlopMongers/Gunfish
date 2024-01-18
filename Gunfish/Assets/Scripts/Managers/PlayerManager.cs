using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : PersistentSingleton<PlayerManager> {
    public List<Color> playerColors;

    public List<Player> Players;// { get; private set; }
    public List<GunfishData> PlayerFish;// { get; private set; }
    public List<PlayerInput> PlayerInputs;// { get; private set; }

    private bool showDebugMessage;

    public void OnPlayerJoined(PlayerInput input) {
        PlayerInputs.Add(input);
        Debug.Log("Added player");
        if (PlayerInputs.Count == 1) {
            showDebugMessage = false;
            InitializePlayers();
            GameManager.Instance.InitializeNonPlayerManagerManagersLol();
        }
    }

    public void OnPlayerLeft(PlayerInput input) {
        Debug.LogError($"Player {input.name} has been disconnected.");
        PlayerInputs.Remove(input);
    }

    public override void Initialize() {
        base.Initialize();

        showDebugMessage = true;

        PlayerInputs = new List<PlayerInput>();
        Players = new List<Player>();
        PlayerFish = new List<GunfishData>();

        SetInputMode(InputMode.UI);
    }

    private void InitializePlayers() {
        SetInputMode(InputMode.UI);
        for (int playerIndex = 0; playerIndex < PlayerInputs.Count; playerIndex++)
        {
            var playerInput = PlayerInputs[playerIndex];
            var player = playerInput.GetComponent<Player>();
            player.Initialize(playerIndex);
            Players.Add(player);
            PlayerFish.Add(null);
        }        
    }

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

    public void OnGUI() {
        if (!showDebugMessage) return;
        GUILayout.TextArea("Welcome to Gunfish! If you're seeing this message it means this game is still initializing. Please press the GUN button for each controller in the following order: RED, GREEN, BLUE, YELLOW.");
    }

    // Must be either Player or UI
    public enum InputMode {
        Player,
        UI,
        EndLevel,
        Null,
    }
}