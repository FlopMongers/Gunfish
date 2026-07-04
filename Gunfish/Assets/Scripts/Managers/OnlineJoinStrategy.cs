using System.Collections.Generic;
using UnityEngine.InputSystem;

public class OnlineJoinStrategy : IPlayerJoinStrategy {
    private PlayerManager owner;

    public void Initialize(PlayerManager owner) {
        this.owner = owner;

        var slotCount = owner.GetComponent<PlayerInputManager>().maxPlayerCount;
        owner.PlayerInputs = new List<PlayerInput>(new PlayerInput[slotCount]);
        owner.Players = new List<Player>(new Player[slotCount]);
        owner.PlayerFish = new List<GunfishData>(new GunfishData[slotCount]);

        GameManager.Instance.InitializePostRitualManagers();
    }

    public void OnPlayerJoined(PlayerInput input) {
        var slot = owner.PlayerInputs.IndexOf(null);
        if (slot == -1) return;

        var player = input.GetComponent<Player>();
        player.Initialize(slot);

        owner.PlayerInputs[slot] = input;
        owner.Players[slot] = player;
        owner.PlayerFish[slot] = null;

        owner.NotifySlotJoined(slot);
    }

    public void OnPlayerLeft(PlayerInput input) {
        var slot = owner.PlayerInputs.IndexOf(input);
        if (slot == -1) return;

        owner.PlayerInputs[slot] = null;
        owner.Players[slot] = null;
        owner.PlayerFish[slot] = null;

        owner.NotifySlotLeft(slot);
    }

    public void OnDeviceLost(Player player) {
        player.FreezeControls = true;
    }

    public void OnDeviceRegained(Player player) {
        player.FreezeControls = false;
    }

    public void OnGUI() { }
}
