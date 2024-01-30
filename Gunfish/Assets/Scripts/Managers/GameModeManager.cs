using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GameModeManager : PersistentSingleton<GameModeManager> {
    private GameObject gameModeInstance;
    public IMatchManager matchManagerInstance { get; private set; }

    [HideInInspector]
    public List<Player> activePlayers = new List<Player>();

    public void InitializeGameMode(GameModeType gameModeType, List<Player> players) {
        var gameMode = GameManager.Instance.GameModeList.gameModes.Where(element => element.gameModeType == gameModeType).FirstOrDefault();
        var levels = SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch);
        activePlayers = players.Where(player => player.Active).ToList();
        var gameParameters = new GameParameters(activePlayers, levels, gameMode.levels.skyboxSceneName);
        var matchManagerPrefab = gameMode.matchManagerPrefab;
        if (gameModeInstance != null) {
            Destroy(gameModeInstance.gameObject);
        }
        gameModeInstance = Instantiate(matchManagerPrefab, transform);

        if (gameModeType == GameModeType.DeathMatch) {
            matchManagerInstance = gameModeInstance.GetComponent<DeathMatchManager>();
        }
        print("trying");
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
        if (null != gameModeInstance) {
            matchManagerInstance.TearDown();
            Destroy(gameModeInstance);
        }
        matchManagerInstance = null;
    }

    public void NextLevel() {
        matchManagerInstance?.NextLevel();
    }
}
