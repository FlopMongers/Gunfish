using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using UnityEngine;

public class PlayerReference {
    public Player player;

    // team number associated with each player
    public TeamReference team;

    public PlayerReference(Player player, TeamReference team) {
        this.player = player;
        this.team = team;
        team.players.Add(this);
    }

    public virtual string GetStatsText() {
        return "";
    }
}

public class TeamReference {
    public List<PlayerReference> players = new List<PlayerReference>();
    public int teamNumber;
    public int VisibleTeamNumber { get { return teamNumber + 1; } }
    public Color teamColor;

    public TeamReference(int teamNumber, Color teamColor) {
        this.teamNumber = teamNumber;
        this.teamColor = teamColor;
    }

    public QuipType GetWinningQuip() {
        if (players.Count > 1) {
            return new QuipType[] { QuipType.Player1Wins, QuipType.Player2Wins, QuipType.Player3Wins, QuipType.Player4Wins }[teamNumber];
        }
        else {
            return new QuipType[] { QuipType.Team1Wins, QuipType.Team2Wins, QuipType.Team3Wins, QuipType.Team4Wins }[teamNumber];
        }
    }

    public string GetTitle() {
        if (players.Count > 1) {
            return $"Team {VisibleTeamNumber}";
        }
        return $"Player {VisibleTeamNumber}";
    }
}

public class ScoredTeamReference : TeamReference {
    public int score;
    public ScoredTeamReference(int teamNumber, Color teamColor) : base(teamNumber, teamColor) {}
}

public class MatchManager<PlayerReferenceType, TeamReferenceType> : MonoBehaviour, IMatchManager where PlayerReferenceType : PlayerReference where TeamReferenceType : TeamReference {
    [HideInInspector]
    public GameParameters parameters;
    protected int currentLevel;

    protected List<Transform> spawnPoints;

    private int nextLevelIndex;
    private bool done;

    public LevelTimer timer;
    static float levelDuration = 90;

    protected float maxNextLevelTimer = 15f;
    protected float nextLevelTimer;
    protected bool waitingForNextLevel = false;

    protected bool endingLevel;
    protected static float endLevelDelay = 0.5f;

    protected List<TeamReferenceType> teams = new List<TeamReferenceType>();
    protected Dictionary<Player, PlayerReferenceType> playerReferences = new Dictionary<Player, PlayerReferenceType>();

    public MatchUI ui;

    public StatsUI statsUI;

    public virtual void Initialize(GameParameters parameters) {
        this.parameters = parameters;
        ui = ui ?? gameObject.GetComponentInChildren<MatchUI>();
        ui.InitializeMatch(parameters.activePlayers);
        Dictionary<int, TeamReferenceType> teamNumbers = new Dictionary<int, TeamReferenceType>();
        foreach (var player in parameters.activePlayers) {
            if (teamNumbers.ContainsKey(player.PlayerNumber) == false) {
                TeamReferenceType TeamRef = GenerateTeamRef(player);
                teamNumbers[player.PlayerNumber] = TeamRef;
                teams.Add(TeamRef);
            }
            AddPlayerReference(player, teamNumbers[player.PlayerNumber]);
        }
        spawnPoints = new List<Transform>();
        LevelManager.Instance.OnFinishLoadLevel += StartLevel;
        LevelManager.Instance.OnStartPlay += StartPlay;
        timer = timer ?? GetComponentInChildren<LevelTimer>();
        if (timer != null) {
            timer.levelDuration = levelDuration;
            timer.OnTimerFinish += OnTimerFinish;
        }
        NextLevel();
    }

    protected virtual void AddPlayerReference(Player player, TeamReference teamRef) {

    }

    protected virtual TeamReferenceType GenerateTeamRef(Player player) {
        return (TeamReferenceType)new TeamReference(player.PlayerNumber, PlayerManager.Instance.playerColors[player.PlayerNumber]);
    }

    void Update() {
        if (waitingForNextLevel) {
            nextLevelTimer -= Time.deltaTime;
            if (nextLevelTimer <= 0) {
                waitingForNextLevel = false;
                NextLevel();
            }
        }
    }

    public void TearDown() {
        LevelManager.Instance.OnFinishLoadLevel -= StartLevel;
        LevelManager.Instance.OnStartPlay -= StartPlay;
    }

    public virtual void SpawnPlayer(Player player) {
        StartCoroutine(CoSpawnPlayer(player));
    }

    protected virtual IEnumerator CoSpawnPlayer(Player player) {
        return null;
    }

    public void FinishSpawningPlayer(Player player) {
        // add player's fish to camera group
        GameCamera.Instance?.targetGroup.AddMember(player.Gunfish.MiddleSegment.transform, 1, 1);
    }

    public virtual void StartLevel() {
        InitializeSpawnPoints();
        FreezeFish(true);
    }


    public virtual void StartPlay() {
        timer?.StartTime();
        FreezeFish(false);
    }

    protected virtual void InitializeSpawnPoints() {
        spawnPoints = new List<Transform>();
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("Spawn")) {
            spawnPoints.Add(spawnPoint.transform);
        }
    }

    protected virtual void EndLevel() {
        timer?.DisappearTimer();
        endingLevel = true;
        FreezeFish(true);
        foreach (var activePlayer in parameters.activePlayers) {
            activePlayer.OnDeath -= OnPlayerDeath;
            activePlayer.Gunfish.OnDeath -= OnPlayerDeath;
            activePlayer.Gunfish.PreDeath -= OnPlayerPreDeath;
        }
        StartCoroutine(CoEndLevel());
    }

    protected virtual IEnumerator CoEndLevel() {
        yield return new WaitForSeconds(endLevelDelay);
        ShowLevelStats();
        float minDelayBeforeContinuing = Mathf.Min(3f, maxNextLevelTimer - 1);
        yield return new WaitForSeconds(minDelayBeforeContinuing);
        PlayerManager.Instance.SetInputMode(PlayerManager.InputMode.EndLevel);
    }

    public void FreezeFish(bool freeze) {
        foreach (var player in parameters.activePlayers) {
            player.FreezeControls = freeze;
        }
    }

    public virtual void OnPlayerDeath(Player player) {
        // remove the fishy from the camera group
        // will this work? I don't know...
        GameCamera.Instance?.targetGroup.RemoveMember(null);
    }

    public virtual void OnPlayerPreDeath(Player player) {
        return;
    }

    public virtual void ShowLevelStats() { }

    public virtual void ShowEndGameStats() {

    }

    protected virtual (PlayerReference, string) Tiebreaker(List<PlayerReference> tiedPlayers) {
        return (null, "WHAT?");
    } 

    public virtual void NextLevel() {
        statsUI.CloseStats();
        waitingForNextLevel = false;
        if (nextLevelIndex < parameters.scenes.Count) {
            LevelManager.Instance.LoadLevel(parameters.scenes[nextLevelIndex], parameters.skyboxScene);
            nextLevelIndex++;
        } else if (done == true) {
            ENDITALL();
        } else {
            done = true;
            LevelManager.Instance.LoadStats(() => {
                ShowEndGameStats();
            });
        }
    }

    public void ENDITALL() {
        // NOTE destroy all players
        LevelManager.Instance.LoadMainMenu(() => {
            GameManager.Instance.ResetGame();
            MusicManager.Instance.PlayTrackSet(TrackSetLabel.Menu);
            MainMenu.Instance.Initialize();
        });
    }

    public virtual bool ResolveHit(Gun gun, GunfishSegment segment) {
        return playerReferences[gun.gunfish.player].team != playerReferences[segment.gunfish.player].team;
    }

    public virtual void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead) {
    }

    public virtual void OnTimerFinish() { }
}

public interface IMatchManager {
    public void Initialize(GameParameters parameters);
    public void TearDown();
    public void NextLevel();
    public bool ResolveHit(Gun gun, GunfishSegment segment);
    public void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead);

    public void ENDITALL();
}