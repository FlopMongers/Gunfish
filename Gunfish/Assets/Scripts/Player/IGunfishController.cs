using static UnityEngine.InputSystem.InputAction;

public interface IGunfishController {
    public void OnMove(CallbackContext context);
    public void OnFire(CallbackContext context);
}
