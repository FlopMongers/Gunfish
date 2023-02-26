using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static readonly bool debug = true;

    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private LevelManager levelManager;

    [SerializeField]
    private PlayerManager playerManager;

    void Start() {

    }

    void Update() {

    }

    public void InitializeGame(IGameMode gameMode) {
        gameMode.Initialize();
    }
}
