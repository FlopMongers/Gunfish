using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

public class PlayerManager : MonoBehaviour {
    public void OnPlayerJoined(PlayerInput input) {
        print($"Player joined with controller id {input.devices[0].deviceId}");
    }

    public void OnPlayerLeft(PlayerInput input) {
        print($"Player exited with controller id {input.devices[0].deviceId}");
    }

    private void Start() {
        JoinPlayers();
    }

    private void JoinPlayers() {
        var inputManager = GetComponent<PlayerInputManager>();
        int index = 0;
        foreach (var device in InputSystem.devices)
        {
            print($"{device.deviceId} - {device.displayName} - {device.description} - {device.name} - {device.path} - {device.shortDisplayName}");

            if (GameManager.debug) {
                if (device.displayName.Contains("Controller") || device.displayName.Contains("Keyboard")) {
                    inputManager.JoinPlayer(playerIndex: index++, pairWithDevice: device);
                }
            } else {
                // TODO: Specific device IDs for each player
            }
        }
    }
}
