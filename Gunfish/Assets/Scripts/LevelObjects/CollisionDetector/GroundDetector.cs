using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    CollisionTracker collisionTracker = new CollisionTracker();
    public Gunfish gunfish;

    [HideInInspector]
    public int groundMask;

    // Start is called before the first frame update
    void Start()
    {
        var compositeCollisionDetector = GetComponent<CompositeCollisionDetector>();
        compositeCollisionDetector.OnComponentCollideEnter += HandleCollisionEnter;
        compositeCollisionDetector.OnComponentCollideExit += HandleCollisionExit;
        
    }

    private void Update() {
        List<GameObject> toRemove = new List<GameObject>();
        HashSet<GameObject> hitObjs = new HashSet<GameObject>();
        RaycastHit2D[] hits = Physics2D.CircleCastAll(gunfish.MiddleSegment.transform.position, gunfish.data.length+1.5f, Vector2.zero, 1);
        foreach (var hit in hits) {
            if (hit.collider != null) {
                hitObjs.Add(hit.collider.gameObject);
            }
            if (hit.rigidbody != null) {
                hitObjs.Add(hit.rigidbody.gameObject);
            }
        }
        foreach (var pair in collisionTracker.tracker) {
            if (pair.Key == null || hitObjs.Contains(pair.Key) == false) {
                toRemove.Add(pair.Key);
            }
        }
        foreach (var removeObject in toRemove) {
            collisionTracker.tracker.Remove(removeObject);
        }
    }

    void HandleCollisionEnter(GameObject src, Collision2D collision) {
        if (groundMask == (groundMask | (1 << collision.transform.gameObject.layer)))
            collisionTracker.AddTarget(src, collision);
    }

    void HandleCollisionExit(GameObject src, Collision2D collision) {
        if (!collisionTracker.tracker.ContainsKey(src))
            return;

        collisionTracker.tracker[src].numCollisions--;
        if (collisionTracker.tracker[src].numCollisions <= 0)
            collisionTracker.tracker.Remove(src);
    }


    public bool IsGrounded() {
        // iterate over collisions and check if any of the collisions points is less than the src.transform.position
        foreach (var pair in collisionTracker.tracker) {
            foreach (var contact in pair.Value.contacts) { 
                if (contact.point.y < pair.Key.transform.position.y) return true;
            }
        }
        return false;
    }
}
