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
        if (Application.isPlaying) {
            throw new UnityException($"Controller with id {input.devices[0].deviceId} was disconnected");
        }
    }

    private void Start() {
        var inputManager = GetComponent<PlayerInputManager>();
        // var inputDeviceMatcher = new InputDeviceMatcher().With
        // InputSystem.RegisterLayoutMatcher()
        int index = 0;
        foreach (var device in InputSystem.devices)
        {
            print($"{device.deviceId} - {device.displayName} - {device.description} - {device.name} - {device.path} - {device.shortDisplayName}");

            if (GameManager.debug) {
                if (device.displayName.Contains("Controller") || device.displayName.Contains("Keyboard")) {
                    inputManager.JoinPlayer(playerIndex: index++, pairWithDevice: device);
                }
            }
        }
        // var inputDevice = InputSystem.AddDevice()
        // inputManager.JoinPlayer(pairWithDevice: )
    }
}
