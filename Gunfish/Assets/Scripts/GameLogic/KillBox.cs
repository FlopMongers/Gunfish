using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class KillBox : MonoBehaviour {
    // add start method to initialize fish detector
    public void Start() {
        var detector = GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += HandleFishTriggerEnter;
    }

    protected void HandleFishTriggerEnter(GunfishSegment segment, Collider2D collider) {
        segment.gunfish.Hit(new FishHitObject(
            segment.index,
            segment.transform.position,
            Vector2.up,
            gameObject,
            100000000000000000f,
            1f, HitType.Impact));
    }
}