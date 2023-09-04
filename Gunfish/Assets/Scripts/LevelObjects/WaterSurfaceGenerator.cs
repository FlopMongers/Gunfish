#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;
using UnityEditor;

public class WaterSurfaceGenerator : MonoBehaviour {
    [SerializeField]
    private Transform startPoint;
    [SerializeField]
    private Transform endPoint;

    [SerializeField]
    [Range(1f, 32f)]
    private float nodesPerUnit;

    public GameObject waterNodePrefab;

    public void ClearCurrentNodes() {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach(Transform child in transform) {
            if(child.gameObject.GetComponent<WaterSurfaceNode>()) {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach(GameObject obj in toDestroy) {
            DestroyImmediate(obj);
        }
    }

    WaterSurfaceNode SpawnNode(Vector3 position, WaterSurfaceNode prevNode) {
        // instantiate node
        var node = Instantiate(waterNodePrefab, position, Quaternion.identity).GetComponent<WaterSurfaceNode>();
        // hook up to prevNode
        if (prevNode != null) {
            node.prevSpring.connectedBody = prevNode.rb;
        } else {
            node.prevSpring.enabled = false;
        }
        node.selfSpring.connectedAnchor = node.transform.position;
        node.transform.parent = transform;
        // set previousNode
        return node;
    }

    public void Garbulate() {
        ClearCurrentNodes();
        
        float length = Vector3.Magnitude(endPoint.position - startPoint.position);
        int nodeCount = Mathf.RoundToInt(nodesPerUnit * length);

        float delta = 1f / nodeCount;

        WaterSurfaceNode previousNode = null;
        for (int i = 0; i <= nodeCount; i++) {
            Vector3 targetPosition = Vector3.Lerp(startPoint.position, endPoint.position, i * delta);
            previousNode = SpawnNode(targetPosition, previousNode);
        }
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(WaterSurfaceGenerator))]
public class WaterSurfaceGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var scripts = targets.OfType<WaterSurfaceGenerator>();
        if (GUILayout.Button("GARBULATE")) {
            foreach (var script in scripts) {
                script.ClearCurrentNodes();
                script.Garbulate();
            }
        }
    }
}
#endif