using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameParameters
{
    public List<Player> activePlayers;
    public GameObject gameUIObject;

    public GameParameters(List<Player> activePlayers, GameObject gameUIObject)
    {
        this.activePlayers = activePlayers;
        this.gameUIObject = gameUIObject; 
    }
}

public enum GameMode_Enum { DeathMatch };

[System.Serializable]
public class GameMode_Tuple
{
    public GameMode_Enum gameMode;
    public GameObject matchManagerPrefab;
    public GameObject gameUIObject;
}


public class GameManager : MonoBehaviour {

    public List<GameMode_Tuple> GameMode_List;
    Dictionary<GameMode_Enum, GameMode_Tuple> gameMode_Map;

    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private Player player;

    List<Player> players;

    void Start() {
        player.SpawnGunfish(Random.insideUnitCircle * 5f);
    }

    void Update() {

    }

    public void InitializeGame(GameMode_Enum gameMode) {
        // spawn match manager
        matchManager = Instantiate(gameMode_Map[gameMode].matchManagerPrefab).GetComponent<MatchManager>();
        // get all active players
        GameParameters parameters = new GameParameters(players, gameMode_Map[gameMode].gameUIObject);
        matchManager.Initialize(parameters);
    }

    
}
