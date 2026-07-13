using System.Collections.Generic;
using UnityEngine;

public enum GameModeType { DeathMatch, Race, Bassball, GunfishGame };

[CreateAssetMenu(fileName = "New Game Mode", menuName = "Scriptable Objects/Game Mode")]
public class GameMode : ScriptableObject {
    public GameModeType gameModeType;
    public Sprite image;
    public GameObject matchManagerPrefab;
    public SceneList levels;
    [Range(1, 5)] public int roundsPerMatch = 3;
    [Range(30f, 300f)] public float secondsPerRound = 120f;
    [Tooltip("Number of lives each player has per round. Set to -1 for unlimited lives.")] [Range(-1, 10)] public int stocksPerRound;
    public string description = "";
    public List<int> requiredPlayerCount = new List<int> { 2 };
}
