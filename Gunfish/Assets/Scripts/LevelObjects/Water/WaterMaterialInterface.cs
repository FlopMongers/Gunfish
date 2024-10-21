using MathNet.Numerics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterMaterialInterface : MonoBehaviour {
    [HideInInspector]
    public List<Transform> waterSurfaceNodes;

    public double[] positionsX;
    public double[] positionsY;

    private double[] coefficients;
    private int degree = 9;

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

        var count = waterSurfaceNodes.Count;
        var width = waterSurfaceNodes[1].position.x - waterSurfaceNodes[0].position.x;
        positionsX = new double[count + 2];
        positionsY = new double[count + 2];
        positionsX[0] = waterSurfaceNodes[0].position.x - width;
        positionsY[0] = waterSurfaceNodes[0].position.y;
        positionsX[count + 1] = waterSurfaceNodes[count-1].position.x + width;
        positionsY[count + 1] = waterSurfaceNodes[count-1].position.y;

        GetComponent<SpriteRenderer>().material.SetInt("_NodeCount", waterSurfaceNodes.Count);
        GetComponent<SpriteRenderer>().material.SetInt("_Degree", degree);
    }

    private void Update() {
        for (int i = 0; i < waterSurfaceNodes.Count; i++) {
            positionsX[i+1] = waterSurfaceNodes[i].position.x;
            positionsY[i+1] = waterSurfaceNodes[i].position.y;

            if (i == waterSurfaceNodes.Count - 1) {
                continue;
            }

            var collider = waterSurfaceNodes[i].GetComponent<BoxCollider2D>();
            var effector = waterSurfaceNodes[i].GetComponent<BuoyancyEffector2D>();
            var waterDimensions = GetComponentInParent<WaterSurfaceGenerator>().dimensions;

            var p1 = waterSurfaceNodes[i].localPosition;
            var p2 = waterSurfaceNodes[i+1].localPosition;
            var width = p2.x - p1.x;
            var pm = (p1 + p2) / 2f;
            var pb = new Vector2(-waterDimensions.x, -waterDimensions.y);
            var size = new Vector2(width, pm.y - pb.y);
            var offset = new Vector2(width / 2f, pm.y - p1.y - size.y / 2f);

            collider.size = size;
            collider.offset = offset;

        }
        coefficients = Fit.Polynomial(positionsX, positionsY, degree);

        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesX", positionsX.Select(x => (float)x).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesY", positionsY.Select(y => (float)y).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_Coefficients", coefficients.Select(c => (float)c).ToArray());
    }
}