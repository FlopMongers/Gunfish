using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public CountGameEvent NextLevel_Event;
    GameParameters parameters;


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
        // get game ui object, tell it to run the start timer
        // hook into on timer complete
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
