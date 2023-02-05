using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private Player player;

    void Start() {
        player.SpawnGunfish(Random.insideUnitCircle * 5f);
    }

    void Update() {

    }

    public void InitializeGame(IGameMode gameMode) {
        gameMode.Initialize();
    }
}
