using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

public class TeamDeathMatchManager : DeathMatchManager
{
    // associate players with teams
    // todo: replace with sub-class of PlayerReference
    public Dictionary<Player, int> playerTeamMap = new Dictionary<Player, int>();

    public override void Initialize(GameParameters parameters) {
        base.Initialize(parameters);
        // player 1 and player 3 are on same team and players 2 and 4 are on same team :P
        foreach (var player in parameters.activePlayers) {
            playerTeamMap[player] = player.PlayerNumber % 2;
        }
    }

    public override bool ResolveHit(Gun gun, GunfishSegment segment) {
        if (!base.ResolveHit(gun, segment))
            return false;

        // return if the player firing the gun belongs to the same team as the target
        return playerTeamMap[gun.gunfish.player] != playerTeamMap[segment.gunfish.player];
    }

    protected override void ShowLevelWinner(Player player) {
        ui.ShowLevelStats((player == null) ? "No one wins!" : $"Team {playerTeamMap[player]} wins!", playerReferences);
    }

    public override void ShowEndGameStats() {
        //base.ShowEndGameStats();
        Dictionary<int, int> teamScores = new Dictionary<int, int>() { { 0,0}, { 1,0} };
        List<Player> winners = new List<Player>();
        foreach ((Player player, PlayerReference playerRef) in playerReferences) {
            teamScores[playerTeamMap[player]] += playerRef.score;
        }
        int winningTeam = -1;
        if (teamScores[0] != teamScores[1])
            winningTeam = (teamScores[0] > teamScores[1]) ? 0: 1;
        foreach ((Player player, PlayerReference playerRef) in playerReferences) {
            if (winningTeam == -1 || playerTeamMap[player] == winningTeam) {
                winners.Add(player);
            }
        }

        string text = "It's a tie!";
        if (winners.Count == 0) {
            text = "No team wins?";
        }
        else if (winners.Count == 2) {
            text = $"Players {winners[0].PlayerNumber} and {winners[1].PlayerNumber} win!!!";
        }
        ui.ShowFinalScores(text, playerReferences, winners);
    }
}
