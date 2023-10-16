using System;
using System.Collections.Generic;
using UnityEngine;

public class BSpline {
    public static List<Vector2> CreateBSpline(List<Vector2> controlPoints, int degree, int numberOfSegments) {
        int n = controlPoints.Count - 1;
        int m = n + degree + 1;

        if (n < degree + 1 || degree < 1)
            throw new ArgumentException("Invalid degree or control points count.");

        List<Vector2> curvePoints = new List<Vector2>(numberOfSegments + 1);
        float delta = 1.0f / numberOfSegments;

        for (int i = 0; i <= numberOfSegments; i++) {
            float t = i * delta;
            curvePoints.Add(CalculateBSplinePoint(controlPoints, t, degree, m));
        }

        return curvePoints;
    }

    private static Vector2 CalculateBSplinePoint(List<Vector2> controlPoints, float t, int degree, int m) {
        int n = controlPoints.Count - 1;
        Vector2 result = Vector2.zero;

        for (int i = 0; i <= n; i++) {
            float blend = CalculateBSplineBlend(i, degree, t, m);
            result += controlPoints[i] * blend;
        }

        return result;
    }

    private static float CalculateBSplineBlend(int i, int degree, float t, int m) {
        if (degree == 1) {
            if (i <= t && t < i + 1) {
                return 1.0f;
            }
            else {
                return 0.0f;
            }
        }

        float blend = 0.0f;

        if (t < i || t >= i + degree + 1) {
            return 0.0f;
        }

        if (degree == 2) {
            if (i == 0 && t >= 0 && t < 1) {
                blend = (1 - t) * (1 - t);
            }
            else if (i == 1 && t >= 0 && t < 1) {
                blend = 2 * t * (1 - t);
            }
            else if (i == 2 && t >= 0 && t < 1) {
                blend = t * t;
            }
        }
        else {
            blend = ((t - i) / (degree)) * CalculateBSplineBlend(i, degree - 1, t, m)
                + ((i + degree + 1 - t) / (degree)) * CalculateBSplineBlend(i + 1, degree - 1, t, m);
        }

        return blend;
    }
}