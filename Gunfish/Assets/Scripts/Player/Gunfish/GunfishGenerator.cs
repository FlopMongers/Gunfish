using System.Collections.Generic;
using UnityEngine;

public class GunfishGenerator {
    private Gunfish gunfish;
    private List<GameObject> segments;
    private LineRenderer line;

    public GunfishGenerator(Gunfish gunfish) {
        this.gunfish = gunfish;
    }

    public List<GameObject> Generate(LayerMask layer, Vector3 position) {
        var data = gunfish.data;
        segments = new List<GameObject>(data.segmentCount);
        var segmentProps = ScriptableObject.CreateInstance<GunfishData>();

        segmentProps.physicsMaterial = data.physicsMaterial;
        segmentProps.length = data.length / data.segmentCount;
        segmentProps.segmentCount = 1;

        var totalArea = 0f;
        for (int i = 0; i < data.segmentCount; i++) {
            var radius = data.width.Evaluate((float)i / data.segmentCount) / 2f;
            var area = Mathf.PI * radius * radius;
            totalArea += area;
        }

        for (int i = 0; i < data.segmentCount; i++) {
            var segmentPos = position + new Vector3(i * segmentProps.length, 0f, 0f);
            var parent = i == 0 ? null : segments[i-1].transform;
            var diameter = data.width.Evaluate((float)i / data.segmentCount);
            var radius = diameter / 2f;
            var area = Mathf.PI * radius * radius;
            segmentProps.mass = area / totalArea * data.mass;
            segmentProps.width = AnimationCurve.Constant(0f, 1f, diameter);
            var node = InstantiateNode(i, segmentPos, segmentProps, layer, parent);
            segments.Add(node);
        }

        return segments;
    }

    private GameObject InstantiateNode(int index, Vector3 globalPosition, GunfishData data, LayerMask layer, Transform parent = null) {
        string name = parent == null ? $"Player{layer.value - 5}GunfishBody" : "Node";
        var obj = new GameObject(name);
        obj.transform.position = globalPosition;
        obj.transform.SetParent(parent);

        obj.layer = layer;

        GunfishSegment segment = obj.AddComponent<GunfishSegment>();
        segment.gunfish = gunfish;
        segment.index = index;


        var rb = obj.AddComponent<Rigidbody2D>();
        rb.mass = data.mass;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.radius = data.width.Evaluate(0f) / 2f;

        // The root fish piece will not have a parent, thus will not need a hinge joint since # hinge joints = # nodes - 1
        if (parent == null || parent.GetComponent<Rigidbody2D>() == null) return obj;

        var connectedBody = parent.GetComponent<Rigidbody2D>();

        var fixedJoint = obj.AddComponent<FixedJoint2D>();
        fixedJoint.connectedBody = connectedBody;
        fixedJoint.dampingRatio = data.fixedJointDamping;
        fixedJoint.frequency = data.fixedJointFrequency;

        var springJoint = obj.AddComponent<SpringJoint2D>();
        springJoint.connectedBody = connectedBody;
        springJoint.dampingRatio = data.springJointDamping;
        springJoint.frequency = data.springJointFrequency;

        return obj;
    }
}
