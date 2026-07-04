using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArcadeJoinStrategy : IPlayerJoinStrategy {
    private PlayerManager owner;
    private bool showDebugMessage;
    private int playerThreshold;

    public void Initialize(PlayerManager owner) {
        this.owner = owner;
        showDebugMessage = true;

        if (GameManager.Instance.debug) {
            playerThreshold = GameManager.Instance.debugPlayerCount;
            DebugRegistrar.Track("PlayerManager.RequiredPlayerCount", () =>
                $"OVERRIDDEN -> {GameManager.Instance.debugPlayerCount} " +
                $"(prod would require {owner.GetComponent<PlayerInputManager>().maxPlayerCount})");
        } else {
            playerThreshold = owner.GetComponent<PlayerInputManager>().maxPlayerCount;
        }

        owner.PlayerInputs = new List<PlayerInput>();
        owner.Players = new List<Player>();
        owner.PlayerFish = new List<GunfishData>();
    }

    public void OnPlayerJoined(PlayerInput input) {
        owner.PlayerInputs.Add(input);

        if (owner.PlayerInputs.Count == playerThreshold) {
            showDebugMessage = false;
            InitializePlayers();
            GameManager.Instance.InitializePostRitualManagers();
        }
    }

    public void OnPlayerLeft(PlayerInput input) {
        Debug.Log($"Player {input.name} has been disconnected.");
        owner.PlayerInputs.Remove(input);
    }

    public void OnDeviceLost(Player player) {
        player.FreezeControls = true;
    }

    public void OnDeviceRegained(Player player) {
        player.FreezeControls = false;
    }

    private void InitializePlayers() {
        owner.SetInputMode(PlayerManager.InputMode.UI);
        for (int playerIndex = 0; playerIndex < owner.PlayerInputs.Count; playerIndex++) {
            var playerInput = owner.PlayerInputs[playerIndex];
            var player = playerInput.GetComponent<Player>();
            player.Initialize(playerIndex);
            owner.Players.Add(player);
            owner.PlayerFish.Add(null);
        }
    }

    public void OnGUI() {
        if (!showDebugMessage) return;
        GUIStyle style = new GUIStyle(GUI.skin.textArea) {
            fontSize = 30,
            wordWrap = true
        };

        GUILayout.TextField(
            "Welcome to Gunfish! If you're seeing this message it means this game is still initializing. Please press the GUN button for each controller in the following order: RED, GREEN, BLUE, YELLOW.",
            style
        );
    }
}
