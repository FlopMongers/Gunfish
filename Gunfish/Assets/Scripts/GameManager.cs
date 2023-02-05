using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private LevelManager levelManager;

    void Start() {

    }

    void Update() {

    }

    public void InitializeGame(IGameMode gameMode) {
        gameMode.Initialize();
    }
}
