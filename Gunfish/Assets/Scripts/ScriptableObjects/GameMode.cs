using UnityEngine;

public enum GameModeType { DeathMatch, Race, Bassball };

[CreateAssetMenu(fileName = "New Game Mode", menuName = "Scriptable Objects/Game Mode")]
public class GameMode : ScriptableObject {
    public GameModeType gameModeType;
    public Sprite image;
    public GameObject matchManagerPrefab;
    public SceneList levels;
    public int roundsPerMatch = 3;
}
