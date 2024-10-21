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
                script.Garbulate();
            }
        }
    }
}
#endif

public class WaterSurfaceGenerator : MonoBehaviour {
    [SerializeField]
    [Range(0.25f, 4f)]
    private float nodesPerUnit;
    private float unitsPerNode;

    [SerializeField]
    private GameObject waterNodePrefab;

    [SerializeField]
    private GameObject renderers;

    [SerializeField]
    private GameObject nodesContainer;

    protected float length, height;

    public Vector2 dimensions = new Vector2(5f, 5f);

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

    public virtual void Garbulate() {
        length = dimensions.x;
        height = dimensions.y;
        int nodeCount = Mathf.RoundToInt(nodesPerUnit * length);

        if (nodeCount < 11) {
            throw new Exception($"Invalid nodesPerUnit {nodesPerUnit} for length {length}. Please increase the nodesPerUnit or length such that nodeCount >= 11. (Current nodeCount: {nodeCount})");
        }

        ClearCurrentNodes();

        renderers.transform.SetGlobalScale(new Vector3(length, height * 2, 1f));

        float delta = 1f / nodeCount;
        unitsPerNode = 1f / nodesPerUnit;
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
        if (previousNode) {
            // Colliders are to the right of their nodes, so remove the last trailing one.
            DestroyImmediate(previousNode.GetComponent<BoxCollider2D>());
        }
    }

    WaterSurfaceNode SpawnNode(Vector3 position, WaterSurfaceNode prevNode) {
        var node = Instantiate(waterNodePrefab, position, Quaternion.identity).GetComponent<WaterSurfaceNode>();
        if (prevNode != null) {
            node.prevSpring.connectedBody = prevNode.rb;
        } else {
            DestroyImmediate(node.prevSpring);
        }
        node.selfSpring.connectedAnchor = node.transform.position;
        node.transform.parent = transform;
        node.zone = GetComponent<WaterZone>();
        node.detector = GetComponent<FishDetector>();
        node.GetComponent<BoxCollider2D>().offset = new Vector2(unitsPerNode / 2f, 0);
        node.GetComponent<BoxCollider2D>().size = new Vector2(unitsPerNode, height * 2f);
        return node;
    }

    void SpawnDensityNode(WaterSurfaceNode prevNode, WaterSurfaceNode curNode) {
            
    }
}
