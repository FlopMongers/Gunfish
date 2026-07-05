using System.Collections.Generic;
using UnityEngine;

public class GunfishRenderer : MonoBehaviour {
    public LineRenderer LineRenderer { get; private set; }
    private List<GameObject> segments = new List<GameObject>();

    public void Init(float widthMultiplier, Material material, List<GameObject> segments) {
        this.segments = segments;

        LineRenderer = gameObject.CheckAddComponent<LineRenderer>();
        LineRenderer.positionCount = segments.Count;
        LineRenderer.material = material;
        LineRenderer.sortingLayerName = "Fish";
        LineRenderer.widthMultiplier = widthMultiplier;

        Render();
    }

    void Update() {
        Render();
    }

    public void Render() {
        for (int i = 0; i < segments.Count; i++) {
            var segment = segments[i];
            if (segment == null || !segment.transform.hasChanged)
                continue;
            LineRenderer.SetPosition(i, segment.transform.position);
        }
    }
}
