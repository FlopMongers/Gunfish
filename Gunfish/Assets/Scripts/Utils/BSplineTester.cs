using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSplineTester : MonoBehaviour {
    [SerializeField]
    private int degree = 3;

    private List<Vector2> controlPoints;
    private List<Vector2> points;

    private void OnValidate() {
        SetPoints();
        UpdateBSpline();
    }

    private void Start() {
        SetPoints();
        UpdateBSpline();
    }

    private void SetPoints() {
        controlPoints = new List<Vector2>(transform.childCount);
        for (int i = 0; i < transform.childCount; i++) {
            controlPoints.Add(transform.GetChild(i).position);
        }
    }

    private void UpdateBSpline() {
        points = BSpline.CreateBSpline(controlPoints, degree, 10);
    }

    private void OnDrawGizmos() {
        if (points == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var point in controlPoints) {
            Gizmos.DrawWireSphere(point, 0.1f);
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < points.Count - 1; i++) {
            var left = points[i];
            var right = points[i + 1];

            Gizmos.DrawLine(left, right);
        }
    }
}
