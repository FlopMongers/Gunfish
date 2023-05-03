using UnityEngine;

public enum GameModeType { DeathMatch };

[CreateAssetMenu(fileName = "GameModeDetails", menuName = "ScriptableObjects/Game Mode Details", order = 0)]
public class GameMode : ScriptableObject {
    public Texture2D image;
    public GameObject matchManagerPrefab;
    public GameObject gameUIObject;
}