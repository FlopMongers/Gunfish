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
    public static readonly bool debug = false;
    public List<GameMode> GameModeList = new List<GameMode>();
    public List<GunfishData> GunfishList = new List<GunfishData>();
    public GameMode selectedGameMode;
    
    private Dictionary<GameModeType, GameMode> gameModeMap = new Dictionary<GameModeType, GameMode>();
    public MatchManager MatchManager { get; private set; }


    protected override void Awake() {
        base.Awake();
        foreach (GameMode gameMode in GameModeList) {
            gameModeMap[gameMode.gameModeType] = gameMode;
        }
    }

    public void InitializeGame(GameModeType gameMode, List<Scene> scenes) {
        // Spawn match manager
        if (gameMode == GameModeType.DeathMatch) {
            MatchManager = new DeathMatchManager();
        } else if (gameMode == GameModeType.Race) {
            MatchManager = null;
        }
        
        // Get all active players
        GameParameters parameters = new GameParameters(PlayerManager.instance.Players, gameModeMap[gameMode].gameUIObject, scenes);
        MatchManager.Initialize(parameters);
    }


}
