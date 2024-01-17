using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

/*
if start jiggling, stick the fish in contact
if jiggling and a fish touches the hook, stick the fish and ZOOM
while touching, accelerate the jiggle timer
turn off the collision when zooming 
*/

public class FishHook : MonoBehaviour {
    public FishDetector detector;
    public Shaker shaker;
    public LineRenderer line;

    public Transform roofPosition;

    public Vector3 lineStartPosition, lineTargetPosition, detectorStartPosition, detectorTargetPosition;

    public float jiggle_timer;
    float jiggleDuration = 10f, jiggleThreshold = 4f, turboMode = 2f;
    float zoomDuration = 0.5f, returnDuration = 1f;
    float jointStrength = 500f;
    public bool zooming = false;

    Dictionary<Gunfish, FixedJoint2D> fishJointMap = new Dictionary<Gunfish, FixedJoint2D>();

    private void Start() {
        detector.OnFishTriggerEnter += OnFishEnter;
        detector.OnFishTriggerExit += OnFishExit;
        jiggle_timer = jiggleDuration;
        lineStartPosition = line.transform.position;
        lineTargetPosition = roofPosition.position;

        // the detector is offset from the line and so must be lerped independently between equivalent positions
        detectorStartPosition = detector.transform.position;
        detectorTargetPosition = roofPosition.position + (detector.transform.position - line.transform.position);
        
        line.SetPosition(1, line.transform.InverseTransformPoint(lineStartPosition));
        line.SetPosition(0, line.transform.InverseTransformPoint(roofPosition.position));
    }

    // Update is called once per frame
    void Update() {
        if (zooming)
            return;
        jiggle_timer = Mathf.Max(0, jiggle_timer - (Time.deltaTime * ((detector.fishes.Count > 0) ? turboMode : 1)));
        if (!shaker.shaking && jiggle_timer < jiggleThreshold) {
            StartJiggle();
        }
        else if (jiggle_timer <= 0) {
            jiggle_timer = jiggleDuration;
            if (detector.fishes.Count > 0)
                Zoom();
        }
        foreach (var fish in detector.fishes.Keys) {
            fish.MiddleSegment.GetComponent<GunfishSegment>().rb.velocity = Vector2.zero;
        }
    }

    private void LateUpdate() {

        foreach (var fish in detector.fishes.Keys) {
            fish.MiddleSegment.GetComponent<GunfishSegment>().rb.velocity = Vector2.zero;
        }
    }

    void Zoom() {
        zooming = true;
        foreach (var fishPair in fishJointMap) {
            if (fishJointMap.Values != null)
                fishPair.Value.breakForce = float.MaxValue;
        }
        StartCoroutine(CoZoom());
    }

    IEnumerator CoZoom() {
        // TODO lerp according to an animation curve
        float zoomTimer = zoomDuration;
        float percentage;
        List<Gunfish> doomedFishes = new List<Gunfish>();
        // freeze the fish and attach them
        foreach (var fish in detector.fishes.Keys) {
            fish.AddEffect(new NoMove_Effect(fish));
            doomedFishes.Add(fish);
            foreach (var segment in fish.segments) {
                segment.GetComponent<GunfishSegment>().rb.isKinematic = true;
            }
            fish.segments[0].transform.parent = detector.transform;
        }
        detector.SetCollidersEnabled(false);
        while (zoomTimer > 0) {
            percentage = 1 - (zoomTimer / zoomDuration);
            line.SetPosition(1, line.transform.InverseTransformPoint(Vector3.Lerp(lineStartPosition, lineTargetPosition, percentage)));
            detector.transform.position = Vector3.Lerp(detectorStartPosition, detectorTargetPosition, percentage);
            zoomTimer -= Time.deltaTime;
            yield return null;
        }
        foreach (var fish in doomedFishes) {
            if (fish == null || fish.RootSegment == null)
                continue;
            fish.Hit(
                new FishHitObject(0, 
                fish.RootSegment.transform.position, 
                Vector2.zero, 
                gameObject, 
                fish.statusData.health, 
                0, HitType.Impact));
        }
            //fish.Kill();
        // kill the fuckers
        zoomTimer = returnDuration;
        while (zoomTimer > 0) {
            percentage = 1 - (zoomTimer / returnDuration);
            line.SetPosition(1, line.transform.InverseTransformPoint(Vector3.Lerp(lineTargetPosition, lineStartPosition, percentage)));
            detector.transform.position = Vector3.Lerp(detectorTargetPosition, detectorStartPosition, percentage);
            zoomTimer -= Time.deltaTime;
            yield return null;
        }
        zooming = false;
        detector.SetCollidersEnabled(true);
    }

    void StartJiggle() {
        shaker.Activate(jiggleThreshold);
    }

    void OnFishEnter(GunfishSegment segment, Collider2D collider) {
        //if (shaker.shaking)
        if (!shaker.shaking)
            jiggle_timer = jiggleThreshold;
        StickFish(segment);
    }

    void OnFishExit(GunfishSegment segment, Collider2D collider) {
        if (fishJointMap.ContainsKey(segment.gunfish)) {
            if (fishJointMap[segment.gunfish] != null) {
                Destroy(fishJointMap[segment.gunfish]);
            }
            fishJointMap.Remove(segment.gunfish);
        }
    }

    void StickFish(GunfishSegment segment) {
        if (fishJointMap.ContainsKey(segment.gunfish))
            return;

        segment.gunfish.MiddleSegment.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        // add component and add to map
        var joint = gameObject.AddComponent<FixedJoint2D>();
        fishJointMap[segment.gunfish] = joint;
        joint.connectedBody = segment.rb;
        joint.breakForce = jointStrength;
    }
}