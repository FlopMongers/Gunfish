using System.Collections.Generic;
using UnityEngine;

public class MatchManager : PersistentSingleton<MatchManager> {
    protected GameParameters parameters;
    protected int currentLevel;

    protected List<Transform> spawnPoints = new List<Transform>();

    private int nextLevelIndex;
    bool done;

    public virtual void Initialize(GameParameters parameters) 
    {
        this.parameters = parameters;
        LevelManager.instance.FinishLoadLevel_Event += StartLevel;
        LevelManager.instance.StartPlay_Event += StartPlay;
        NextLevel();
    }

    public virtual void SpawnPlayer(Player player) 
    {
        // add player's fish to camera group
        GameCamera.instance?.targetGroup.AddMember(player.Gunfish.MiddleSegment.transform, 1, 1);
    }

    public virtual void StartLevel() 
    {
        spawnPoints = new List<Transform>();
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("Spawn")) {
            spawnPoints.Add(spawnPoint.transform);
        }

        FreezeFish(true);
        // move this to the level manager, maybe
        // PlayerManager.instance.SetInputMode(PlayerManager.InputMode.Player);
    }


    public virtual void StartPlay() 
    {
        // unfreeze players
        FreezeFish(false);
    }

    public void FreezeFish(bool freeze)
    {
        foreach (var player in parameters.activePlayers)
        {
            player.FreezeControls = freeze;
        }
    }

    public virtual void OnPlayerDeath(Player player) 
    {
        // remove the fishy from the camera group
        // will this work? I don't know...
        GameCamera.instance?.targetGroup.RemoveMember(null);
    }

    public virtual void ShowStats()
    {

    }

    public virtual void NextLevel() 
    {
        if (nextLevelIndex < parameters.scenes.Count) {
            LevelManager.instance.LoadLevel(parameters.scenes[nextLevelIndex]);
            nextLevelIndex++;
        }
        else if (done == true) 
        {
            // NOTE destroy all players
            LevelManager.instance.LoadMainMenu();
            Destroy(gameObject);
        }
        else {
            done = true;
            LevelManager.instance.LoadStats();
        }
    }

    public virtual bool ResolveHit(Gun gun, GunfishSegment segment) 
    {
        return gun.gunfish != segment.gunfish;
    }
}
