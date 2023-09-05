using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;
using System.Linq;

public class WaterMaterialInterface : MonoBehaviour {
    [SerializeField]
    private List<Transform> waterSurfaceNodes;
    private double[] positionsX;
    private double[] positionsY;

    private double[] coefficients;
    private int degree = 10;

    private void Start() {
        Init();
    }

    private void OnDrawGizmos() {
        if (coefficients == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i <= 100; i++) {
            float x = Mathf.Lerp(-10, 10, (float)i / 100);
            float y = Evaluate(x);
            Gizmos.DrawSphere(new Vector3(x, y), 0.1f);
        }
        Gizmos.color = Color.white;
    }

    private float Evaluate(float x) {
        if (coefficients == null)
            return 0f;
        float sum = 0;
        for (int i = 0; i < degree + 1; i++) {
            if (i >= coefficients.Length)
                continue;
            float xpow = 1;
            for (int j = 0; j < i; j++) {
                xpow *= x;
            }
            sum += (float)coefficients[i] * xpow;
        }
        return sum;
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
        foreach (Transform child in transform.parent) {
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
        }

        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesX", positionsX.Select(x => (float)x).ToArray());
        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesY", positionsY.Select(y => (float)y).ToArray());

        coefficients = Fit.Polynomial(positionsX, positionsY, degree);
        PrintEquation(coefficients, degree);
        GetComponent<SpriteRenderer>().material.SetFloatArray("_Coefficients", coefficients.Select(c => (float)c).ToArray());
    }

    float[] CalculatePolynomialCoefficients(float[] xValues, float[] yValues, int degree) {
        int n = xValues.Length;
        int numCoefficients = degree + 1;

        float[] coefficients = new float[numCoefficients];

        for (int i = 0; i < numCoefficients; i++)
        {
            float sumXToThePower = 0.0f;
            float sumYTimesXToThePower = 0.0f;

            for (int j = 0; j < n; j++)
            {
                sumXToThePower += Mathf.Pow(xValues[j], i);
                sumYTimesXToThePower += yValues[j] * Mathf.Pow(xValues[j], i);
            }

            coefficients[i] = sumYTimesXToThePower / sumXToThePower;
        }

        return coefficients;
    }

}
