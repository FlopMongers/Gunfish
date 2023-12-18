using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class GameModeManager : PersistentSingleton<GameModeManager> {
    private GameObject gameModeInstance;
    public MatchManager matchManagerInstance { get; private set; }

    public void InitializeGameMode(GameModeType gameModeType, List<Player> players) {
        var gameMode = GameManager.Instance.GameModeList.gameModes.Where(element => element.gameModeType == gameModeType).FirstOrDefault();
        var gameParameters = new GameParameters(players, gameMode.levels.sceneNames, gameMode.levels.skyboxSceneName);
        var matchManagerPrefab = gameMode.matchManagerPrefab;
        gameModeInstance = Instantiate(matchManagerPrefab, transform);

        if (gameModeType == GameModeType.DeathMatch) {
            matchManagerInstance = gameModeInstance.GetComponent<DeathMatchManager>();
        }

        matchManagerInstance.Initialize(gameParameters);
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
