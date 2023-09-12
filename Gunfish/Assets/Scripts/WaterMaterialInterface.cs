using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using System.Linq;
using static FunkyCode.Rendering.Universal.Sprite;

public class WaterMaterialInterface : MonoBehaviour {
    [HideInInspector]
    public List<Transform> waterSurfaceNodes;

    private double[] positionsX;
    private double[] positionsY;
    private double[] positionsdX;
    private double[] positionsdY;

    private double[] coefficients;
    private int degree = 10;

    private void Start() {
        Init();
    }

    public void PrintEquation(double[] coefficients, int degree) {
        int exponent = 0;
        string equation = "";
        foreach (var item in coefficients) {
            if (item == 0) {
                continue;
            }

            equation += item.ToString("0.00");

            if (exponent != 0) {
                equation += $"x^({exponent})";
            }

            if (exponent != degree) {
                equation += " + ";
            }
            exponent++;
        }
        print(equation);
    }

    public void Init() {
        waterSurfaceNodes = new List<Transform>();
        foreach (Transform child in transform.parent.Find("Nodes")) {
            if (child.GetComponent<WaterSurfaceNode>()) {
                waterSurfaceNodes.Add(child);
            }
        }

        waterSurfaceNodes.Sort((a, b) => a.position.x < b.position.x ? -1 : 1);

        positionsX = new double[waterSurfaceNodes.Count];
        positionsY = new double[waterSurfaceNodes.Count];
        positionsdX = new double[waterSurfaceNodes.Count];
        positionsdY = new double[waterSurfaceNodes.Count];

        GetComponent<SpriteRenderer>().material.SetInt("_NodeCount", waterSurfaceNodes.Count);
        GetComponent<SpriteRenderer>().material.SetInt("_Degree", degree);
    }

    private void Update() {
        for (int i = 0; i < waterSurfaceNodes.Count; i++) {
            positionsX[i] = waterSurfaceNodes[i].position.x;
            positionsY[i] = waterSurfaceNodes[i].position.y;
            positionsdX[i] = 0.333;
            positionsdY[i] = 0;

            if (i == waterSurfaceNodes.Count - 1) {
                continue;
            }

            var colliders = waterSurfaceNodes[i].GetComponents<BoxCollider2D>();
            var effector = waterSurfaceNodes[i].GetComponent<BuoyancyEffector2D>();
            var waterDimensions = GetComponentInParent<WaterSurfaceGenerator>().dimensions;

            Vector2 offset = (waterSurfaceNodes[i + 1].position - waterSurfaceNodes[i].position);
            var surfaceMidpoint = (Vector2)waterSurfaceNodes[i].position + offset / 2;

            var width = offset.x;
            var height = surfaceMidpoint.y - transform.parent.position.y + waterDimensions.y / 2;

            //effector.surfaceLevel = height / 2 - 5f;

            foreach (var collider in colliders) {
                collider.offset = new Vector2(offset.x / 2, -height / 2);
                collider.size = new Vector2(width, height);
            }
        }
        coefficients = Fit.Polynomial(positionsX, positionsY, degree);

        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesX", positionsX.Select(x => (float)x).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesY", positionsY.Select(y => (float)y).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesdX", positionsX.Select(x => (float)0.3333333).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesdY", positionsY.Select(y => (float)0).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_Coefficients", coefficients.Select(c => (float)c).ToArray());
    }

    Vector2 midpoint(Vector2 p1, Vector2 p2, float t) {
        return Vector2.Lerp(p1,p2,t);
    }

    Vector2 cubicBezier(Vector2[] controlPoints, float t) {
        Vector2 midpoint0 = midpoint(controlPoints[0],controlPoints[1],t);
        Vector2 midpoint1 = midpoint(controlPoints[1],controlPoints[2],t);
        Vector2 midpoint2 = midpoint(controlPoints[2],controlPoints[3],t);
        Vector2 midmid0 = midpoint(midpoint0,midpoint1,t);
        Vector2 midmid1 = midpoint(midpoint1,midpoint2,t);
        Vector2 midmidmid = midpoint(midmid0,midmid1,t);
        return midmidmid;
    }

    float bezierHeight(float x) {
        int prevIndex = ClosestIndexBefore(positionsX, x);
        int nextIndex = prevIndex+1;
        Vector2 prev = new Vector2((float)(positionsX[prevIndex]),(float)(positionsY[prevIndex]));
        Vector2 next = new Vector2((float)(positionsX[nextIndex]),(float)(positionsY[nextIndex]));
        Vector2 control0 = prev;
        Vector2 control1 = new Vector2((float)(prev.x+positionsdX[prevIndex]),(float)(prev.y+positionsdY[prevIndex]));
        Vector2 control2 = new Vector2((float)(next.x-positionsdX[nextIndex]),(float)(next.y-positionsdY[nextIndex]));
        Vector2 control3 = next;
        Vector2[] controlPoints = new Vector2[4] {control0,control1,control2,control3};
        float t = (x-prev.x)/(next.x-prev.x);
        return cubicBezier(controlPoints,t).y;
    }

    int ClosestIndexBefore(double[] positions, float x) {
        int beforeIndex = positions.Length - 1;
        for (int i = 0; i < positions.Length; i++) {
            float px = (float)positions[i];
            if (px > x) {
                beforeIndex = i - 1;
                break;
            }
        }
        return beforeIndex;
    }
}
