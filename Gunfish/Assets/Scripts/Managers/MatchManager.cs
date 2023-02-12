using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    MapManager currentMapManager;
    GameObject mapManagerPrefab;
    GameParameters parameters;

    // Start is called before the first frame update
    void Start()
    {
    }

    public virtual void NextLevel(int remainingPlayers)
    {
        if (currentMapManager != null)
        {
            currentMapManager.NextLevel_Event -= NextLevel;
        }
        // if it's the end, go to the stats screen
        // else level manager load next map, on finish run finish load level
    }

    public void FinishLoadLevel()
    {
        currentMapManager = Instantiate(mapManagerPrefab).GetComponent<MapManager>();
        currentMapManager.Initialize(parameters);
        currentMapManager.NextLevel_Event += NextLevel; 
    }

    public virtual void Initialize(GameParameters parameters)
    {
        this.parameters = parameters;
    }
}
