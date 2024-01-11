using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : Singleton<GameUIManager> {
    public GameEvent FinishStartTimer_Event;

    public void StartTimer() {
        // TODO, add actual UI
        FinishStartTimer_Event?.Invoke();
    }
}