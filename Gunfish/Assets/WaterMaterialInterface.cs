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
    }
}
