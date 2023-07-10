using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MatchManager : MonoBehaviour
{
    protected GameParameters parameters;
    protected int currentLevel;

    protected List<Transform> spawnPoints;

    private int nextLevelIndex;

    protected virtual void Start()
    {
    }

    public virtual void Initialize(GameParameters parameters)
    {
        this.parameters = parameters;
        LevelManager.instance.FinishLoadLevel_Event += StartLevel;
        LevelManager.instance.StartPlay_Event += StartPlay;

        NextLevel();
    }

    public virtual void SpawnPlayer(Player player)
    {
    }

    public virtual void StartLevel()
    {
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("Spawn"))
        {
            spawnPoints.Add(spawnPoint.transform);
        }
        // freeze the players
        foreach (var player in parameters.activePlayers)
        {
            player.FreezeControls = true;
        }
    }

    
    public virtual void StartPlay()
    {
        // unfreeze players
        foreach (var player in parameters.activePlayers)
        {
            player.FreezeControls = false;
        }
    }

    public virtual void OnPlayerDeath(Player player)
    {
    }

    public virtual void NextLevel()
    {
        if (nextLevelIndex < parameters.scenes.Count)
        {
            // TODO, add actual async loading with UI and stuff
            LevelManager.instance.LoadLevel(parameters.scenes[nextLevelIndex].name);
            nextLevelIndex++;
        }
        else
        {
            LevelManager.instance.LoadStats();
        }
    }
}
