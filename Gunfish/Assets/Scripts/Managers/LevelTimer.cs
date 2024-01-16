using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public float levelDuration;
    public LevelTimerUI ui;
    public float timer;
    public bool awakened = false;

    public GameEvent OnTimerFinish;

    public void AwakenTimer() {
        awakened = true;
        ui.TimerFade(0, 1, 0.25f);
    }

    public void DisappearTimer() {
        awakened = false;
        ui.TimerFade(1, 0, 0.25f);
    }

    public void StartTime() {
        // update ui
        timer = levelDuration;
        AwakenTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (!awakened)
            return;

        timer -= Time.deltaTime;
        ui.UpdateTime(Mathf.RoundToInt(Mathf.Clamp(Mathf.Round(timer), 0, levelDuration)));
        if (timer <= 0) {
            // fire event and deaden timer
            OnTimerFinish?.Invoke();
            DisappearTimer();
        }
    }
}
