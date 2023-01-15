using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishGenerator : MonoBehaviour
{
    public GunfishData props;
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

        if (Input.GetKeyDown(KeyCode.R)){
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        if (Input.GetMouseButton(0)) {
            var targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var targetSegment = segments[segments.Count / 2];
            targetSegment.GetComponent<Rigidbody2D>().AddForce(1 * (targetPos - targetSegment.transform.position));
        }
    }

    private void Generate(GunfishData props) {
        if (props.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {props.segmentCount}. Must be greater than 3.");
        }
        segments = new List<GameObject>(props.segmentCount);
        var segmentProps = new GunfishData();

        segmentProps.physicsMaterial = props.physicsMaterial;
        segmentProps.length = props.length / props.segmentCount;
        segmentProps.maxBend = props.maxBend / props.segmentCount;
        segmentProps.segmentCount = 1;

        var totalArea = 0f;
        for (int i = 0; i < props.segmentCount; i++)
        {
            var radius = props.width.Evaluate((float)i / props.segmentCount) / 2f;
            var area = Mathf.PI * radius * radius;
            totalArea += area;
        }

        for (int i = 0; i < props.segmentCount; i++)
        {
            var position = new Vector3(i * segmentProps.length, 0f, 0f);
            var parent = i == 0 ? null : segments[i-1].transform;
            var diameter = props.width.Evaluate((float)i / props.segmentCount);
            var radius = diameter / 2f;
            var area = Mathf.PI * radius * radius;
            segmentProps.mass = area / totalArea * props.mass;
            segmentProps.width = AnimationCurve.Constant(0f, 1f, diameter);
            var node = InstantiateNode(position, segmentProps, parent);
            segments.Add(node);
        }

        line = segments[0].AddComponent<LineRenderer>();
        line.positionCount = props.segmentCount;
        line.material = props.spriteMat;
    }

    private GameObject InstantiateNode(Vector3 globalPosition, GunfishData props, Transform parent = null) {
        string name = parent == null ? "Gunfish" : "Node";
        var obj = new GameObject(name);
        obj.transform.position = globalPosition;
        obj.transform.SetParent(parent);

        obj.layer = LayerMask.NameToLayer("Player1");

        var rb = obj.AddComponent<Rigidbody2D>();
        rb.mass = props.mass;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.radius = props.width.Evaluate(0f) / 2f;

        // The root fish piece will not have a parent, thus will not need a hinge joint since # hinge joints = # nodes - 1
        if (parent == null || parent.GetComponent<Rigidbody2D>() == null) return obj;

        var connectedBody = parent.GetComponent<Rigidbody2D>();

        var fixedJoint = obj.AddComponent<FixedJoint2D>();
        fixedJoint.connectedBody = connectedBody;
        fixedJoint.dampingRatio = props.fixedJointDamping;
        fixedJoint.frequency = props.fixedJointFrequency;

        var springJoint = obj.AddComponent<SpringJoint2D>();
        springJoint.connectedBody = connectedBody;
        springJoint.dampingRatio = props.springJointDamping;
        springJoint.frequency = props.springJointFrequency;

        return obj;
    }
}
