using UnityEngine.UIElements;

public interface IMenuPage {
    public void OnEnable(MenuPageContext context);
    public void OnDisable(MenuPageContext context);
    public void OnUpdate(MenuPageContext context);
}