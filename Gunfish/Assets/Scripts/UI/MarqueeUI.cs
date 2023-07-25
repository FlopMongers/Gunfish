using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;
using System;

[RequireComponent(typeof(UIDocument))]
public class MarqueeUI : MonoBehaviour {

    private struct MarqueeContents {
        public string text;
        public float duration;
        public AnimationCurve tween;
        public Action callback;

        public MarqueeContents(string text, float duration, AnimationCurve tween, Action callback) {
            this.text = text;
            this.duration = duration;
            this.tween = tween;
            this.callback = callback;
        }
    }

    [SerializeField]
    private float defaultDuration = 1f;
    [SerializeField]
    private AnimationCurve defaultTween = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private UIDocument document;
    private Label label;
    private Queue<MarqueeContents> queue = new();

    private bool transitioning = false;

    private void Start() {
        document = GetComponent<UIDocument>();
        label = document.rootVisualElement.Q<Label>("Text");
    }

    private void Update() {
        UpdateDebug();
    }

    private void UpdateDebug() {
        if (!GameManager.debug) return;

        if (Input.GetKeyDown(KeyCode.P)) {
            Enqueue("3");
            Enqueue("2");
            Enqueue("1");
            Enqueue("GO", () => { Debug.Log("Countdown over!"); });
        }
    }

    public void Enqueue(string text, Action callback = null) {
        Enqueue(text, defaultDuration, defaultTween, callback);
    }

    public void Enqueue(string text, float duration, Action callback = null) {
        Enqueue(text, duration, defaultTween, callback);
    }

    public void Enqueue(string text, float duration, AnimationCurve tween, Action callback = null) {
        var contents = new MarqueeContents(text, duration, tween, callback);
        queue.Enqueue(contents);
        if (!transitioning) {
            transitioning = true;
            StartCoroutine(Scroll());
        }
    }

    private void OnGUI() {
        if (!GameManager.debug) return;
        GUI.TextField(new Rect(0f, 20f, 200f, 20f), "Press P to start a countdown");
    }

    private IEnumerator Scroll() {
        while (queue.Count > 0) {
            MarqueeContents contents;
            if (!queue.TryDequeue(out contents)) {
                continue;
            }

            label.text = contents.text;
            
            float t = 0f;
            while (t < 1f) {
                var tween = contents.tween.Evaluate(t);
                var value = Mathf.Lerp(110f, -110f, tween);

                label.style.left = new Length(value, LengthUnit.Percent);

                t += Time.deltaTime / contents.duration;
                yield return new WaitForEndOfFrame();
            }

            contents.callback?.Invoke();
        }
        transitioning = false;
    }
}
