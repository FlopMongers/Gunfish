using UnityEngine;

public enum GameModeType { TeamDeathmatch, DeathMatch, Race };

[CreateAssetMenu(fileName = "New Game Mode", menuName = "Scriptable Objects/Game Mode")]
public class GameMode : ScriptableObject {
    public GameModeType gameModeType;
    public Texture2D image;
    public GameObject matchManagerPrefab;
    public SceneList levels;
}
