using UnityEngine.UIElements;

public class GameModeSelectMenuPage : IMenuPage {
    private VisualTreeAsset page;
    public GameModeDetails[] gameModeDetails;

    public void OnEnable(UIDocument document) {
        page = document.visualTreeAsset;
        gameModeDetails = AssetsDAO.LoadScriptableObjects<GameModeDetails>();
    }

    public void OnDisable(UIDocument document) {

    }

    public void OnUpdate(UIDocument document) {

    }
}
