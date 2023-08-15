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
                var playerDist = activePlayer.Gunfish.GetPosition();
                if (playerDist.HasValue)
                    distance = Mathf.Min(distance, Vector2.Distance(spawnPoint.position, playerDist.Value));
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
        else {
            remainingPlayers--;
            if (remainingPlayers <= 1) {
                EndLevel();
            }
        }
    }

    private Player GetLastPlayerStanding() {
        foreach(var kvp in playerStocks) {
            if (kvp.Value > 0) return kvp.Key;
        }
        return null;
    }

    public override void NextLevel()
    {
        ui.CloseLevelStats();
        ui.OnLoadingStart();
        base.NextLevel();
    }

    private void EndLevel()
    {
        FreezeFish(true);

        var player = GetLastPlayerStanding();
        if (player != null)
        {
            playerScores[player] += 1;
            ui.OnScoreChange(player, playerScores[player]);
        }

        ui.ShowLevelStats(player, playerScores); // if player is null, no one wins
        PlayerManager.instance.SetInputMode(PlayerManager.InputMode.EndLevel);
    }

    public override void ShowStats()
    {
        base.ShowStats();
        ui.ShowFinalScores(playerScores);
    }
}
