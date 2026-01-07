using UnityEngine;

[ExecuteAlways]
public class LineRendererPoints : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform[] points;

    void Update()
    {
        if (lineRenderer == null || points == null)
            return;

        lineRenderer.positionCount = points.Length;
        lineRenderer.widthCurve = AnimationCurve.EaseInOut(0f, points[0].lossyScale.x, 0.1f, points[points.Length-1].lossyScale.x);
        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, points[i].position);
        }
    }
}
