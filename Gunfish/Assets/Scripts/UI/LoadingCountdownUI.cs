using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCountdownUI : MonoBehaviour {
    private Animator anim;
    private string countdownAnimation = "Countdown";
    private string loadingAnimation = "Loading";

    void Awake() {
        anim = GetComponent<Animator>();
    }

    public void ShowLoadingScreen() {
        anim.Play(loadingAnimation);
    }

    public void StartCountdown() {
        anim.Play(countdownAnimation);
    }
}