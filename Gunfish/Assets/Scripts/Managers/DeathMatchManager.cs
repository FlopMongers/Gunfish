using FunkyCode.LightingSettings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeathMatchManager : MatchManager {
    private const int defaultStocks = 3;
    protected Dictionary<Player, int> playerScores = new Dictionary<Player, int>();
    private Dictionary<Player, int> playerStocks = new Dictionary<Player, int>();
    private int remainingPlayers;
    private List<Player> eliminatedPlayers = new List<Player>();

    protected DeathMatchUI ui;

    public PelicanSpawner pelicanSpawner;

    bool endingLevel;
    static float endLevelDelay = 0.5f;


    public override void Initialize(GameParameters parameters) {
        foreach (var player in parameters.activePlayers) {
            playerScores[player] = 0;
            Debug.Log("Player Score: " + playerScores[player]);
        }
        ui = gameObject.GetComponentInChildren<DeathMatchUI>();
        // ui.OnLoadingStart();
        ui.InitializeMatch(parameters.activePlayers);
        base.Initialize(parameters);
        pelicanSpawner = GetComponentInChildren<PelicanSpawner>();
    }

    public override void StartLevel() {
        base.StartLevel();
        endingLevel = false;
        eliminatedPlayers = new List<Player>();
        ui.InitializeLevel(parameters.activePlayers, defaultStocks);
        remainingPlayers = parameters.activePlayers.Count;
        pelicanSpawner.FetchSpawnZones();
        pelicanSpawner.active = false;
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
        base.SpawnPlayer(player);
    }

    public override void OnPlayerDeath(Player player) {
        base.OnPlayerDeath(player);
        UpdateStock(player, -1);
        if (playerStocks[player] > 0) {
            SpawnPlayer(player);
        }
        else {
            remainingPlayers--;
            eliminatedPlayers.Add(player);
            if (remainingPlayers <= 1 && !endingLevel) {
                StartCoroutine(EndLevel());
            }
        }
    }

    private Player GetLastPlayerStanding() {
        foreach (var kvp in playerStocks) {
            if (kvp.Value > 0) {
                return kvp.Key;
            }
        }
        if (eliminatedPlayers.Count > 0) {
            return eliminatedPlayers[eliminatedPlayers.Count - 1];
        }
        return null;
    }

    public override void NextLevel() {
        ui.CloseLevelStats();
        // ui.OnLoadingStart();
        base.NextLevel();
    }

    private IEnumerator EndLevel() {
        endingLevel = true;
        pelicanSpawner.active = false;
        FreezeFish(true);
        foreach (var activePlayer in parameters.activePlayers) {
            activePlayer.OnDeath -= OnPlayerDeath;
            activePlayer.Gunfish.OnDeath -= OnPlayerDeath;
        }

        // todo: do a little async wait and THEN let the player win in case all the remaining players die at roughly the same time
        yield return new WaitForSeconds(endLevelDelay);

        // NOTE(Wyatt): I'm redoing scoring a bit. BITE ME!
        /*
        // Player score = which place they were eliminated at
        for (int i = 0; i < eliminatedPlayers.Count; i++) {
            playerScores[eliminatedPlayers[i]] += i;
        }
        */

        var player = GetLastPlayerStanding();
        if (player != null) {
            UpdateScore(player, 1);
            //UpdateScore(player, playerScores.Count);
        }
        ShowLevelWinner(player);
        //ui.ShowLevelStats((player == null) ? -1: player.playerNumber, playerScores); // if player is null, no one wins
        PlayerManager.Instance.SetInputMode(PlayerManager.InputMode.EndLevel);
    }

    protected virtual void ShowLevelWinner(Player player) {
        ui.ShowLevelStats((player == null) ? "No one wins!" : $"Player {player.PlayerNumber} wins!", playerScores);
    }

    public void UpdateScore(Player player, int scoreDelta) {
        playerScores[player] += scoreDelta;
        ui.OnScoreChange(player, playerScores[player]);
    }
    public void UpdateStock(Player player, int stockDelta) {
        playerStocks[player] += stockDelta;
        ui.OnStockChange(player, playerStocks[player]);
    }

    public override void ShowEndGameStats() {
        base.ShowEndGameStats();
        //Dictionary<int, int> teamScores = new Dictionary<int, int>() { { 0,0}, { 1,0} };
        int topScore = 0;
        List<Player> winners = new List<Player>();
        foreach (var playerScore in playerScores.OrderByDescending(x => x.Value)) {
            if (playerScore.Value >= topScore) { 
                winners.Add(playerScore.Key);
                topScore = playerScore.Value;
            };
        }

        string text = "It's a tie!";
        if (winners.Count == 0) {
            text = "No one wins?";
        }
        else if (winners.Count == 1) {
            text = $"Player {winners[0].PlayerNumber} wins!!!";
        }
        ui.ShowFinalScores(text, playerScores, winners);
    }

    /*
        int playerIdx = 0;
        int topScore = 0;
        List<Player> winners = new List<Player>();
        foreach (var playerScore in playerScores.OrderByDescending(x => x.Value)) {
            playerPanels[playerIdx].playerName.text = $"Player {playerScore.Key.playerNumber}";
            playerPanels[playerIdx].playerImg.sprite = playerScore.Key.gunfishData.sprite;
            playerPanels[playerIdx].playerScore.text = playerScore.Value.ToString();
            if (playerScore.Value >= topScore) {
                playerPanels[playerIdx].highlight.enabled = true;
                winners.Add(playerScore.Key);
                topScore = playerScore.Value;
            }
            playerPanels[playerIdx].panel.SetActive(true);
            playerIdx++;
        }

        if (winners.Count == 0) {
            winnerText.text = "No one wins?";
        }
        else if (winners.Count == 1) {
            winnerText.text = $"Player {winners[0].playerNumber} wins!!!";
        }
        else {
            winnerText.text = "It's a tie!";
        }
    */

    public override void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish) {
        base.HandleFishDamage(fishHit, gunfish);
        // if fish is dead
        if (gunfish.statusData.health > 0) {
            return;
        }

        // if src fish, then award points, otherwise detract points
        Gunfish sourceGunfish = fishHit.source.GetComponent<Gunfish>();
        sourceGunfish = sourceGunfish ?? fishHit.source.GetComponent<Gun>()?.gunfish;
        print(fishHit.source);
        if (sourceGunfish != null) {
            MarqueeManager.Instance.EnqueueRandomQuip();
            UpdateScore(sourceGunfish.player, 1);
        }
        else if (!endingLevel) {
            // todo: this should play a special suicide quip (Selfish Destruction!)
            MarqueeManager.Instance.EnqueueRandomQuip();
            UpdateScore(gunfish.player, -1);
        }
    }

    public override void OnTimerFinish() {
        base.OnTimerFinish();
        // todo: SUMMON THE FUCKING PELICANS
        pelicanSpawner.active = true;
        foreach (var (player, stock) in playerStocks) {
            if (stock > 1) {
                UpdateStock(player, -(playerStocks[player] - 1));
            }
        }
        // maybe play a quip? (SUDDEN DEATH!)
    }
}