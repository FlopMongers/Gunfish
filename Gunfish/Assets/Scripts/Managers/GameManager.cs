using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
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
    [SerializeField]
    [FormerlySerializedAs("debug")]
    private bool _debug = false;
    public bool debug {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetDebug(out var debugOverride)) return debugOverride;
#endif
            return _debug;
        }
    }

    [SerializeField]
    [FormerlySerializedAs("debugPlayerCount")]
    private int _debugPlayerCount = 1;
    public int debugPlayerCount {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetDebugPlayerCount(out var debugPlayerCountOverride)) return debugPlayerCountOverride;
#endif
            return _debugPlayerCount;
        }
    }

    public bool useSavedVolumes = false;

    [SerializeField]
    private GameModeList _gameModeList;
    public GameModeList GameModeList {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetGameModeList(out var gameModeListOverride)) return gameModeListOverride;
#endif
            return _gameModeList;
        }
    }

    [SerializeField]
    private GunfishDataList _gunfishDataList;
    public GunfishDataList GunfishDataList {
        get {
#if UNITY_EDITOR
            if (DevConfigOverride.TryGetGunfishDataList(out var gunfishDataListOverride)) return gunfishDataListOverride;
#endif
            return _gunfishDataList;
        }
    }

    private GameMode selectedGameMode;
    public GameModeType defaultGameMode;

    public MatchManager<PlayerReference, TeamReference> MatchManager { get; private set; }

    protected override void Awake() {
        base.Awake();
        Cursor.visible = false;
    }

    protected void Start() {
        Initialize();
    }

    public override void Initialize() {
#if UNITY_EDITOR
        RegisterDevConfigOverrideDebugEntries();
#endif
        PlayerManager.Instance.Initialize();
    }

#if UNITY_EDITOR
    private void RegisterDevConfigOverrideDebugEntries() {
        DebugRegistrar.Track("DevConfigOverride.Debug", () =>
            DevConfigOverride.TryGetDebug(out var d) ? $"OVERRIDDEN -> {d}" : "inactive");
        DebugRegistrar.Track("DevConfigOverride.DebugPlayerCount", () =>
            DevConfigOverride.TryGetDebugPlayerCount(out var c) ? $"OVERRIDDEN -> {c}" : "inactive");
        DebugRegistrar.Track("DevConfigOverride.GameModeList", () =>
            DevConfigOverride.TryGetGameModeList(out var l) ? $"OVERRIDDEN -> {l.name}" : "inactive");
        DebugRegistrar.Track("DevConfigOverride.GunfishDataList", () =>
            DevConfigOverride.TryGetGunfishDataList(out var l) ? $"OVERRIDDEN -> {l.name}" : "inactive");
    }
#endif

    public void InitializePostRitualManagers() {
        StartCoroutine(InitializePostRitualManagersCR());
    }

    private IEnumerator InitializePostRitualManagersCR() {
        // Yielding for one frame is required due to a bug in Unity.
        yield return new WaitForEndOfFrame();
        LevelManager.Instance.Initialize();
        MusicManager.Instance.Initialize();
        ArduinoManager.Instance.Initialize();
        FX_Spawner.Instance.Initialize();
        MarqueeManager.Instance.Initialize();
        PauseManager.Instance.Initialize();
        GameModeManager.Instance.Initialize();
        MainMenu.Instance.Initialize();
    }

    public void InitializeGame(List<string> forcedLevels = null) {
        // Spawn match manager
        // Get all active players
        GameModeManager.Instance.InitializeGameMode(selectedGameMode, PlayerManager.Instance.Players, forcedLevels);
        MusicManager.Instance.PlayTrackSet(TrackSetLabel.Gameplay);
    }

    public void ResetGame() {
        GameModeManager.Instance.TeardownGameMode();
    }

    
    // NOTE(Wyatt): we should just be using a gamemode scriptable object instead of this enum GARBAGE.
    // whatever. this is just a quick implementation.
    [HideInInspector]
    public GameMode currentGameMode;

    public void SetSelectedGameMode(GameMode gameMode) {
        // currentGameMode = GameModeList.gameModes.Where(element => element.gameModeType == gameMode).FirstOrDefault();
        currentGameMode = gameMode;
        selectedGameMode = gameMode;
    }
}