using System.Collections;
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
        ui.OnLoadingStart();
        ui.InitializeMatch(parameters.activePlayers);
        base.Initialize(parameters);
    }

    public override void StartLevel() {
        base.StartLevel();
        ui.InitializeLevel(parameters.activePlayers, defaultStocks);
        ui.OnLoadingEnd();
        remainingPlayers = parameters.activePlayers.Count;
        // iterate players and set up stocks
        foreach (var player in parameters.activePlayers) {
            playerStocks[player] = defaultStocks;
            player.OnDeath += OnPlayerDeath;
            SpawnPlayer(player);
            player.Gunfish.OnDeath += OnPlayerDeath;
        }
    }

    public override void StartPlay() {
        StartCoroutine(StartPlayCoroutine());
    }

    private IEnumerator StartPlayCoroutine() {
        ui.StartMatchCountdown(3f);
        yield return new WaitForSeconds(3f);
        base.StartPlay();
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
        ui.OnStockChange(player, playerStocks[player]);
        if (playerStocks[player] > 0) {
            SpawnPlayer(player);
        }
        else if (remainingPlayers <= 1) {
            OnPlayerWin(GetLastPlayerStanding());
        }
    }

    private void OnPlayerWin(Player player) {
        playerScores[player] += 1;
        ui.OnScoreChange(player, playerScores[player]);
        // TODO show victory animations and delay level loading
        ui.OnLoadingStart();
        NextLevel();
    }

    private Player GetLastPlayerStanding() {
        foreach(var kvp in playerStocks) {
            if (kvp.Value > 0) return kvp.Key;
        }
        return null;
    }
}
