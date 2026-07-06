using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class BassballTeamReference : ScoredTeamReference {
    public Goal goal;
    public int gamesWon = 0;

    public BassballTeamReference(int teamNumber, Color teamColor) : base(teamNumber, teamColor) { }
}

public class BassballMatchManager : MatchManager<PlayerReference, BassballTeamReference>
{
    // a ball spawns every X seconds
    Dictionary<Goal, BassballTeamReference> goalToTeam = new Dictionary<Goal, BassballTeamReference>();
    protected BassballTeamReference lastGameWinner;
    List<Goal> goals = new List<Goal>();

    public override void StartLevel() {
        base.StartLevel();
        ui.InitializeLevel(parameters.activePlayers, "X");
        ui.gameScore.gameObject.SetActive(true);
        ui.SetGameScore(0, 0);
        teams[0].score = 0;
        teams[1].score = 0;
    }

    protected override void EndLevel() {
        base.EndLevel();
        foreach (var goal in goals) {
            goal.OnGoal -= GOAL;
        }
        ui.gameScore.gameObject.SetActive(false);
    }

    protected override void InitializeSpawnPoints() {
        var sortedTeams = teams.OrderBy(x => x.teamNumber).ToList();
        var goals = FindObjectsByType<Goal>().OrderBy(x => x.transform.position.x).ToList();
        if (sortedTeams.Count() != goals.Count()) {
            Debug.Log($"Goals and Teams must be 1:1 but is {goals.Count()}:{sortedTeams.Count()}!");
        }
        for (int i = 0; i < sortedTeams.Count(); i++) {
            goalToTeam[goals[i]] = sortedTeams[i];
            sortedTeams[i].goal = goals[i];
            goals[i].OnGoal += GOAL;
        }
    }

    protected override void AddPlayerReference(Player player, TeamReference teamRef) {
        playerReferences[player] = new PlayerReference(player, (BassballTeamReference)teamRef);
    }

    protected override BassballTeamReference GenerateTeamRef(Player player) {
        return new BassballTeamReference(player.PlayerNumber, PlayerManager.Instance.playerColors[player.PlayerNumber]);
    }

    protected override IEnumerator CoSpawnPlayer(Player player) {
        yield return new WaitForSeconds(spawnDelay);

        // spawn a player at their respective spawn area
        player.SpawnGunfish(((BassballTeamReference)playerReferences[player].team).goal.GetNextSpawnPoint().position);
    }

    public override void OnPlayerDeath(Player player) {
        base.OnPlayerDeath(player);
        SpawnPlayer(player);
    }

    public void GOAL(Goal goal, BassballBall ball) {
        FindAnyObjectByType<CinemachineTargetGroup>().RemoveMember(ball.transform);
        foreach (var team in teams) {
            if (team.goal != goal) {
                MarqueeManager.Instance.PlayRandomQuip(QuipType.Goal);
                UpdateTeamScore(team, 1);
                return;
            }
        }
    }

    protected void UpdateTeamScore(BassballTeamReference team, int delta) {
        team.score += delta;
        ui.SetGameScore(teams[0].score, teams[1].score);
    }

    public override void OnTimerFinish() {
        base.OnTimerFinish();
        EndLevel();
    }

    public override void ShowLevelStats() {
        base.ShowLevelStats();
        // get first player who reached end to generate winner text
        // otherwise "No one reached the end..."

        string winnerText = "No one wins...";
        BassballTeamReference winner;
        Dictionary<PlayerReference, string> tiebreakerText = new Dictionary<PlayerReference, string>();
        if (teams[0].score == teams[1].score) {
            // get closer distance of ball
            float minDistance1 = float.MaxValue, minDistance2 = float.MaxValue;
            foreach (var ball in FindObjectsByType<BassballBall>()) {
                minDistance1 = Mathf.Min(minDistance1, Vector3.Distance(ball.transform.position, teams[1].goal.transform.position));
                minDistance2 = Mathf.Min(minDistance2, Vector3.Distance(ball.transform.position, teams[0].goal.transform.position));
            }
            winner = (minDistance1 < minDistance2) ? teams[0] : teams[1];
            winnerText = $"{winner.GetTitle()} wins by a tie!";
            tiebreakerText[winner.players.First()] = "*Closer to goal!";
        }
        else {
            winner = (teams[0].score > teams[1].score) ? teams[0] : teams[1];
            winnerText = $"{winner.GetTitle()} wins!";
        }
        MarqueeManager.Instance.PlayWinQuip(winner);
        winner.gamesWon += 1;

        List<PlayerReference> sortedList = playerReferences.Values.OrderByDescending(x => winner == x.team)
                              .ToList();
        statsUI.ShowStats(winnerText, sortedList, winner, tiebreakerText, scoreLambda:(x => ((BassballTeamReference)x.team).score.ToString()));
        lastGameWinner = winner;
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }

    public override void ShowEndGameStats() {
        base.ShowEndGameStats();

        // get team with most wins
        // tiebreaker is who won last game

        string winnerText = "No one wins...";
        BassballTeamReference winner;
        Dictionary<PlayerReference, string> tiebreakerText = new Dictionary<PlayerReference, string>();
        if (teams[0].score == teams[1].score) {
            // get closer distance of ball
            winner = lastGameWinner;
            winnerText = $"{winner.GetTitle()} wins by a tie!";
            tiebreakerText[winner.players.First()] = "*Last game winner!";
        }
        else {
            winner = (teams[0].gamesWon > teams[1].gamesWon) ? teams[0] : teams[1];
            winnerText = $"{winner.GetTitle()} wins!";
        }
        MarqueeManager.Instance.PlayWinQuip(winner);
        List<PlayerReference> sortedList = playerReferences.Values.OrderByDescending(x => winner == x.team)
                              .ToList();
        statsUI.ShowStats(winnerText, sortedList, winner, tiebreakerText, "Games", scoreLambda:(x => ((BassballTeamReference)x.team).gamesWon.ToString()));
        nextLevelTimer = maxNextLevelTimer;
        waitingForNextLevel = true;
    }
}
