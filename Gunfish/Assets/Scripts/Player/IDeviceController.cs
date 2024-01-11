using UnityEngine.InputSystem;

public interface IDeviceController {
    public void OnDeviceLost(PlayerInput input);
    public void OnDeviceRegained(PlayerInput input);
    public void OnControlsChanged(PlayerInput input);
}