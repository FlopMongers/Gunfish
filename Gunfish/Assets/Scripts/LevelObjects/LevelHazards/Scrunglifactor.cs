using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class Scrunglifactor : MonoBehaviour
{
    public struct Scrunglifacts {
        public bool facted;
        public float originalColliderRadius;

        public float originalFixedDampening;
        public float originalFixedFrequency;

        public bool originalSpringAutoDistance;
        public float originalSpringDistance;
        public float originalSpringDampening;
        public float originalSpringFrequency;
    }

    Dictionary<GunfishSegment, Scrunglifacts> scrunglified = new();

    // add start method to initialize fish detector
    public void Start() {
        var detector = GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += HandleFishTriggerEnter;
        detector.OnFishTriggerExit += HandleFishTriggerExit;
    }

    protected void HandleFishTriggerEnter(GunfishSegment segment, Collider2D collider) {
        if (!scrunglified.ContainsKey(segment)) {
            Scrunglifacts facts = segment.Scrunglify();
            if (facts.facted) {
                scrunglified[segment] = facts;
            }
        }
    }

    protected void HandleFishTriggerExit(GunfishSegment segment, Collider2D collider) {
        if (scrunglified.ContainsKey(segment)) {
            Scrunglifacts facts = scrunglified[segment];
            segment.UnScrunglify(facts);
            scrunglified.Remove(segment);
        }
    }
}
