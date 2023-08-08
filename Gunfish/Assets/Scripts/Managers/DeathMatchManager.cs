using System.Collections.Generic;
using UnityEngine;

public class DeathMatchManager : MatchManager {
    private const int defaultStocks = 3;
    private Dictionary<Player, int> playerScores = new Dictionary<Player, int>();
    private Dictionary<Player, int> playerStocks = new Dictionary<Player, int>();
    private int remainingPlayers;
    private DeathMatchUI ui;

    public override void Initialize(GameParameters parameters) {
        foreach (var player in parameters.activePlayers) {
            playerScores[player] = 0;
        }
        ui = gameObject.GetComponentInChildren<DeathMatchUI>();
        ui?.InitializeMatch(parameters.activePlayers);
        base.Initialize(parameters);
    }

    public override void StartLevel() {
        base.StartLevel();
        ui?.InitializeLevel(parameters.activePlayers, defaultStocks);
        remainingPlayers = parameters.activePlayers.Count;
        // iterate players and set up stocks
        foreach (var player in parameters.activePlayers) {
            playerStocks[player] = defaultStocks;
            player.OnDeath += OnPlayerDeath;
            SpawnPlayer(player);
            player.Gunfish.OnDeath += OnPlayerDeath;
        }
    }

    public override void SpawnPlayer(Player player) {
        Transform currentSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        float maxDistance = float.MinValue;
        float distance;
        foreach (var spawnPoint in spawnPoints) {
            distance = float.MaxValue;
            foreach (var activePlayer in parameters.activePlayers) {
                distance = Mathf.Min(distance, Vector2.Distance(spawnPoint.position, activePlayer.Gunfish.transform.position));
            }
            if (distance > maxDistance) {
                maxDistance = distance;
                currentSpawnPoint = spawnPoint;
            }
        }
        player.SpawnGunfish(currentSpawnPoint.position);
    }

    public override void OnPlayerDeath(Player player) {
        playerStocks[player]--;
        ui?.OnStockChange(player, playerStocks[player]);
        if (playerStocks[player] > 0) {
            SpawnPlayer(player);
        }
        else if (remainingPlayers <= 1) {
            // TODO increment score and trigger ui score and winner display, delay level loading

            NextLevel();
            //NextLevel_Event?.Invoke(remainingPlayers);
        }
    }
}
