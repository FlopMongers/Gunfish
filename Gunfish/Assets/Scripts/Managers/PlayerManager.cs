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

    public int playerThreshold = 4;

    private void JoinPlayers() {
        PlayerInputs = new List<PlayerInput>();
        Players = new List<Player>();
        PlayerFish = new List<GunfishData>();

        var inputManager = GetComponent<PlayerInputManager>();

        var pattern = GameManager.Instance.debug == true ? "(Keyboard|Controller|Joystick)" : "(Controller|Joystick)";
        var regex = new Regex(pattern);
        var inputDevices = InputSystem.devices.Where(device => regex.IsMatch(device.displayName)).OrderBy(device => device.deviceId).ToList();

        int playerIndex = 0;
        foreach (var device in inputDevices)
            print(device);
        inputDevices.ForEach(device => {
            var playerInput = inputManager.JoinPlayer(playerIndex: playerIndex, pairWithDevice: device);
            var player = playerInput.GetComponent<Player>();
            player.Initialize(playerIndex);
            PlayerInputs.Add(playerInput);
            Players.Add(player);
            PlayerFish.Add(GameManager.Instance.GunfishDataList.gunfishes[0]);
            playerIndex++;
        });
    }

    public override void Initialize() {
        base.Initialize();

        JoinPlayers();
        SetInputMode(InputMode.UI);
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