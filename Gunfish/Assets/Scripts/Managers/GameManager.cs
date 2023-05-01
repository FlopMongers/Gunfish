using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameParameters
{
    public List<Player> activePlayers;
    public GameObject gameUIObject;
    public List<Scene> scenes;

    public GameParameters(List<Player> activePlayers, GameObject gameUIObject)
    {
        this.activePlayers = activePlayers;
        this.gameUIObject = gameUIObject;
    }
}


public class GameManager : Singleton<GameManager> {
    public static readonly bool debug = true;
    public List<GameMode> GameMode_List;
    Dictionary<GameModeType, GameMode> gameMode_Map;
    public GameMode selectedGameMode;

    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private Player player;

    List<Player> players;

    void Start() {

    }

    void Update() {

    }

    public void InitializeGame(GameModeType gameMode) {
        // spawn match manager
        matchManager = Instantiate(gameMode_Map[gameMode].matchManagerPrefab).GetComponent<MatchManager>();
        // get all active players
        GameParameters parameters = new GameParameters(players, gameMode_Map[gameMode].gameUIObject);
        matchManager.Initialize(parameters);
    }


}
