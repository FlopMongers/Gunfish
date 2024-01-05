using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public enum FadeMode { Stop, Fade, PingPong }

public class Fader : MonoBehaviour
{
    public bool fetchRenderersOnStart = true;
    protected List<SpriteRenderer> renderers = new List<SpriteRenderer>();

    public Vector2 fadeRange = Vector2.one;

    public FadeMode fadeMode = FadeMode.Stop;

    public float fadeSpeed = 1f;
    protected float fadeValue;

    public GameEvent OnFadeDone;

    // Start is called before the first frame update
    void Start()
    {
        if (fetchRenderersOnStart)
            FetchRenderers();
        fadeValue = fadeRange.x;
        // todo: map to record initial alpha values of sprite renderers
        SetRenderersAlpha();
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeMode == FadeMode.Stop)
            return;

        SetRenderersAlpha();
        // move towards target
        var delta = ((fadeValue > fadeRange.y) ? -fadeSpeed : fadeSpeed) * Time.deltaTime;
        if (Mathf.Abs(fadeRange.y - fadeValue) < delta)
            delta = fadeRange.y - fadeValue;
        fadeValue += delta;
        if (Mathf.Approximately(fadeValue, fadeRange.y)) {
            OnFadeDone?.Invoke();
            if (fadeMode == FadeMode.Fade) {
                fadeMode = FadeMode.Stop;
            }
            else {
                fadeRange = new Vector2(fadeRange.y, fadeRange.x);
            }
        }
    }

    public virtual void FetchRenderers() {
        renderers = GetComponentsInChildren<SpriteRenderer>().ToList();
    }

    public virtual void SetRenderersAlpha() {
        foreach (var renderer in renderers) {
            Color color = renderer.color;
            color.a = fadeValue;
            renderer.color = color;
        }
    }

    public void SetTarget(Vector2 fadeRange, FadeMode fadeMode=FadeMode.Fade) {
        this.fadeMode = fadeMode;
        this.fadeRange = fadeRange;
        if (!fetchRenderersOnStart)
            FetchRenderers();

    }
}
