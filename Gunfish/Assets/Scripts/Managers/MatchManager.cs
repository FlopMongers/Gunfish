using System.Collections.Generic;
using UnityEngine;

public class MatchManager : PersistentSingleton<MatchManager> {
    protected GameParameters parameters;
    protected int currentLevel;

    protected List<Transform> spawnPoints = new List<Transform>();

    private int nextLevelIndex;

    public virtual void Initialize(GameParameters parameters) {
        this.parameters = parameters;
        LevelManager.instance.FinishLoadLevel_Event += StartLevel;
        LevelManager.instance.StartPlay_Event += StartPlay;
        NextLevel();
    }

    public virtual void SpawnPlayer(Player player) {

    }

    public virtual void StartLevel() {
        spawnPoints = new List<Transform>();
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("Spawn")) {
            spawnPoints.Add(spawnPoint.transform);
        }
        
        // Freeze the players
        foreach (var player in parameters.activePlayers) {
            player.FreezeControls = true;
        }
    }

    
    public virtual void StartPlay() {
        // unfreeze players
        foreach (var player in parameters.activePlayers) {
            player.FreezeControls = false;
        }
    }

    public virtual void OnPlayerDeath(Player player) {

    }

    public virtual void NextLevel()
    {
        if (nextLevelIndex < parameters.scenes.Count)
        {
            // TODO, add actual async loading with UI and stuff
            LevelManager.instance.LoadLevel(parameters.scenes[nextLevelIndex]);
            nextLevelIndex++;
        }
        else
        {
            LevelManager.instance.LoadStats();
        }
    }

    public virtual bool ResolveHit(Gun gun, GunfishSegment segment)
    {
        return gun.gunfish != segment.gunfish;
    }
}
