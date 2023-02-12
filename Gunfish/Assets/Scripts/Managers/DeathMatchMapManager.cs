using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMatchLevelManager : MapManager
{
    Dictionary<Player, int> playerStocks = new Dictionary<Player, int>();
    int remainingPlayers;

    public int DefaultStocks = 3;

    public override void SpawnPlayer(Player player)
    {
        // get spawn point which is farthest from any player
        // instantiate fish
    }

    public override void Initialize(GameParameters parameters)
    {
        remainingPlayers = 0;
        // iterate players and set up stocks
        foreach (var player in parameters.activePlayers)
        {
            remainingPlayers++;
            playerStocks[player] = DefaultStocks;
            player.OnDeath += OnPlayerDeath;
            SpawnPlayer(player);
        }
        base.Initialize(parameters);
    }

    public override void OnPlayerDeath(Player player)
    {
        playerStocks[player]--;
            // update stock ui
        if (playerStocks[player] <= 0)
        {
            // flashy ui thingy
            if (remainingPlayers <= 1)
            {
                NextLevel_Event?.Invoke(remainingPlayers);
            }
        }
        else
        {
            SpawnPlayer(player);
        }
    }

}
