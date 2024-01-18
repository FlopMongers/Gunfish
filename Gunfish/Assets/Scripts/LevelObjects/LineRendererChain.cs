using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineRendererChain : MonoBehaviour
{
    private LineRenderer line;
    [SerializeField]
    private List<Transform> transforms;
    
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        PopulateLineRendererPointCount();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLineRenderer();
    }

    private void PopulateLineRendererPointCount() {
        line.positionCount = transforms.Count;
    }

    private void UpdateLineRenderer() {
        for (int i = 0; i < transforms.Count; i++) {
            var segment = transforms[i];
            if (segment == null || !segment.transform.hasChanged)
                continue; //No need to reassign if it hasn't moved
            line.SetPosition(i, segment.transform.position);
        }
    }
}
