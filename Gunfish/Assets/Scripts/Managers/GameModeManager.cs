using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


public class GameModeManager : PersistentSingleton<GameModeManager> {
    private GameObject gameModeInstance;
    public IMatchManager matchManagerInstance { get; private set; }

    [HideInInspector]
    public List<Player> activePlayers = new List<Player>();
    private List<string> levels;

    private List<LevelResultsData> levelResultsData = new List<LevelResultsData>();

    private DateTime matchStartTime;

    public void InitializeGameMode(GameModeType gameModeType, List<Player> players) {
        var gameMode = GameManager.Instance.GameModeList.gameModes.Where(element => element.gameModeType == gameModeType).FirstOrDefault();
        levels = SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch);
        activePlayers = players.Where(player => player.Active).ToList();
        var gameParameters = new GameParameters(activePlayers, levels, gameMode.levels.skyboxSceneName);
        var matchManagerPrefab = gameMode.matchManagerPrefab;
        if (gameModeInstance != null) {
            Destroy(gameModeInstance.gameObject);
        }
        gameModeInstance = Instantiate(matchManagerPrefab, transform);

        matchManagerInstance = gameModeInstance.GetComponent<IMatchManager>();
        matchManagerInstance.Initialize(gameParameters);
        matchStartTime = DateTime.Now;
    }

    public List<string> SelectLevels(List<string> levelSet, int quantity) {
        if (quantity > levelSet.Count) {
            throw new UnityException($"Cannot select {quantity} levels from level set of size {levelSet.Count}");
        }

        // Randomly select "quantity" levels
        levelSet.Shuffle();
        return levelSet.GetRange(0, quantity);
    }

    public void TeardownGameMode() {
        Debug.Log("Tearing down Gamemode");
        
        LogMatch();

        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++) {
            PlayerManager.Instance.SetPlayerFish(i, null);
        }

        if (null != gameModeInstance) {
            matchManagerInstance.TearDown();
            Destroy(gameModeInstance);
        }
        matchManagerInstance = null;
    }

    private void LogMatch() {

        var matchResult = new MatchResult
        {
            GameMode = matchManagerInstance.GetType().ToString().Replace("MatchManager", ""),
            StartTime = matchStartTime.ToString(),
            EndTime = DateTime.Now.ToString(),
            LevelCount = levels.Count,
            PlayerCount = activePlayers.Count
        };

        var playerResults = new List<PlayerMatchResult>();
        foreach (var player in activePlayers) {
            var playerMatchResult = new PlayerMatchResult
            {
                MatchResultId = matchResult.Id,
                PlayerId = player.PlayerNumber,
                PlayerFish = player.gunfishData.name,
                TotalScore = matchManagerInstance.GetPlayerScore(player),
                TotalKills = matchManagerInstance.GetPlayerKills(player),
                TotalDeaths = matchManagerInstance.GetPlayerDeaths(player),
                Rating = 0
            };
            playerResults.Add(playerMatchResult);
        }

        foreach (var levelData in levelResultsData) {
            levelData.levelResult.MatchResultId = matchResult.Id;
            foreach (var plr in levelData.playerLevelResults) {
                plr.MatchResultId = matchResult.Id;
            }
        }

        StatsManager.SaveMatchResults(matchResult, playerResults, levelResultsData);
        levelResultsData.Clear();
    }

    public void LogLevel(DateTime startTime) {
        var levelResult = new LevelResult
        {
            LevelName = matchManagerInstance.GetCurrentLevelName(),
            StartTime = startTime.ToString(),
            EndTime = DateTime.Now.ToString()
        };

        levelResultsData.Add(new LevelResultsData
        {
            levelResult = levelResult,
            playerLevelResults = new List<PlayerLevelResult>()
        });

        foreach (var player in activePlayers) {

            var playerLevelResult = new PlayerLevelResult
            {
                LevelResultId = levelResult.Id,
                PlayerId = player.PlayerNumber,
                Score = matchManagerInstance.GetPlayerScore(player),
                Kills = matchManagerInstance.GetPlayerKills(player),
                Deaths = matchManagerInstance.GetPlayerDeaths(player)
            };
            levelResultsData[levelResultsData.Count - 1].playerLevelResults.Add(playerLevelResult);
        }
    }

    public void NextLevel() {
        matchManagerInstance?.NextLevel();
    }
}
