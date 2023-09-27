using Cinemachine;
using SolidUtilities.UnityEngineInternals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionEventTracker {
    public float timestamp;
    public float oomph;
    public Collision2D collision;
    public ContactPoint2D[] contacts;
}

public class CollisionDamageHandler : MonoBehaviour
{

    public Dictionary<GameObject, CollisionEventTracker> collisionTracker = new Dictionary<GameObject, CollisionEventTracker>();
    int checkCollisions;
    float oomphThreshold;

    static float collisionTimeThreshold = 0.1f;

    public CompositeCollisionDetector collisionDetector;

    private void Start() {
        if (collisionDetector == null) {
            collisionDetector = GetComponent<CompositeCollisionDetector>();
        }
        collisionDetector.OnComponentCollideEnter += HandleCollisionEnter;
    }


    private void Update() {
        // iterate over oomphs
        // if sufficient time has passed, pass to the relevant 
        if (checkCollisions <= 0) {
            checkCollisions = 0;
            return;
        }

        List<GameObject> removeList = new List<GameObject>();
        foreach (var target in collisionTracker) {
            if ((Time.time - target.Value.timestamp) > collisionTimeThreshold) {
                removeList.Add(target.Key);
                checkCollisions--;
                // apply damage to the target
                target.Key.GetComponent<CollisionDamageReceiver>().Damage(new CollisionHitObject(target.Value.collision, target.Value.contacts, gameObject, target.Value.oomph));
            }
        }

        foreach (var target in removeList) {
            collisionTracker.Remove(target);
        }
    }

    void HandleCollisionEnter(GameObject src, Collision2D collision) {
        var target = collision.collider.gameObject;
        var subDetector = collision.collider.GetComponent<SubCollisionDetector>();
        if (subDetector != null) {
            target = subDetector.parentDetector.gameObject;
        }

        float oomph = src.GetComponent<OomphCalculator>().Oomph(collision);

        // if not enough oomph, just return
        if (oomph <= oomphThreshold)
            return;

        if (collisionTracker.ContainsKey(target) == false) {
            collisionTracker[target] = new CollisionEventTracker();
            collisionTracker[target].timestamp = Time.time;
        }

        if (Time.time - collisionTracker[target].timestamp < collisionTimeThreshold) {
            if (oomph > collisionTracker[target].oomph) {
                collisionTracker[target].collision = collision.ShallowCopy();
                collisionTracker[target].contacts = collision.contacts;
                collisionTracker[target].oomph = oomph;
                collisionTracker[target].timestamp = Time.time;
            }
            checkCollisions++;
        }
    }
}
