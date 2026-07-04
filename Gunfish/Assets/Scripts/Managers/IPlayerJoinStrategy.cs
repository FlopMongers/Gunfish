using UnityEngine.InputSystem;

public interface IPlayerJoinStrategy {
    void Initialize(PlayerManager owner);
    void OnPlayerJoined(PlayerInput input);
    void OnPlayerLeft(PlayerInput input);
    void OnDeviceLost(Player player);
    void OnDeviceRegained(Player player);
    void OnGUI();
}
