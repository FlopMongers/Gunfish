using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public float levelDuration;
    // LevelTimerUI ui;
    float startTime;
    bool awakened;

    public GameEvent OnTimerFinish;

    public void AwakenTimer() {
        awakened = true;
    }

    public void DisappearTimer() {
        awakened = false;
    }

    public void StartTime() {
        // update ui
        startTime = Time.time;
        AwakenTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (!awakened)
            return;

        // get elapsed time since start time and update timer
        float timeRemaining = Mathf.Round(Time.time - startTime);
        // ui.UpdateTime(Mathf.Min(0, Mathf.Round(Time.time - startTime)));
        if (timeRemaining <= 0) {
            // fire event and deaden timer
            OnTimerFinish?.Invoke();
            DisappearTimer();
        }
    }
}
