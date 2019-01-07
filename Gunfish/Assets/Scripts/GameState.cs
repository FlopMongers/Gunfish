using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameStateType { Menu, Lobby, Paused, Playing, End, Credits }

public static class GameState
{
    public static GameStateType gameState = GameStateType.Playing;
}
