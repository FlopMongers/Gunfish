using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DG;
using DG.Tweening;

public class LevelTimerUI : Singleton<LevelTimerUI>
{
    public TextMeshProUGUI timerText;
    public CanvasGroup canvasGroup;
    
    public void UpdateTime(int timer) {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timer);
        string formattedTime = $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        timerText.text = formattedTime;
    }

    public void TimerFade(float from, float to, float duration) {
        //awakened = true;
        canvasGroup.DOFade(to, duration).From(from);
    }
}
