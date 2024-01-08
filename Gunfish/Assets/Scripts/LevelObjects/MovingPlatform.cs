using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovePoint {
    public Transform point;
    // how long should it take to get here?
    public float duration;

    public MovePoint(Transform point, float duration) {
        this.point = point;
        this.duration = duration;
    }
}

[RequireComponent(typeof(FishDetector))]
public class MovingPlatform : MonoBehaviour
{
    Dictionary<Rigidbody2D, int> rbMap = new Dictionary<Rigidbody2D, int>();
    Dictionary<Rigidbody2D, Transform> rbParentMap = new Dictionary<Rigidbody2D, Transform>();
    public Rigidbody2D rb;

    public List<MovePoint> movePoints = new List<MovePoint>();
    int index = 0;
    Transform nextPoint;
    float threshold = 0.1f;

    public FishDetector detector;


    // Start is called before the first frame update
    void Start()
    {
        detector = detector ?? GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += delegate (GunfishSegment segment, Collider2D collision) { CarryObject(segment.gunfish.RootSegment.GetComponent<Rigidbody2D>()); };
        detector.OnFishTriggerExit += delegate (GunfishSegment segment, Collider2D collision) { ReleaseObject(segment.gunfish.RootSegment.GetComponent<Rigidbody2D>()); };
        rb = rb ?? GetComponent<Rigidbody2D>();
        if (movePoints.Count == 0) {
            movePoints.Add(new MovePoint(transform, 0));
        }
        GetNextPoint();
    }

    private void Update() {
        if (Vector2.Distance(transform.position, nextPoint.position) < threshold) {
            GetNextPoint();
        }
    }

    void GetNextPoint() {
        index = (index + 1) % movePoints.Count;
        // get movePoint
        nextPoint = movePoints[index].point;
        // calculate by movePoint distance/duration
        Vector2 dir = (nextPoint.position - transform.position);
        rb.velocity = dir.magnitude / movePoints[index].duration * dir.normalized;
    }

    // get all objects with rigidbodies in zone
    // apply
    private void OnTriggerEnter2D(Collider2D collision) {
        var otherRB = collision.attachedRigidbody;
        if (otherRB == null || otherRB.GetComponentInParent<GunfishSegment>() != null)
            return;
        CarryObject(otherRB);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var otherRB = collision.attachedRigidbody;
        if (otherRB == null || otherRB.GetComponentInParent<GunfishSegment>() != null)
            return;
        ReleaseObject(otherRB);
    }

    void CarryObject(Rigidbody2D otherRB) {
        if (otherRB == null)
            return;
        if (rbMap.ContainsKey(otherRB) == false) {
            rbParentMap[otherRB] = otherRB.transform.parent;
            otherRB.transform.parent = transform;
            rbMap[otherRB] = 0;
        }
        rbMap[otherRB]++;
    }

    void ReleaseObject(Rigidbody2D otherRB) {
        if (otherRB == null)
            return;
        if (rbMap.ContainsKey(otherRB) == false)
            return;
        rbMap[otherRB]--;
        if (rbMap[otherRB] <= 0) {
            otherRB.transform.parent = rbParentMap[otherRB];
            rbParentMap.Remove(otherRB);
            rbMap.Remove(otherRB);
        }
    }
}
