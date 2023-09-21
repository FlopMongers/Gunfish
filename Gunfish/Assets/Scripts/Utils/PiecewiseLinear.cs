using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public delegate Vector2 PositionGetter<T>(T point);

public class PiecewiseLinear {
    public static PositionGetter<Vector2> vecPosition = delegate (Vector2 point) { return point; };
    public static PositionGetter<Transform> transformPosition = delegate (Transform point) { return point.position; };

    public static int ClosestIndexBefore<T>(List<T> points, float x, PositionGetter<T> position, bool sorted = false) {

        if (!sorted) {
            points = points.OrderBy(p => position(p).x).ToList();
        }
        int beforeIndex = points.Count - 1;
        for (int i = 0; i < points.Count; i++) {
            Vector2 p = position(points[i]);
            if (p.x > x) {
                beforeIndex = i - 1;
                break;
            }
        }
        return beforeIndex;
    }
    public static float Evaluate(List<Vector2> points, float x, bool sorted=false) {
        if (x < points[0].x || x >= points[points.Count - 1].x) {
            return 0f;
        }
        int beforeIndex = ClosestIndexBefore<Vector2>(points, x, vecPosition, sorted);
        float t = (x - points[beforeIndex].x) / (points[beforeIndex + 1].x - points[beforeIndex].x);
        return Mathf.Lerp(points[beforeIndex].y, points[beforeIndex + 1].y, t);
    }
    public static float DistanceToFunction(List<Vector2> points, Vector2 p, bool sorted = false) {
        float y = Evaluate(points,p.x, sorted);
        return y - p.y;
    }
}
