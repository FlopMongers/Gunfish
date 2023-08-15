using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : Singleton<PlayerManager> {
    // TODO: Replace mock device IDs
    public static readonly int playerOneDeviceId = 19;
    public static readonly int playerTwoDeviceId = 20;
    public static readonly int playerThreeDeviceId = 11;
    public static readonly int playerFourDeviceId = 17;

    public List<Player> Players { get; private set; }
    public List<GunfishData> PlayerFish { get; private set; }
    public List<PlayerInput> PlayerInputs { get; private set; }

    protected override void Awake() {
        base.Awake();
        Players = new List<Player>();
        PlayerFish = new List<GunfishData>();
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
            //PlayerInputs.Add(null);
            //PlayerInputs.Add(null);
            Players.Add(null);
            Players.Add(null);
            //Players.Add(null);
            //Players.Add(null);
        }

        PlayerInput playerInput;
        foreach (var device in InputSystem.devices) {
            Debug.Log($"Device {device.deviceId}: {device.displayName}");
            PlayerFish.Add(null);
            if (GameManager.debug == false) {
                // Player 1
                if (device.deviceId == playerOneDeviceId) {
                    playerInput = inputManager.JoinPlayer(playerIndex: 0, pairWithDevice: device);
                    PlayerInputs[0] = playerInput;
                    Players[0] = playerInput.GetComponent<Player>();
                }
                // Player 2
                else if (device.deviceId == playerTwoDeviceId) {
                    playerInput = inputManager.JoinPlayer(playerIndex: 1, pairWithDevice: device);
                    PlayerInputs[1] = playerInput;
                    Players[1] = playerInput.GetComponent<Player>();
                }
                // Player 3
                else if (device.deviceId == playerThreeDeviceId) {
                    playerInput = inputManager.JoinPlayer(playerIndex: 2, pairWithDevice: device);
                    PlayerInputs[2] = playerInput;
                    Players[2] = playerInput.GetComponent<Player>();
                }
                // Player 4
                else if (device.deviceId == playerFourDeviceId) {
                    playerInput = inputManager.JoinPlayer(playerIndex: 3, pairWithDevice: device);
                    PlayerInputs[3] = playerInput;
                    Players[3] = playerInput.GetComponent<Player>();
                }
            } else {
                if (device.displayName.Contains("Controller") || device.displayName.Contains("Keyboard") || device.deviceId == 19 || device.deviceId == 20) {
                    playerInput = inputManager.JoinPlayer(playerIndex: index++, pairWithDevice: device);
                    PlayerInputs.Add(playerInput);
                    Players.Add(playerInput.GetComponent<Player>());
                }
            }
        }
    }

    private void Update() {
        // if (Input.GetKeyDown(KeyCode.Space)) {
        //     LoadPlayers();
        // }
    }

    // public void LoadPlayers() {
    //     foreach (var playerInput in PlayerInputs) {
    //         if (!playerInput) continue;
    //         var index = PlayerInputs.IndexOf(playerInput);
    //         var layer = LayerMask.NameToLayer($"Player{index + 1}");
    //         var gunfish = playerInput.GetComponent<Gunfish>();
    //         gunfish.playerNum = index + 1;
    //         gunfish.data = PlayerFish[index];
    //         gunfish.Spawn(gunfish.data, layer);
    //     }
    // }

    public void SetPlayerFish(int playerIndex, GunfishData data) {
        if (playerIndex < 0 || playerIndex >= PlayerFish.Count) {
            return;
        }
        PlayerFish[playerIndex] = data;
        Players[playerIndex].gunfishData = data;
    }

    public void SetInputMode(InputMode inputMode) {
        foreach (var playerInput in PlayerInputs) {
            playerInput?.SwitchCurrentActionMap(inputMode.ToString());
        }
    }

    // Must be either Player or UI
    public enum InputMode {
        Player,
        UI,
        EndLevel,
        Null,
    }
}
