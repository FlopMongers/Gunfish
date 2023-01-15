using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GunfishProps
{
    public Material spriteMat;
    public PhysicsMaterial2D physicsMaterial;
    [Range(1f, 100f)] public float mass;
    [Range(20f, 30f)] public float maxBend;
    [Range(0.5f, 5f)] public float length;
    [Range(3, 33)] public int segmentCount;
}

public class GunfishGenerator : MonoBehaviour
{
    public Sprite debugNodeSprite;
    public GunfishProps props;
    private List<GameObject> segments;
    private LineRenderer line;

    // Start is called before the first frame update
    private void Start()
    {
        Generate(props);
    }

    private void Update()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (!segment.transform.hasChanged) continue; //No need to reassign if it hasn't moved
            line.SetPosition(i, segment.transform.position);
        }
    }

    private void Generate(GunfishProps props) {
        if (props.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {props.segmentCount}. Must be greater than 3.");
        }
        segments = new List<GameObject>(props.segmentCount);
        var segmentProps = new GunfishProps();

        segmentProps.physicsMaterial = props.physicsMaterial;
        segmentProps.mass = props.mass / props.segmentCount;
        segmentProps.length = props.length / props.segmentCount;
        segmentProps.maxBend = props.maxBend / props.segmentCount;
        segmentProps.segmentCount = 1;

        for (int i = 0; i < props.segmentCount; i++)
        {
            var position = new Vector3(i * segmentProps.length, 0f, 0f);
            var parent = i == 0 ? null : segments[i-1].transform;
            var node = InstantiateNode(position, segmentProps, parent);
            segments.Add(node);
        }

        line = segments[0].AddComponent<LineRenderer>();
        line.positionCount = props.segmentCount;
        line.material = props.spriteMat;
    }

    private GameObject InstantiateNode(Vector3 globalPosition, GunfishProps props, Transform parent = null) {
        string name = parent == null ? "Node" : $"Node{parent.childCount}";
        var obj = new GameObject(name);
        obj.transform.position = globalPosition;
        obj.transform.SetParent(parent);

        obj.layer = LayerMask.NameToLayer("Player1");

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = debugNodeSprite;

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.mass = props.mass;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var collider = obj.AddComponent<CircleCollider2D>();

        // The root fish piece will not have a parent, thus will not need a hinge joint since # hinge joints = # nodes - 1
        if (parent == null || parent.GetComponent<Rigidbody2D>() == null) return obj;

        var joint = obj.AddComponent<HingeJoint2D>();
        joint.connectedBody = parent.GetComponent<Rigidbody2D>();
        var halfAngle = props.maxBend / 2f;
        // joint.useLimits = true;
        var limits = new JointAngleLimits2D();
        limits.min = -halfAngle;
        limits.max = halfAngle;
        joint.limits = limits;

        var joint2 = obj.AddComponent<HingeSpring2D>();
        joint2.minAngle = 0f;
        joint2.force = 128f;

        return obj;
    }
}
