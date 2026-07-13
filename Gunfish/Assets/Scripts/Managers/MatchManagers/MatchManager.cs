using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerReference {
    public Player player;

    // team number associated with each player
    public TeamReference team;
    public int rating;

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
    [System.NonSerialized]
    public GameParameters parameters;
    protected int currentLevel;

    protected bool skipLastStats = false;

    protected List<Transform> spawnPoints;

    protected int nextLevelIndex;
    protected bool done;

    public LevelTimer timer;

    public float spawnDelay = 0.5f;

    protected float maxNextLevelTimer = 8f;
    protected float nextLevelTimer;
    protected bool waitingForNextLevel = false;

    protected bool endingLevel;
    protected static float endLevelDelay = 0.5f;

    public bool teamMode = false;

    protected List<TeamReferenceType> teams = new List<TeamReferenceType>();
    protected Dictionary<Player, PlayerReferenceType> playerReferences = new Dictionary<Player, PlayerReferenceType>();

    public MatchUI ui;

    public MatchUI GetUI() {
        return ui;
    }

    public StatsUI statsUI;

    private MatchResult matchResult;

    public virtual void Initialize(GameParameters parameters) {
        this.parameters = parameters;
        ui = ui ?? gameObject.GetComponentInChildren<MatchUI>();
        ui.InitializeMatch(parameters.activePlayers);
        Dictionary<int, TeamReferenceType> teamNumbers = new Dictionary<int, TeamReferenceType>();
        int playerTeamNumber = 0;

        foreach (var player in parameters.activePlayers.OrderBy(activePlayer => activePlayer.PlayerNumber)) {
            if (teamMode == false) {
                playerTeamNumber = player.TeamNumber;
            }
            if (teamNumbers.ContainsKey(player.PlayerNumber) == false) {
                TeamReferenceType TeamRef = GenerateTeamRef(player);
                teamNumbers[playerTeamNumber] = TeamRef;
                teams.Add(TeamRef);
            }
            AddPlayerReference(player, teamNumbers[playerTeamNumber]);
            playerTeamNumber = (playerTeamNumber + 1) % 2;
        }
        spawnPoints = new List<Transform>();
        LevelManager.Instance.OnFinishLoadLevel += StartLevel;
        LevelManager.Instance.OnStartPlay += StartPlay;
        timer = timer ?? GetComponentInChildren<LevelTimer>();
        if (timer != null) {
            timer.levelDuration = this.parameters.secondsPerRound;
            timer.OnTimerFinish += OnTimerFinish;
        }
        matchResult = new MatchResult
        {
            GameMode = this.GetType().ToString().Replace("Manager", ""),
            StartTime = DateTime.Now,
            LevelCount = parameters.scenes.Count,
            PlayerCount = parameters.activePlayers.Count,
            PlayerMatchResults = new Dictionary<Player, PlayerMatchResult>(),
            LevelResults = new List<LevelResult>()
        };
        foreach (var player in parameters.activePlayers) {
            matchResult.PlayerMatchResults[player] = new PlayerMatchResult
            {
                PlayerId = player.PlayerNumber,
                PlayerTeam = player.TeamNumber,
                PlayerFish = player.gunfishData.name,
            };
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
        foreach (var player in parameters.activePlayers) {
            // StartLevel() subscribes these; EndLevel() unsubscribes them on a natural round end,
            // but TearDown() is also reachable mid-round (e.g. an aborted match), where EndLevel()
            // never runs. Without this, a later death on the same (persistent) Player still invokes
            // OnPlayerDeath on this now-destroyed match manager. Safe to unsubscribe twice.
            player.OnDeath -= OnPlayerDeath;
            player.Gunfish.OnDeath -= OnPlayerDeath;
            player.Gunfish.PreDeath -= OnPlayerPreDeath;
            matchResult.PlayerMatchResults[player].Score = GetPlayerScore(player);
            matchResult.PlayerMatchResults[player].Rating = GetPlayerRating(player);
        }
        StatsManager.LogMatchResults(matchResult);
    }

    public virtual void SpawnPlayer(Player player) {
        StartCoroutine(CoSpawnPlayer(player));
    }

    protected virtual IEnumerator CoSpawnPlayer(Player player) {
        StatsManager.LogStat(new PlayerSpawn
        {
            PlayerId = player.PlayerNumber,
            SpawnTime = DateTime.Now,
            X = player.Gunfish.MiddleSegment.transform.position.x,
            Y = player.Gunfish.MiddleSegment.transform.position.y
        });
        yield return null;
    }

    public virtual void StartLevel() {
        InitializeSpawnPoints();
        FreezeFish(true);
        endingLevel = false;
        // iterate players and set up stocks
        foreach (var player in parameters.activePlayers) {
            SetUpPlayer(player);
            player.OnDeath += OnPlayerDeath;
            player.Gunfish.OnDeath += OnPlayerDeath;
            player.Gunfish.PreDeath += OnPlayerPreDeath;
            SpawnPlayer(player);
        }

        matchResult.LevelResults.Add(new LevelResult
        {
            StartTime = DateTime.Now,
            LevelName = GetCurrentLevelName()
        });

        ui.InitializeLevel(parameters.activePlayers, parameters.stocksPerRound);
    }

    public virtual void SetUpPlayer(Player player) { }


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

        matchResult.LevelResults[^1].EndTime = DateTime.Now;

        if (skipLastStats && nextLevelIndex >= parameters.scenes.Count) {
            PlayerManager.Instance.SetInputMode(PlayerManager.InputMode.EndLevel);
            StartCoroutine(CoEndLastLevel());
        }
        else {
            StartCoroutine(CoEndLevel());
        }

    }

    protected virtual IEnumerator CoEndLastLevel() {
        yield return new WaitForSeconds(2f);
        EndLastLevel();
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
            EndLastLevel();
        }
    }

    public virtual void EndLastLevel() {
        done = true;
        LevelManager.Instance.LoadStats(() => {
            ShowEndGameStats();
        });
        matchResult.EndTime = DateTime.Now;
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

    public virtual void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead)
    {
        // If the fish is already dead, don't track a death
        if (alreadyDead)
            return;
        // If the hit didn't do damage, don't track a hit
        if (fishHit.damage <= 0)
            return;

        string sourceType = fishHit.source.name;
        int sourceId = fishHit.source.GetEntityId().GetHashCode();
        bool isFatal = gunfish.statusData.health <= 0;
        bool IsSelfInflicted = false;

        // See whether source is actually a player
        Gunfish sourceGunfish = fishHit.source.GetComponent<Gunfish>();
        sourceGunfish = sourceGunfish ?? fishHit.source.GetComponent<Gun>()?.gunfish;
        if (sourceGunfish == gunfish) {
            IsSelfInflicted = true;
        }

        // FIXME: Try to use the DeathMatchManager's last-hitter identification somehow
        Player sourcePlayer = fishHit.source.GetComponent<Player>();
        if (sourcePlayer != null)
        {
            Debug.Log("Player object was source of FishHit!");
            sourceType = "Player";
            sourceId = sourcePlayer.PlayerNumber;
            IsSelfInflicted = sourcePlayer == gunfish.player;
        }
        else if (sourcePlayer == null && sourceGunfish != null)
        {
            sourcePlayer = sourceGunfish?.player;
            sourceType = "Player";
            sourceId = sourcePlayer.PlayerNumber;
        }

        StatsManager.LogStat(new PlayerDamage
        {
            PlayerId = gunfish.player.PlayerNumber,
            DamageTime = DateTime.Now,
            SourceType = sourceType.Replace("(Clone)", ""),
            SourceId = sourceId,
            Amount = fishHit.damage,
            IsFatal = isFatal,
            IsSelfInflicted = IsSelfInflicted,
            X = gunfish.MiddleSegment.transform.position.x,
            Y = gunfish.MiddleSegment.transform.position.y
        });
    }

    public virtual void OnTimerFinish() { }

    public virtual int GetPlayerScore(Player player) { return 0; }

    public virtual string GetCurrentLevelName() {
        return Path.GetFileNameWithoutExtension(parameters.scenes[currentLevel]);
    }

    public virtual void SetPlayerRating(Player player, int rating) {}

    public virtual int GetPlayerRating(Player player) { return 0; }
}

public interface IMatchManager {
    public void Initialize(GameParameters parameters);
    public void TearDown();
    public void NextLevel();
    public bool ResolveHit(Gun gun, GunfishSegment segment);
    public void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead);
    public void ENDITALL();
    public MatchUI GetUI();
    public int GetPlayerScore(Player player);
    public string GetCurrentLevelName();
    public void SetPlayerRating(Player player, int rating);
    public int GetPlayerRating(Player player);
}