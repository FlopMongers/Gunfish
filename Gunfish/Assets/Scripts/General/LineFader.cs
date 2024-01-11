using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineFader : Fader
{
    protected List<LineRenderer> lines = new List<LineRenderer>();

    public override void FetchRenderers() {
        base.FetchRenderers();
        lines = GetComponentsInChildren<LineRenderer>().ToList();
    }

    public override void SetRenderersAlpha() {
        base.SetRenderersAlpha();
        foreach (var renderer in lines) {
            Color color = renderer.material.color;
            color.a = fadeValue;
            renderer.material.color = color;
        }
    }
}
