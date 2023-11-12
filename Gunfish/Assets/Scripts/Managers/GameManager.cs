using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameParameters {
    public List<Player> activePlayers;
    public List<string> scenes;

    public GameParameters(List<Player> activePlayers, List<string> scenes) {
        this.activePlayers = activePlayers;
        this.scenes = scenes;
    }
}

public class GameManager : PersistentSingleton<GameManager> {
    public static readonly bool debug = true;
    public List<string> testLevelList = new List<string>();

    public List<GameMode> GameModeList = new List<GameMode>();
    public List<GunfishData> GunfishList = new List<GunfishData>();

    private GameObject gameModeObject;
    private GameModeType selectedGameMode;

    private Dictionary<GameModeType, GameMode> gameModeMap = new Dictionary<GameModeType, GameMode>();
    public MatchManager MatchManager { get; private set; }


    protected override void Awake() {
        base.Awake();
        foreach (GameMode gameMode in GameModeList) {
            gameModeMap[gameMode.gameModeType] = gameMode;
        }
    }

    protected void Start() {
        Initialize();
    }

    public override void Initialize() {
        PlayerManager.Instance.Initialize();
        LevelManager.Instance.Initialize();
        MusicManager.Instance.Initialize();
        ArduinoManager.Instance.Initialize();
        FX_Spawner.Instance.Initialize();
        MarqueeManager.Instance.Initialize();
        PauseManager.Instance.Initialize();
        MainMenu.Instance.Initialize();
    }

    public void InitializeGame() {
        // Spawn match manager
        if (selectedGameMode == GameModeType.DeathMatch) {
            gameModeObject = Instantiate(gameModeMap[selectedGameMode].matchManagerPrefab);
            MatchManager = gameModeObject.GetComponent<MatchManager>();
        }
        else if (selectedGameMode == GameModeType.Race) {
            MatchManager = null;
        }

        var scenes = testLevelList; //new List<string>() { "Sea Urchin Testing" };

        // Get all active players
        GameParameters parameters = new GameParameters(PlayerManager.Instance.Players, scenes);
        MatchManager?.Initialize(parameters);
    }

    public void SetSelectedGameMode(GameModeType gameMode) {
        selectedGameMode = gameMode;
    }
}