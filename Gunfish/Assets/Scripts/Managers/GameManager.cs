using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameParameters {
    public List<Player> activePlayers;
    public GameObject gameUIObject;
    public List<Scene> scenes;

    public GameParameters(List<Player> activePlayers, GameObject gameUIObject, List<Scene> scenes) {
        this.activePlayers = activePlayers;
        this.gameUIObject = gameUIObject;
        this.scenes = scenes;
    }
}

public class GameManager : PersistentSingleton<GameManager> {
    public static readonly bool debug = true;
    public List<GameMode> GameModeList = new List<GameMode>();
    public List<GunfishData> GunfishList = new List<GunfishData>();
    public GameMode selectedGameMode;
    
    private Dictionary<GameModeType, GameMode> gameModeMap = new Dictionary<GameModeType, GameMode>();
    private MatchManager matchManager;


    protected override void Awake() {
        base.Awake();
        foreach (GameMode gameMode in GameModeList) {
            gameModeMap[gameMode.gameModeType] = gameMode;
        }
    }

    public void InitializeGame(GameModeType gameMode, List<Scene> scenes) {
        // spawn match manager
        matchManager = Instantiate(gameModeMap[gameMode].matchManagerPrefab).GetComponent<MatchManager>();
        // get all active players
        GameParameters parameters = new GameParameters(players, gameModeMap[gameMode].gameUIObject, scenes);
        matchManager.Initialize(parameters);
    }


}
