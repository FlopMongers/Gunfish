using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public CountGameEvent NextLevel_Event;
    protected GameParameters parameters;

    protected List<Transform> spawnPoints;

    protected virtual void Start()
    {
        foreach (var spawnPoint in GameObject.FindGameObjectsWithTag("SpawnPoint"))
        {
            spawnPoints.Add(spawnPoint.transform);
        }
        GameUIManager.instance.FinishStartTimer_Event += OnTimerFinish;
    }

    public virtual void SpawnPlayer(Player player)
    {
    }

    public virtual void Initialize(GameParameters parameters)
    {
        // freeze the players
        foreach (var player in parameters.activePlayers)
        {
            player.FreezeControls = true;
        }
        this.parameters = parameters;
    }

    public virtual void OnTimerFinish()
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
}
