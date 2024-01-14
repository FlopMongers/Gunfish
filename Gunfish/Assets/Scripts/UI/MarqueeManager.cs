using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarqueeManager : PersistentSingleton<MarqueeManager> {
    [Serializable]
    private struct Quip {
        public string text;
        public AudioClip clip;
    }

    [Serializable]
    private struct MarqueeData {
        public float duration;
        public AnimationCurve tween;
        public TMP_Text text;
        
        [HideInInspector]
        public float t;

        public void Init() {
            t = 1f;
            text.SetText("");
            text.rectTransform.anchoredPosition = new Vector2(-Screen.width, 0f);
        }
    }
    
    [SerializeField]
    private List<Quip> quips = new();

    [SerializeField]
    private MarqueeData titleSettings;
    [SerializeField]
    private MarqueeData quipSettings;


    private void Start() {
        titleSettings.Init();
        quipSettings.Init();
    }

    private void Update() {
        UpdateData(ref titleSettings);
        UpdateData(ref quipSettings);
    }

    private void UpdateData(ref MarqueeData data) {
        if (data.t < 1f) {
            var tween = data.tween.Evaluate(data.t);
            var value = Mathf.Lerp(Screen.width, -Screen.width, tween);
            data.text.rectTransform.anchoredPosition = new Vector2(value, 0f);

            data.t += Time.deltaTime / data.duration;
        }
    }

    public void PlayTitle(string title) {
        titleSettings.text?.SetText(title);
        titleSettings.t = 0;
    }

    public void PlayRandomQuip() {
        if (quips == null || quips.Count == 0) {
            Debug.LogWarning("Could not enqueue quip. Make sure you have at least one in the MarqueeManager");
            return;
        }

        var index = UnityEngine.Random.Range(0, quips.Count);
        PlayQuip(quips[index]);
    }

    private void PlayQuip(Quip quip) {
        ArduinoManager.Instance.PlayClip(quip.clip);
        quipSettings.text?.SetText(quip.text);
        quipSettings.t = 0;
    }
}
