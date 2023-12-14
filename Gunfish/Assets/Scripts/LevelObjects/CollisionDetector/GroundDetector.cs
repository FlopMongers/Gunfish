using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GroundDetector : MonoBehaviour
{
    CollisionTracker collisionTracker = new CollisionTracker();

    [HideInInspector]
    public int groundMask;

    // Start is called before the first frame update
    void Start()
    {
        var compositeCollisionDetector = GetComponent<CompositeCollisionDetector>();
        compositeCollisionDetector.OnComponentCollideEnter += HandleCollisionEnter;
        compositeCollisionDetector.OnComponentCollideExit += HandleCollisionExit;
    }

    void HandleCollisionEnter(GameObject src, Collision2D collision) {
        if (groundMask == (groundMask | (1 << collision.transform.gameObject.layer)))
            collisionTracker.AddTarget(src, collision);
    }

    void HandleCollisionExit(GameObject src, Collision2D collision) {
        if (!collisionTracker.tracker.ContainsKey(src))
            return;

        collisionTracker.tracker[src].numCollisions--;
        if (collisionTracker.tracker[src].numCollisions == 0)
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
