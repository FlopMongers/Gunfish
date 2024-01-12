using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishDetector : MonoBehaviour {
    public Dictionary<Gunfish, int> fishes = new Dictionary<Gunfish, int>();

    public FishCollisionEvent OnFishCollideEnter, OnFishCollideExit;
    public FishTriggerEvent OnFishTriggerEnter, OnFishTriggerExit, OnFirstSegmentTriggerExit;

    public bool DetectCollision = true, DetectTrigger = true;

    [HideInInspector]
    public List<Collider2D> colliders = new List<Collider2D>();

    private void Start() {
        colliders = new List<Collider2D>(gameObject.GetComponentsInChildren<Collider2D>());
    }

    public void SetCollidersEnabled(bool enable) {
        foreach (var collider in colliders)
            collider.enabled = enable;
    }

    public bool DetectFishEnter(GunfishSegment segment) {
        if (segment == null)
            return false;
        if (fishes.ContainsKey(segment.gunfish) == false) {
            fishes[segment.gunfish] = 0;
        }
        fishes[segment.gunfish] += 1;
        return fishes[segment.gunfish] == 1; // we'll see about this...
    }

    public bool DetectFishExit(GunfishSegment segment) {
        if (segment == null)
            return false;
        if (fishes.ContainsKey(segment.gunfish) == false) {
            return false;
        }
        fishes[segment.gunfish] -= 1;
        if (fishes[segment.gunfish] <= 0) {
            fishes.Remove(segment.gunfish);
            return true;
        }
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!DetectCollision)
            return;
        var segment = collision.collider.GetComponent<GunfishSegment>();
        if (DetectFishEnter(segment))
            OnFishCollideEnter?.Invoke(segment, collision);
    }

    private void OnCollisionExit2D(Collision2D collision) {
        if (!DetectCollision)
            return;
        var segment = collision.collider.GetComponent<GunfishSegment>();
        if (DetectFishExit(segment))
            OnFishCollideExit?.Invoke(segment, collision);
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        if (!DetectTrigger)
            return;
        var segment = collision.GetComponent<GunfishSegment>();
        if (DetectFishEnter(segment)) {
            OnFishTriggerEnter?.Invoke(segment, collision);
        }
    }

    public void OnTriggerExit2D(Collider2D collision) {
        if (!DetectTrigger)
            return;
        var segment = collision.GetComponent<GunfishSegment>();
        if (DetectFishExit(segment)) {
            OnFishTriggerExit?.Invoke(segment, collision);
        }
        else if (segment != null && fishes.ContainsKey(segment.gunfish)) {
            if (fishes[segment.gunfish] == segment.gunfish.segments.Count - 1) {
                OnFirstSegmentTriggerExit?.Invoke(segment, collision);
            }
        }
    }
}