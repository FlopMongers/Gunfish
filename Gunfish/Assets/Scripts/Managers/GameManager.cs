using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameParameters
{
    public List<Player> activePlayers;
    public GameObject gameUIObject;
    public List<Scene> scenes;

    public GameParameters(List<Player> activePlayers, GameObject gameUIObject, List<Scene> scenes)
    {
        this.activePlayers = activePlayers;
        this.gameUIObject = gameUIObject;
        this.scenes = scenes;
    }
}


public class GameManager : PersistentSingleton<GameManager> {
    public static readonly bool debug = true;
    public List<GameMode> GameMode_List = new List<GameMode>();
    public List<GunfishData> Gunfish_List = new List<GunfishData>();
    Dictionary<GameModeType, GameMode> gameMode_Map = new Dictionary<GameModeType, GameMode>();
    public GameMode selectedGameMode;

    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private Player player;

    List<Player> players;

    protected override void Awake() {
        base.Awake();
        foreach (GameMode gameMode in GameMode_List)
        {
            gameMode_Map[gameMode.gameModeType] = gameMode;
        }
    }

    void Update() {

    }

    public void InitializeGame(GameModeType gameMode, List<Scene> scenes) {
        // spawn match manager
        matchManager = Instantiate(gameMode_Map[gameMode].matchManagerPrefab).GetComponent<MatchManager>();
        // get all active players
        GameParameters parameters = new GameParameters(players, gameMode_Map[gameMode].gameUIObject, scenes);
        matchManager.Initialize(parameters);
    }


}
