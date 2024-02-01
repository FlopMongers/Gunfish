using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RacePlayerReference : PlayerReference {
    public Checkpoint lastCheckpoint;
    public float lastCheckpointTimestamp;
    public bool finished;

    public float firstCheckpointDistance;

    public bool IsAlive { get { return player.Gunfish.RootSegment != null; } }

    public RacePlayerReference(Player player, TeamReference team) : base(player, team) { }

    public override string GetStatsText() {
        TimeSpan timeSpan = TimeSpan.FromSeconds(lastCheckpointTimestamp);
        string formattedTime = $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        return (finished == true) ? $"{formattedTime}" : $"DNF (({lastCheckpoint.checkpointOrder})), {formattedTime}";
    }
}

public class RaceMatchManager : MatchManager<RacePlayerReference, TeamReference> {

    List<Checkpoint> checkpoints = new List<Checkpoint>();
    Checkpoint lastCheckpoint;

    HashSet<RacePlayerReference> finishedPlayers = new HashSet<RacePlayerReference>();

    protected override void AddPlayerReference(Player player, TeamReference teamRef) {
        playerReferences[player] = new RacePlayerReference(player, (ScoredTeamReference)teamRef);
    }

    public override void StartLevel() {
        base.StartLevel();
        endingLevel = false;
        // TODO: set up UI
        ui.InitializeLevel(parameters.activePlayers, "X");
        // iterate players and set up stocks
        foreach (var player in parameters.activePlayers) {
            RacePlayerReference playerRef = playerReferences[player];
            playerRef.lastCheckpoint = checkpoints[0];
            playerRef.lastCheckpointTimestamp = 0;
            playerRef.finished = false;
            player.OnDeath += OnPlayerDeath;
            player.Gunfish.OnDeath += OnPlayerDeath;
            player.Gunfish.PreDeath += OnPlayerPreDeath;
            SpawnPlayer(player);
        }
    }

    protected override void InitializeSpawnPoints() {
        checkpoints = FindObjectsOfType<Checkpoint>().OrderBy(x => x.checkpointOrder).ToList();
        // hook into the checkpoint events
        foreach (var checkpoint in checkpoints) {
            checkpoint.fishEnterEvent += OnCheckpointEnter;
        }
        lastCheckpoint = checkpoints[checkpoints.Count - 1];
    }

    void OnCheckpointEnter(Gunfish gunfish, Checkpoint checkpoint) {
        // get player reference
        RacePlayerReference playerRef = playerReferences[gunfish.player];
        // if checkpoint order greater than player last checkpoint
        if (checkpoint.checkpointOrder <= playerRef.lastCheckpoint.checkpointOrder)
            return;

        gunfish.AddEffect(new Invincibility_Effect(gunfish, 2f));

        // update player's shit
        playerRef.lastCheckpoint = checkpoint;
        playerRef.lastCheckpointTimestamp = timer.levelDuration - timer.timer; //Time.time;
        // if last checkpoint put the player in finished players (and freeze that player)
        if (checkpoint == lastCheckpoint) {
            playerRef.player.FreezeControls = true;
            playerRef.finished = true;
            finishedPlayers.Add(playerRef);
            // if no players left, end level
            if (finishedPlayers.Count == parameters.activePlayers.Count) {
                EndLevel();
            }
        }
    }

    protected override IEnumerator CoSpawnPlayer(Player player) {
        yield return new WaitForSeconds(0.5f);

        // spawn a player at their last checkpoint
        player.SpawnGunfish(playerReferences[player].lastCheckpoint.GetNextSpawnPoint().position);
        FinishSpawningPlayer(player);
    }

    public override void OnPlayerPreDeath(Player player) {
        // get and save distance to next checkpoint if applicable
        RacePlayerReference playerRef = playerReferences[player];
        if (playerRef.lastCheckpoint.checkpointOrder != 0)
            return;

        playerRef.firstCheckpointDistance = Vector3.Distance(player.Gunfish.RootSegment.transform.position, checkpoints[1].transform.position);
    }

    public override void OnPlayerDeath(Player player) {
        base.OnPlayerDeath(player);
        SpawnPlayer(player);
    }

    public override void OnTimerFinish() {
        base.OnTimerFinish();

        // for any applicable players, calculate firstCheckpointDistance
        foreach ((Player player, RacePlayerReference playerRef) in playerReferences) {
            if (player.Gunfish.RootSegment != null) {
                playerRef.firstCheckpointDistance = Vector3.Distance(player.Gunfish.RootSegment.transform.position, checkpoints[1].transform.position);
            }
        }
    }

    public override void ShowLevelStats() {
        base.ShowLevelStats();
        // get first player who reached end to generate winner text
        // otherwise "No one reached the end..."

        string winnerText = "No one reached the end...";
        var winnerList = playerReferences.Values.Where(x => x.lastCheckpoint == lastCheckpoint).OrderBy(x => x.lastCheckpointTimestamp);
        RacePlayerReference winner = null;
        if (winnerList.Count() > 0) {
            winner = winnerList.First();
            winnerText = $"{winner.team.GetTitle()} wins the level!";
        }

        //  1) who got the farthest checkpoint
        //  2) by earliest non-negative timestamp
        //  3) being alive vs dead
        //  4) by being closest to first checkpoint
        List<RacePlayerReference> sortedList = playerReferences.Values.OrderByDescending(x => x.lastCheckpoint.checkpointOrder)
                              .ThenBy(x => x.lastCheckpointTimestamp)
                              .ThenByDescending(x => x.finished)
                              .ThenByDescending(x => x.firstCheckpointDistance)
                              .ToList();
        statsUI.ShowStats(winnerText, sortedList, winner?.team, new Dictionary<RacePlayerReference, string>());
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }

    public override void ShowEndGameStats() {
        base.ShowEndGameStats();
        string winnerText = "No one wins?";

        // resolve ties
        List<RacePlayerReference> preSortedTeams = playerReferences.Values.OrderByDescending(x => x.lastCheckpoint.checkpointOrder).ToList();
        List<RacePlayerReference> players = new List<RacePlayerReference>();
        RacePlayerReference winningTeam = null;
        RacePlayerReference currentTeam = preSortedTeams.Pop();
        Dictionary<RacePlayerReference, string> tiebreakerTextMap = new Dictionary<RacePlayerReference, string>();
        while (preSortedTeams.Count > 0) {
            RacePlayerReference nextTeam = preSortedTeams[0];
            RacePlayerReference displayTeam = currentTeam;
            (PlayerReference tiebreakPlayer, string tiebreakText) = Tiebreaker(currentTeam, nextTeam);
            if ((nextTeam.lastCheckpoint.checkpointOrder == currentTeam.lastCheckpoint.checkpointOrder && tiebreakPlayer == nextTeam)) {
                displayTeam = nextTeam;
                preSortedTeams.Pop();
            }
            else {
                tiebreakText = "";
                displayTeam = currentTeam;
                currentTeam = preSortedTeams.Pop();
            }

            if (winningTeam == null) {
                winningTeam = displayTeam;
                if (tiebreakText != "") {
                    winnerText = $"{displayTeam.team.GetTitle()} wins... by a tiebreak!";
                }
                else {
                    winnerText = $"{displayTeam.team.GetTitle()} wins!";
                }
            }
            tiebreakerTextMap[displayTeam] = tiebreakText;
        }
        players.Add(currentTeam);

        // no winner
        // X wins
        // X wins... by a tiebreak!
        statsUI.ShowStats(winnerText, players, winningTeam.team, tiebreakerTextMap);
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }

    // NOTE: might not need this

    protected (RacePlayerReference, string) Tiebreaker(RacePlayerReference player1, RacePlayerReference player2) {

        // these players have the same finish

        //  2) by earliest non-negative timestamp
        if (player1.lastCheckpointTimestamp != 0 ^ player2.lastCheckpointTimestamp != 0) {
            return (player1.lastCheckpointTimestamp != 0) ? (player1, "*Faster checkpoint!") : (player2, "*Faster checkpoint!");
        }

        //  3) being alive vs dead
        if (player1.IsAlive ^ player2.IsAlive) {
            return (player1.IsAlive) ? (player1, "*Not dead!") : (player2, "*Not dead!");
        }

        //  4) by being closest to first checkpoint
        return (player1.firstCheckpointDistance > player2.firstCheckpointDistance) ? (player1, "*Closer to checkpoint!"): (player2, "*Closer to checkpoint!");
    }
}
