using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public float levelDuration;
    // LevelTimerUI ui;
    public float timer;
    public bool awakened = false;

    public GameEvent OnTimerFinish;

    public void AwakenTimer() {
        awakened = true;
    }

    public void DisappearTimer() {
        awakened = false;
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
        // ui.UpdateTime(Mathf.Clamp(Mathf.Round(timer), 0, levelDuration));
        if (timer <= 0) {
            // fire event and deaden timer
            OnTimerFinish?.Invoke();
            DisappearTimer();
        }
    }
}
