using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class PlayerManager : Singleton<PlayerManager> {

    public List<InputDevice> PlayerDevices { get; private set; }

    public void OnPlayerJoined(PlayerInput input) {
        print($"Player joined with controller id {input.devices[0].deviceId}");
    }

    public void OnPlayerLeft(PlayerInput input) {
        print($"Player exited with controller id {input.devices[0].deviceId}");
    }

    protected override void Awake() {
        base.Awake();
        PlayerDevices = new List<InputDevice>();
        JoinPlayers();
    }

    private void JoinPlayers() {
        var inputManager = GetComponent<PlayerInputManager>();
        int index = 0;

        // Real devices need to be placed at specific indices.
        // Since connection order is not guaranteed, have to have capacity ready.
        if (GameManager.debug == false) {
            PlayerDevices.Add(null);
            PlayerDevices.Add(null);
            PlayerDevices.Add(null);
            PlayerDevices.Add(null);
        }

        foreach (var device in InputSystem.devices)
        {
            print($"{device.deviceId} - {device.displayName} - {device.description} - {device.name} - {device.path} - {device.shortDisplayName}");

            if (GameManager.debug) {
                if (device.displayName.Contains("Controller") || device.displayName.Contains("Keyboard")) {
                    PlayerDevices.Add(device);
                    inputManager.JoinPlayer(playerIndex: index++, pairWithDevice: device);
                }
            } else {
                // TODO: Replace mock device IDs
                // Player 1
                if (device.deviceId == 1337) {
                    PlayerDevices.Insert(0, device);
                    var player = inputManager.JoinPlayer(playerIndex: 0, pairWithDevice: device);
                }
                // Player 2
                else if (device.deviceId == 1338) {
                    PlayerDevices.Insert(1, device);
                    inputManager.JoinPlayer(playerIndex: 1, pairWithDevice: device);
                }
                // Player 3
                else if (device.deviceId == 1339) {
                    PlayerDevices.Insert(2, device);
                    inputManager.JoinPlayer(playerIndex: 2, pairWithDevice: device);
                }
                // Player 4
                else if (device.deviceId == 1340) {
                    PlayerDevices.Insert(3, device);
                    inputManager.JoinPlayer(playerIndex: 3, pairWithDevice: device);
                }
            }
        }
    }
}
