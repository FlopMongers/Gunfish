using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : Singleton<PlayerManager> {
    // TODO: Replace mock device IDs
    public static readonly int playerOneDeviceId = 1337;
    public static readonly int playerTwoDeviceId = 1338;
    public static readonly int playerThreeDeviceId = 1339;
    public static readonly int playerFourDeviceId = 1340;

    public List<PlayerInput> PlayerInputs { get; private set; }

    protected override void Awake() {
        base.Awake();
        PlayerInputs = new List<PlayerInput>();
        JoinPlayers();
    }

    private void JoinPlayers() {
        var inputManager = GetComponent<PlayerInputManager>();
        int index = 0;

        // Real devices need to be placed at specific indices.
        // Since connection order is not guaranteed, have to have capacity ready.
        if (GameManager.debug == false) {
            PlayerInputs.Add(null);
            PlayerInputs.Add(null);
            PlayerInputs.Add(null);
            PlayerInputs.Add(null);
        }

        foreach (var device in InputSystem.devices) {
            if (GameManager.debug == false) {
                // Player 1
                if (device.deviceId == PlayerManager.playerOneDeviceId) {
                    PlayerInputs.Add(inputManager.JoinPlayer(playerIndex: 0, pairWithDevice: device));
                }
                // Player 2
                else if (device.deviceId == PlayerManager.playerTwoDeviceId) {
                    PlayerInputs.Add(inputManager.JoinPlayer(playerIndex: 1, pairWithDevice: device));
                }
                // Player 3
                else if (device.deviceId == PlayerManager.playerThreeDeviceId) {
                    PlayerInputs.Add(inputManager.JoinPlayer(playerIndex: 2, pairWithDevice: device));
                }
                // Player 4
                else if (device.deviceId == PlayerManager.playerFourDeviceId) {
                    PlayerInputs.Add(inputManager.JoinPlayer(playerIndex: 3, pairWithDevice: device));
                }
            } else {
                if (device.displayName.Contains("Controller") || device.displayName.Contains("Keyboard")) {
                    PlayerInputs.Add(inputManager.JoinPlayer(playerIndex: index++, pairWithDevice: device));
                }
            }
        }
    }

    public void SetInputMode(InputMode inputMode) {
        foreach (var playerInput in PlayerInputs) {
            playerInput.SwitchCurrentActionMap(inputMode.ToString());
        }
    }

    // Must be either Player or UI
    public enum InputMode {
        Player,
        UI,
    }
}
