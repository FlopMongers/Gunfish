using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameParameters {
    public List<Player> activePlayers;
    public List<string> scenes;
    public string skyboxScene;

    public GameParameters(List<Player> activePlayers, List<string> scenes, string skyboxScene) {
        this.activePlayers = activePlayers;
        this.scenes = scenes;
        this.skyboxScene = skyboxScene;
    }
}

public class GameManager : PersistentSingleton<GameManager> {
    public static readonly bool debug = true;
    
    [SerializeField]
    private GameModeList _gameModeList;
    public GameModeList GameModeList { get => _gameModeList; }

    [SerializeField]
    private GunfishDataList _gunfishDataList;
    public GunfishDataList  GunfishDataList { get => _gunfishDataList; }

    private GameModeType selectedGameMode;

    public MatchManager MatchManager { get; private set; }

    protected override void Awake() {
        base.Awake();
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
        GameModeManager.Instance.Initialize();
        MainMenu.Instance.Initialize();
    }

    public void InitializeGame() {
        // Spawn match manager
        // Get all active players
        GameModeManager.Instance.InitializeGameMode(selectedGameMode, PlayerManager.Instance.Players);
        MusicManager.Instance.PlayTrackSet(TrackSetLabel.Gameplay);
    }

    public void ResetGame() {
        GameModeManager.Instance.TeardownGameMode();
    }

    public void SetSelectedGameMode(GameModeType gameMode) {
        selectedGameMode = gameMode;
    }
}