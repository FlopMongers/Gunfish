using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DeathMatchPlayerReference : PlayerReference {
    public float lastHitTimestamp = -1;
    public Player lastHitter;

    // reset each level
    public float firstKill;
    public float lastDeath;

    public int score;
    public int stocks;

    public DeathMatchPlayerReference(Player player, ScoredTeamReference team, int stocks) : base(player, team) {
        this.stocks = stocks;
    }

    public override string GetStatsText() {
        return score.ToString();
    }
}

public class DeathMatchManager : MatchManager<DeathMatchPlayerReference, ScoredTeamReference> {
    private const int defaultStocks = 3;
    protected HashSet<TeamReference> eliminatedTeams = new HashSet<TeamReference>();

    [HideInInspector]

    public PelicanSpawner pelicanSpawner;

    static float lastHitThreshold = 4f;


    public override void Initialize(GameParameters parameters) {
        pelicanSpawner = GetComponentInChildren<PelicanSpawner>();
        base.Initialize(parameters);
    }

    protected override void AddPlayerReference(Player player, TeamReference teamRef) {
        playerReferences[player] = new DeathMatchPlayerReference(player, (ScoredTeamReference)teamRef, defaultStocks);
    }

    protected override ScoredTeamReference GenerateTeamRef(Player player) {
        return new ScoredTeamReference(player.PlayerNumber, PlayerManager.Instance.playerColors[player.PlayerNumber]);
    }

    public override void StartLevel() {
        base.StartLevel();
        eliminatedTeams = new HashSet<TeamReference>();
        ui.InitializeLevel(parameters.activePlayers, defaultStocks.ToString());
        pelicanSpawner.FetchSpawnZones();
        pelicanSpawner.active = false;
    }

    public override void SetUpPlayer(Player player) {
        base.SetUpPlayer(player);
        playerReferences[player].stocks = defaultStocks;
        playerReferences[player].firstKill = -1;
        playerReferences[player].lastDeath = -1;
    }


    protected override IEnumerator CoSpawnPlayer(Player player) {
        yield return new WaitForSeconds(spawnDelay);
        Transform currentSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        float maxDistance = float.MinValue;
        float distance;
        foreach (var spawnPoint in spawnPoints) {
            distance = float.MaxValue;
            bool skip = true;
            foreach (var activePlayer in parameters.activePlayers) {
                if (activePlayer.Gunfish.RootSegment == null) {
                    continue;
                } else {
                    skip = false;
                }
                var playerDist = activePlayer.Gunfish.GetPosition();
                if (playerDist.HasValue)
                    distance = Mathf.Min(distance, Vector2.Distance(spawnPoint.position, playerDist.Value));
            }
            if (skip == false && distance > maxDistance) {
                maxDistance = distance;
                currentSpawnPoint = spawnPoint;
            }
        }
        player.SpawnGunfish(currentSpawnPoint.position);
        //FinishSpawningPlayer(player);
    }

    public override void OnPlayerDeath(Player player) {
        DeathMatchPlayerReference playerRef = playerReferences[player];
        base.OnPlayerDeath(player);
        UpdateStock(player, -1);
        if (playerRef.stocks > 0) {
            SpawnPlayer(player);
        } else if (!playerRef.team.players.Any(x => ((DeathMatchPlayerReference)x).stocks > 0)) {
            eliminatedTeams.Add(playerRef.team);
            if ((teams.Count - eliminatedTeams.Count) <= 1 && !endingLevel) {
                EndLevel();
            }
        }
    }

    private PlayerReference GetLastPlayerStanding() {
        foreach ((Player _, DeathMatchPlayerReference playerRef) in playerReferences) {
            if (playerRef.stocks > 0)
                return playerRef;
        }
        return null;
    }

    protected override void EndLevel() {
        SharkmodeManager.Instance.StopMusic();
        pelicanSpawner.active = false;
        base.EndLevel();
    }

    public override void ShowLevelStats() {
        // winner text ("(Team/Player X) wins the level!")
        // if no winner, No level winner!
        base.ShowLevelStats();
        DeathMatchPlayerReference winningPlayer = (DeathMatchPlayerReference)GetLastPlayerStanding();
        string winnerText = "No level winner...";
        if (winningPlayer != null) {
            winnerText = $"{winningPlayer.team.GetTitle()} wins the level!";
            UpdateScore(winningPlayer.player, 1);
        }
        // sort player references by scores
        List<DeathMatchPlayerReference> players = playerReferences.Values.OrderByDescending(x => x.score).ToList();
        statsUI.ShowStats(winnerText, players, winningPlayer?.team, new Dictionary<DeathMatchPlayerReference, string>());
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }

    public override void ShowEndGameStats() {
        base.ShowEndGameStats();

        string winnerText = "No one wins?";

        // resolve ties
        List<ScoredTeamReference> preSortedTeams = teams.OrderByDescending(x => x.score).ToList();
        List<DeathMatchPlayerReference> players = new List<DeathMatchPlayerReference>();
        ScoredTeamReference winningTeam = null;
        ScoredTeamReference currentTeam = preSortedTeams.Pop();
        Dictionary<DeathMatchPlayerReference, string> tiebreakerTextMap = new Dictionary<DeathMatchPlayerReference, string>();
        while (preSortedTeams.Count > 0) {
            ScoredTeamReference nextTeam = preSortedTeams[0];
            ScoredTeamReference displayTeam = currentTeam;
            (PlayerReference tiebreakPlayer, string tiebreakText) = Tiebreaker(currentTeam, nextTeam);
            if (nextTeam.score != currentTeam.score) {
                tiebreakText = "";
            }
            if ((nextTeam.score == currentTeam.score && tiebreakPlayer.team == nextTeam)) {
                displayTeam = nextTeam;
                preSortedTeams.Pop();
            }
            else {
                displayTeam = currentTeam;
                currentTeam = preSortedTeams.Pop();
            }

            if (winningTeam == null) {
                winningTeam = displayTeam;
                if (tiebreakText != "") {
                    winnerText = $"{displayTeam.GetTitle()} wins... by a tiebreak!";
                }
                else {
                    winnerText = $"{displayTeam.GetTitle()} wins!";
                }
            }
            foreach (var player in displayTeam.players.OrderByDescending(x => ((DeathMatchPlayerReference)x).score)) {
                players.Add((DeathMatchPlayerReference)player);
            }
            tiebreakerTextMap[(DeathMatchPlayerReference)displayTeam.players[0]] = tiebreakText;
        }
        foreach (var player in currentTeam.players.OrderByDescending(x => ((DeathMatchPlayerReference)x).score)) {
            players.Add((DeathMatchPlayerReference)player);
        }

        // no winner
        // X wins
        // X wins... by a tiebreak!
        statsUI.ShowStats(winnerText, players, winningTeam, tiebreakerTextMap);
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }

    protected (DeathMatchPlayerReference, string) Tiebreaker(TeamReference team1, TeamReference team2) {
        // get player with earliest non-negative kill
        List<DeathMatchPlayerReference> tiedPlayers = new List<DeathMatchPlayerReference>();
        foreach (var playerRef in team1.players.Union(team2.players)) {
            tiedPlayers.Add((DeathMatchPlayerReference)playerRef);
        }
        List<DeathMatchPlayerReference> tiebreaker = tiedPlayers.Where(x => x.firstKill > 0).OrderBy(x => x.firstKill).ToList();
        DeathMatchPlayerReference tiebreakPlayer;
        if (tiebreaker.Count != 0) {
            tiebreakPlayer = tiebreaker.First();
            return (tiebreakPlayer, "*Faster kill!");
        }
        // if null, get player with last death
        tiebreakPlayer = tiedPlayers.OrderByDescending(x => x.lastDeath).FirstOrDefault();
        return (tiebreakPlayer, "*Later death!");
    }

    public void UpdateScore(Player player, int scoreDelta) {
        DeathMatchPlayerReference playerRef = playerReferences[player];
        playerRef.score += scoreDelta;
        ((ScoredTeamReference)playerRef.team).score += scoreDelta;
        ui.OnScoreChange(player, playerRef.score);
    }
    public void UpdateStock(Player player, int stockDelta) {
        DeathMatchPlayerReference playerRef = playerReferences[player];
        playerRef.stocks += stockDelta;
        ui.OnStockChange(player, playerRef.stocks);
    }

    public override void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead) {
        base.HandleFishDamage(fishHit, gunfish, alreadyDead);
        if (alreadyDead == true)
            return;

        DeathMatchPlayerReference playerRef = playerReferences[gunfish.player];

        // if src fish, then award points, otherwise detract points
        Gunfish sourceGunfish = fishHit.source.GetComponent<Gunfish>();
        sourceGunfish = sourceGunfish ?? fishHit.source.GetComponent<Gun>()?.gunfish;
        if (sourceGunfish == gunfish) {
            sourceGunfish = null;
        }
        if (sourceGunfish != null) {
            playerRef.lastHitTimestamp = Time.time;
            playerRef.lastHitter = sourceGunfish.player;
        }

        if (gunfish.statusData.health > 0) {
            return;
        }

        if ((Time.time - playerRef.lastHitTimestamp) <= lastHitThreshold && playerRef.lastHitter != null) {
            sourceGunfish = playerRef.lastHitter.Gunfish;
        }
        playerRef.lastDeath = Time.time;

        if (sourceGunfish != null) {
            MarqueeManager.Instance.PlayRandomQuip(QuipType.PlayerDeath);
            // update first kill
            if (playerReferences[sourceGunfish.player].firstKill < 0)
                playerReferences[sourceGunfish.player].firstKill = Time.time;
            UpdateScore(sourceGunfish.player, 1);
        } else if (!endingLevel) {
            // todo: this should play a special suicide quip (Selfish Destruction!)
            MarqueeManager.Instance.PlayRandomQuip(QuipType.PlayerDeath);
        }
    }

    public override void OnTimerFinish() {
        base.OnTimerFinish();
        // todo: SUMMON THE FUCKING PELICANS
        MarqueeManager.Instance.PlayRandomQuip(QuipType.Pelicans);
        //MarqueeManager.Instance.PlayTitle("PELICAN TIME!!!");
        pelicanSpawner.active = true;
        foreach ((Player player, DeathMatchPlayerReference playerRef) in playerReferences) {
            if (playerRef.stocks > 1) {
                UpdateStock(player, -(playerRef.stocks - 1));
            }
        }
        // maybe play a quip? (SUDDEN DEATH!)
    }
}