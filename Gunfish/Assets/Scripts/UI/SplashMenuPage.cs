using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class SplashMenuPage : IMenuPage {
    private MenuPageContext menuContext;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;
        menuContext.devices.ForEach((device) => {

        });
        //context.actionMaps.FindAction("Any").performed += OnAnyKey;
    }

    public void OnDisable(MenuPageContext context) {
        //context.actionMaps.FindAction("Any").performed -= OnAnyKey;
    }

    public void OnUpdate(MenuPageContext context) {
        
    }

    private void OnAnyKey(InputAction.CallbackContext context) {
        menuContext.menu.SetState(MenuState.GameModeSelect);
    }
}
