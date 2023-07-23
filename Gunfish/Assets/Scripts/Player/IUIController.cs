using UnityEngine.InputSystem;

public interface IUIController {
    public void OnNavigate(InputValue value);
    public void OnSubmit(InputValue value);
}
