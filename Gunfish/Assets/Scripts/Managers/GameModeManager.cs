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
        
        LogGame();

        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++) {
            PlayerManager.Instance.SetPlayerFish(i, null);
        }

        if (null != gameModeInstance) {
            matchManagerInstance.TearDown();
            Destroy(gameModeInstance);
        }
        matchManagerInstance = null;
    }

    private void LogGame() {
        var outputFile = Path.Combine(Application.persistentDataPath, "gunfish.csv");
        // var header = "player1,score1,player2,score2,player3,score3,player4,score4,map,rating";
        var line = "";
        for (int i = 0; i < 4; i++)
        {
            var fish = "";
            if (i < PlayerManager.Instance.PlayerFish.Count) {
                if (PlayerManager.Instance.PlayerFish[i]) {
                    fish = PlayerManager.Instance.PlayerFish[i].name;
                }
            }
            line += fish + ",";

            var score = "";
            if (i < PlayerManager.Instance.Players.Count) {
                if (PlayerManager.Instance.Players[i]) {
                    var prospectiveScore = matchManagerInstance.GetPlayerScore(PlayerManager.Instance.Players[i]);
                    if (prospectiveScore >= 0) {
                        score = prospectiveScore.ToString();
                    }
                }
            }
            line += score + ",";
        }
        // assume 1 level
        var map = Path.GetFileNameWithoutExtension(levels[0]);
        line += map + ",";
        line += "1";
        StreamWriter writer = new StreamWriter(outputFile, true);
        writer.WriteLine(line);
        writer.Close();
        print("Wrote to " + outputFile);
    }

    public void NextLevel() {
        matchManagerInstance?.NextLevel();
    }
}
