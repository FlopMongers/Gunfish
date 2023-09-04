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
    public Transform startPoint, endPoint;

    public int numMiddleNodes;

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
        // set prevNode
        return node;
    }

    public void Garbulate() {
        Vector3 nodeSpace = Vector3.right * (endPoint.position.x - startPoint.position.x) / (numMiddleNodes + 1);
        // place start
        WaterSurfaceNode prevNode = SpawnNode(startPoint.position, null);
        for (int i = 0; i < numMiddleNodes+1; i++) {
            // instantiate node
            prevNode = SpawnNode(startPoint.position + (nodeSpace * (i+1)), prevNode);
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