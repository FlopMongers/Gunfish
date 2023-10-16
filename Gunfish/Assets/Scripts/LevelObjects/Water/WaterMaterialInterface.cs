using MathNet.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FunkyCode.Rendering.Universal.Sprite;

public class WaterMaterialInterface : MonoBehaviour {
    [HideInInspector]
    public List<Transform> waterSurfaceNodes;

    private double[] positionsX;
    private double[] positionsY;

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

        GetComponent<SpriteRenderer>().material.SetInt("_NodeCount", waterSurfaceNodes.Count);
        GetComponent<SpriteRenderer>().material.SetInt("_Degree", degree);
    }

    private void Update() {
        for (int i = 0; i < waterSurfaceNodes.Count; i++) {
            positionsX[i] = waterSurfaceNodes[i].position.x;
            positionsY[i] = waterSurfaceNodes[i].position.y;

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
        GetComponent<SpriteRenderer>().material.SetFloatArray("_Coefficients", coefficients.Select(c => (float)c).ToArray());
    }
}