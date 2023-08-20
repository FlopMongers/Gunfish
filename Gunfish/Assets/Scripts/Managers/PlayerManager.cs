using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : Singleton<PlayerManager> {
    public List<Player> Players { get; private set; }
    public List<GunfishData> PlayerFish { get; private set; }
    public List<PlayerInput> PlayerInputs { get; private set; }

    private void Start() {
        JoinPlayers();
        SetInputMode(InputMode.UI);
        MainMenu.instance.InitializeMenu();
    }

    private void JoinPlayers() {
        PlayerInputs = new List<PlayerInput>();
        Players = new List<Player>();
        PlayerFish = new List<GunfishData>();

        var inputManager = GetComponent<PlayerInputManager>();

        var pattern = GameManager.debug == true ? "(Keyboard|Controller|Joystick)" : "Joystick";
        var regex = new Regex(pattern);
        var inputDevices = InputSystem.devices.Where(device => regex.IsMatch(device.displayName)).OrderBy(device => device.deviceId).ToList();
        
        int playerIndex = 0;
        inputDevices.ForEach(device => {
            var playerInput = inputManager.JoinPlayer(playerIndex: playerIndex++, pairWithDevice: device);
            var player = playerInput.GetComponent<Player>();
            PlayerInputs.Add(playerInput);
            Players.Add(player);
            PlayerFish.Add(GameManager.instance.GunfishList[0]);
        });
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
