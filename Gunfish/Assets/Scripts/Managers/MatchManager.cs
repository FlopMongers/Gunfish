using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    MapManager currentMapManager;
    GameObject mapManagerPrefab;
    GameParameters parameters;
    protected int currentLevel;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        LevelManager.instance.FinishLoadLevel_Event += FinishLoadLevel;
    }

    public virtual void NextLevel(int remainingPlayers)
    {
        if (currentMapManager != null)
        {
            currentMapManager.NextLevel_Event -= NextLevel;
        }

        LevelManager.instance.LoadNextLevel();
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
