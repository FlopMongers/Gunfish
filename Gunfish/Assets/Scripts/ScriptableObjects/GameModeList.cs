using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Mode List", menuName = "Scriptable Objects/Game Mode List")]
public class GameModeList : ScriptableObject {
    public List<GameMode> gameModes;
}
