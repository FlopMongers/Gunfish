using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMaterialInterface : MonoBehaviour {
    [SerializeField]
    private List<Transform> waterSurfaceNodes;
    private float[] positionBufferX;
    private float[] positionBufferY;

    private void Start() {
        Init();
    }

    public void Init() {
        waterSurfaceNodes = new List<Transform>();
        foreach (Transform child in transform.parent) {
            if (child.GetComponent<WaterSurfaceNode>()) {
                waterSurfaceNodes.Add(child);
            }
        }

        waterSurfaceNodes.Sort((a, b) => a.position.x < b.position.x ? -1 : 1);

        positionBufferX = new float[1000];
        positionBufferY = new float[1000];
        for (int i = 0; i < 1000; i++) {
            positionBufferX[i] = 0f;
            positionBufferY[i] = 0f;
        }

        GetComponent<SpriteRenderer>().material.SetInt("_NodeCount", waterSurfaceNodes.Count);
    }

    private void Update() {
        for (int i = 0; i < waterSurfaceNodes.Count; i++) {
            positionBufferX[i] = waterSurfaceNodes[i].position.x;
            positionBufferY[i] = waterSurfaceNodes[i].position.y;
        }

        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesX", positionBufferX);
        GetComponent<SpriteRenderer>().material.SetFloatArray("_NodesY", positionBufferY);

        var coefficients = CalculatePolynomialCoefficients(positionBufferX, positionBufferY, waterSurfaceNodes.Count);

        GetComponent<SpriteRenderer>().material.SetFloatArray("_Coefficients", coefficients);
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
