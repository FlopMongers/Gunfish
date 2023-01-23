using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public LevelManager levelManager;

    void Start() {

    }

    void Update() {

    }

    public void InitializeGame(IGameMode gameMode) {
        gameMode.Initialize();
    }
}
