using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PiecewiseLinear
{
    public static float Evaluate(List<Vector2> points, float x, bool sorted=false) {
        if (!sorted) {
            points = points.OrderBy(p => p.x).ToList();
        }
        if(x < points[0].x || x >= points[points.Count - 1].x) {
            return 0f;
        }
        int beforeIndex = points.Count-1;
        for (int i = 0; i < points.Count; i++) {
            Vector2 p = points[i];
            if (p.x > x) {
                beforeIndex = i-1;
                break;
            }
        }
        float t = (x - points[beforeIndex].x) / (points[beforeIndex + 1].x - points[beforeIndex].x);
        return Mathf.Lerp(points[beforeIndex].y, points[beforeIndex + 1].y, t);
    }
    public static float DistanceToFunction(List<Vector2> points, Vector2 p, bool sorted = false) {
        float y = Evaluate(points,p.x, sorted);
        return y - p.y;
    }
}
