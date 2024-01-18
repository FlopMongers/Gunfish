using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : PersistentSingleton<PlayerManager> {
    public List<Color> playerColors;

    public List<Player> Players { get; private set; }
    public List<GunfishData> PlayerFish { get; private set; }
    public List<PlayerInput> PlayerInputs { get; private set; }

    public override void Initialize() {
        base.Initialize();
        JoinPlayers();
        SetInputMode(InputMode.UI);
    }

    private void JoinPlayers() {
        PlayerInputs = new List<PlayerInput>();
        Players = new List<Player>();
        PlayerFish = new List<GunfishData>();

        var inputManager = GetComponent<PlayerInputManager>();

        var pattern = GameManager.Instance.debug == true ? "(Keyboard|Controller|Joystick)" : "(Controller|Joystick)";
        var regex = new Regex(pattern);
        var inputDevices = InputSystem.devices.Where(device => regex.IsMatch(device.displayName)).OrderBy(device => device.deviceId).ToList();

        int playerIndex = 0;
        List<InputDevice> devices = new List<InputDevice>(inputDevices);
        List<int> deviceIdPressOrder = new List<int>();
        // List<int> deviceIdPressOrder = GameManager.Instance.controllerPressOrder;
        if (deviceIdPressOrder.Count > 0) {
            devices = new List<InputDevice>();
            List<InputDevice> assignedDevices = new List<InputDevice>();
            deviceIdPressOrder.ForEach(deviceId => {
                inputDevices.ForEach(device => {
                    if (device.deviceId == deviceId) {
                        devices.Add(device);
                    }
                });
            });
        }
        devices.ForEach(device => {
            var playerInput = inputManager.JoinPlayer(playerIndex: playerIndex, pairWithDevice: device);
            var player = playerInput.GetComponent<Player>();
            player.Initialize(playerIndex);
            PlayerInputs.Add(playerInput);
            Players.Add(player);
            PlayerFish.Add(null);
            playerIndex++;
        });
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

    // Must be either Player or UI
    public enum InputMode {
        Player,
        UI,
        EndLevel,
        Null,
    }
}