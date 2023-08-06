using UnityEngine;

public enum GameModeType { DeathMatch, Race };

[CreateAssetMenu(fileName = "GameModeDetails", menuName = "ScriptableObjects/Game Mode Details", order = 0)]
public class GameMode : ScriptableObject {
    public GameModeType gameModeType;
    public Texture2D image;
    public GameObject matchManagerPrefab;
}