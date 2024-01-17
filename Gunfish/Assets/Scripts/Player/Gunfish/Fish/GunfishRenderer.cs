using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishRenderer {
    public LineRenderer LineRenderer { get; private set; }
    public List<GameObject> Segments { get; private set; }

    public GunfishRenderer(float widthMultiplier, Material material, List<GameObject> segments) {
        this.Segments = segments;

        LineRenderer = segments[0].AddComponent<LineRenderer>();
        LineRenderer.positionCount = segments.Count;
        LineRenderer.material = material;
        LineRenderer.sortingLayerName = "Fish";

        LineRenderer.widthMultiplier = widthMultiplier;
    }

    public void Render() {
        for (int i = 0; i < Segments.Count; i++) {
            var segment = Segments[i];
            if (!segment.transform.hasChanged)
                continue; //No need to reassign if it hasn't moved
            LineRenderer.SetPosition(i, segment.transform.position);
        }
    }
}