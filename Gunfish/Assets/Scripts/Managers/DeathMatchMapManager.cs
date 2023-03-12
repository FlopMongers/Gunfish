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
        Transform currentSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        float maxDistance = float.MinValue;
        float distance;
        foreach (var spawnPoint in spawnPoints)
        {
            distance = float.MaxValue;
            foreach (var activePlayer in parameters.activePlayers)
            {
                distance = Mathf.Min(distance, Vector2.Distance(spawnPoint.position, activePlayer.Gunfish.transform.position));
            }
            if (distance > maxDistance)
            {
                maxDistance = distance;
                currentSpawnPoint = spawnPoint;
            }
        }
        player.SpawnGunfish(currentSpawnPoint.position);
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
        // TODO update stock ui
        if (playerStocks[player] <= 0)
        {
            // TODO flashy ui thingy when player is eliminated
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
