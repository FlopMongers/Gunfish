using UnityEngine.InputSystem;

public interface IGunfishController {
    public void OnMove(InputValue value);
    public void OnFire(InputValue value);
}
