using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
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

public class WaterSurfaceGenerator : MonoBehaviour {
    [SerializeField]
    [Range(1f, 32f)]
    private float nodesPerUnit;

    [SerializeField]
    private GameObject waterNodePrefab;

    [SerializeField]
    private GameObject renderers;

    [SerializeField]
    private GameObject nodesContainer;

    protected float length, height;

    public Vector2 dimensions = new Vector2(5f, 5f);

    private void Start() {

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position - new Vector3(0f, dimensions.y / 2f), dimensions);
    }

    public void ClearCurrentNodes() {
        var nodes = transform.Find("Nodes");

        for (int i = nodes.childCount - 1; i >= 0; i--) {
            DestroyImmediate(nodes.GetChild(i).gameObject);
        }
    }

    WaterSurfaceNode SpawnNode(Vector3 position, WaterSurfaceNode prevNode) {
        var node = Instantiate(waterNodePrefab, position, Quaternion.identity).GetComponent<WaterSurfaceNode>();
        if (prevNode != null) {
            node.prevSpring.connectedBody = prevNode.rb;
        }
        else {
            node.prevSpring.enabled = false;
        }
        node.selfSpring.connectedAnchor = node.transform.position;
        node.transform.parent = transform;
        node.zone = GetComponent<WaterZone>();
        node.detector = GetComponent<FishDetector>();
        return node;
    }

    public virtual void Garbulate() {
        ClearCurrentNodes();

        length = dimensions.x;
        height = dimensions.y;


        renderers.transform.SetGlobalScale(new Vector3(length, height * 2, 1f));

        int nodeCount = Mathf.RoundToInt(nodesPerUnit * length);

        float delta = 1f / nodeCount;
        var parent = transform.Find("Nodes");

        var topLeft = new Vector3(transform.position.x - length / 2, transform.position.y);
        var topRight = new Vector3(transform.position.x + length / 2, transform.position.y);

        WaterSurfaceNode previousNode = null;
        for (int i = 0; i <= nodeCount; i++) {
            Vector3 targetPosition = Vector3.Lerp(topLeft, topRight, i * delta);
            previousNode = SpawnNode(targetPosition, previousNode);
            previousNode.name = $"WaterSurfaceNode{i}";
            previousNode.transform.SetParent(parent);
        }
    }
}
