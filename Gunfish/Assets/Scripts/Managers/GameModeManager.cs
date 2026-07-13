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


    public void InitializeGameMode(GameMode gameMode, List<Player> players, List<string> forcedLevels = null) {
        levels = forcedLevels ?? SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch);
        activePlayers = players.Where(player => player != null && player.Active).ToList();
        var gameParameters = new GameParameters(
            activePlayers,
            levels,
            gameMode.levels.skyboxSceneName,
            gameMode.secondsPerRound,
            gameMode.stocksPerRound
        );
        var matchManagerPrefab = gameMode.matchManagerPrefab;
        if (gameModeInstance != null) {
            // Must be immediate, not deferred: nested Singleton<T> components (e.g. LevelTimerUI)
            // need their OnDestroy() to run and clear Instance before the Instantiate() below runs
            // its own Awake() pass, or the new instance sees a stale InstanceExists == true and
            // self-destructs instead of registering itself.
            DestroyImmediate(gameModeInstance.gameObject);
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
        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++) {
            if (PlayerManager.Instance.Players[i] == null) continue;
            PlayerManager.Instance.SetPlayerFish(i, null);
        }

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
