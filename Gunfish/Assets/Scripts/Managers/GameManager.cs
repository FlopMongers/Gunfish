using System.Collections.Generic;
using UnityEngine;

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
    
    [SerializeField]
    public GameModeList _gameModeList;
    public GameModeList GameModeList { get => _gameModeList; }

    [SerializeField]
    public GunfishDataList _gunfishDataList;
    public GunfishDataList GunfishDataList { get => _gunfishDataList; }

    private GameObject gameModeObject;
    private GameModeType selectedGameMode;

    private Dictionary<GameModeType, GameMode> gameModeMap = new Dictionary<GameModeType, GameMode>();
    public MatchManager MatchManager { get; private set; }


    protected override void Awake() {
        base.Awake();
        foreach (GameMode gameMode in GameModeList.gameModes) {
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
        gameModeObject = Instantiate(gameModeMap[selectedGameMode].matchManagerPrefab);
        MatchManager = gameModeObject.GetComponent<MatchManager>();

        var scenes = gameModeMap[selectedGameMode].levels.sceneNames;

        // Get all active players
        GameParameters parameters = new GameParameters(PlayerManager.Instance.Players, scenes);
        MatchManager?.Initialize(parameters);
    }

    public void SetSelectedGameMode(GameModeType gameMode) {
        selectedGameMode = gameMode;
    }
}