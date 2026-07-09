using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class MovePoint {
    public Transform point;
    // how long to pause here before auto-advancing (Persistent mode only)
    public float timeAtPoint;

    public MovePoint(Transform point, float timeAtPoint) {
        this.point = point;
        this.timeAtPoint = timeAtPoint;
    }
}

public enum PlatformMode { Persistent, EventBased }
public enum LoopMode { Cycle, PingPong }

[RequireComponent(typeof(FishDetector))]
public class MovingPlatform : MonoBehaviour
{
    Dictionary<Rigidbody2D, int> rbMap = new Dictionary<Rigidbody2D, int>();
    Dictionary<Rigidbody2D, Transform> rbParentMap = new Dictionary<Rigidbody2D, Transform>();
    public Rigidbody2D rb;

    public PlatformMode mode = PlatformMode.Persistent;
    public LoopMode loopMode = LoopMode.Cycle;
    public List<MovePoint> movePoints = new List<MovePoint>();

    public float transitionSpeed = 1f;
    public bool normalizeSpeedByDistance = true;
    public AnimationCurve easingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public FishDetector detector;

    int index = 0;
    int direction = 1;
    Tween moveTween;
    Tween pauseTween;


    // Start is called before the first frame update
    void Start()
    {
        detector = detector ?? GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += delegate (GunfishSegment segment, Collider2D collision) { if (segment.gunfish != null && segment.gunfish.RootSegment != null) CarryObject(segment.gunfish.RootSegment.GetComponent<Rigidbody2D>()); };
        detector.OnFishTriggerExit += delegate (GunfishSegment segment, Collider2D collision) { if (segment.gunfish != null && segment.gunfish.RootSegment != null) ReleaseObject(segment.gunfish.RootSegment.GetComponent<Rigidbody2D>()); };
        rb = rb ?? GetComponent<Rigidbody2D>();
        if (movePoints.Count == 0) {
            movePoints.Add(new MovePoint(transform, 0f));
        }
        index = 0;
        direction = 1;
        rb.position = movePoints[0].point.position;
        if (mode == PlatformMode.Persistent && movePoints.Count > 1) {
            BeginLeg(GetNextIndex(index, movePoints.Count, loopMode, ref direction));
        }
    }

    void OnDestroy() {
        rb.DOKill();
        if (pauseTween != null && pauseTween.IsActive()) pauseTween.Kill();
    }

    static int GetNextIndex(int current, int count, LoopMode loopMode, ref int direction) {
        if (count <= 1) return current;
        if (loopMode == LoopMode.Cycle) return (current + 1) % count;
        int next = current + direction;
        if (next >= count) { direction = -1; next = current + direction; }
        else if (next < 0) { direction = 1; next = current + direction; }
        return next;
    }

    float ComputeLegDuration(Vector2 from, Vector2 to) {
        float speed = Mathf.Max(transitionSpeed, 0.0001f);
        return normalizeSpeedByDistance ? Vector2.Distance(from, to) / speed : 1f / speed;
    }

    void BeginLeg(int targetIndex) {
        index = targetIndex;
        Vector2 to = movePoints[index].point.position;
        float duration = ComputeLegDuration(rb.position, to);
        moveTween = rb.DOMove(to, duration).SetEase(easingCurve).OnComplete(OnLegComplete);
    }

    void OnLegComplete() {
        moveTween = null;
        if (mode != PlatformMode.Persistent) return;
        float pause = movePoints[index].timeAtPoint;
        if (pause > 0f) pauseTween = DOVirtual.DelayedCall(pause, ContinuePersistent);
        else ContinuePersistent();
    }

    void ContinuePersistent() {
        pauseTween = null;
        if (movePoints.Count <= 1) return;
        BeginLeg(GetNextIndex(index, movePoints.Count, loopMode, ref direction));
    }

    public void AdvanceToNextPoint() {
        if (movePoints.Count <= 1) return;
        if (moveTween != null && moveTween.IsActive() && moveTween.IsPlaying()) return;
        if (pauseTween != null && pauseTween.IsActive()) pauseTween.Kill();
        pauseTween = null;
        BeginLeg(GetNextIndex(index, movePoints.Count, loopMode, ref direction));
    }

    // Editor-only preview: evaluates where the platform would be at normalized time t (0-1)
    // across one full loop of the path (respecting loopMode/easingCurve/transitionSpeed).
    public Vector3? EvaluatePathPosition(float t) {
        if (movePoints == null || movePoints.Count == 0 || movePoints[0].point == null) return null;
        if (movePoints.Count == 1) return movePoints[0].point.position;

        List<int> sequence = BuildFullLoopSequence();
        if (sequence.Count < 2) return movePoints[sequence[0]].point.position;

        List<float> legDurations = new List<float>();
        float totalDuration = 0f;
        for (int i = 0; i < sequence.Count - 1; i++) {
            Transform a = movePoints[sequence[i]].point;
            Transform b = movePoints[sequence[i + 1]].point;
            float d = (a == null || b == null) ? 0f : ComputeLegDuration(a.position, b.position);
            legDurations.Add(d);
            totalDuration += d;
        }
        if (totalDuration <= 0f) return movePoints[sequence[0]].point.position;

        float targetTime = Mathf.Clamp01(t) * totalDuration;
        float cumulative = 0f;
        for (int i = 0; i < legDurations.Count; i++) {
            float legDuration = legDurations[i];
            bool isLastLeg = i == legDurations.Count - 1;
            if (targetTime <= cumulative + legDuration || isLastLeg) {
                Transform from = movePoints[sequence[i]].point;
                Transform to = movePoints[sequence[i + 1]].point;
                if (from == null || to == null) return null;
                float localT = legDuration > 0f ? Mathf.Clamp01((targetTime - cumulative) / legDuration) : 1f;
                float easedT = easingCurve != null && easingCurve.length > 0 ? easingCurve.Evaluate(localT) : localT;
                return Vector3.Lerp(from.position, to.position, easedT);
            }
            cumulative += legDuration;
        }
        return movePoints[sequence[sequence.Count - 1]].point.position;
    }

    List<int> BuildFullLoopSequence() {
        int count = movePoints.Count;
        List<int> sequence = new List<int> { 0 };
        int idx = 0;
        int dir = 1;
        int steps = loopMode == LoopMode.Cycle ? count : (count - 1) * 2;
        for (int i = 0; i < steps; i++) {
            idx = GetNextIndex(idx, count, loopMode, ref dir);
            sequence.Add(idx);
        }
        return sequence;
    }

    // get all objects with rigidbodies in zone
    // apply
    private void OnTriggerEnter2D(Collider2D collision) {
        var otherRB = collision.attachedRigidbody;
        if (otherRB == null || otherRB.GetComponentInParent<GunfishSegment>() != null || otherRB.gameObject.layer == LayerMask.GetMask("Pelican"))
            return;
        CarryObject(otherRB);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var otherRB = collision.attachedRigidbody;
        if (otherRB == null || otherRB.GetComponentInParent<GunfishSegment>() != null || otherRB.gameObject.layer == LayerMask.GetMask("Pelican"))
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
