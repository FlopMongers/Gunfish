using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishRenderer {
    private Gunfish gunfish;
    private List<GameObject> segments;
    private LineRenderer line;

    public GunfishRenderer(Material material, List<GameObject> segments) {
        this.segments = segments;

        line = segments[0].AddComponent<LineRenderer>();
        line.positionCount = segments.Count;
        line.material = material;
    }

    public void Render() {
        for (int i = 0; i < segments.Count; i++) {
            var segment = segments[i];
            if (!segment.transform.hasChanged)
                continue; //No need to reassign if it hasn't moved
            line.SetPosition(i, segment.transform.position);
        }
    }
}