using Cinemachine;
using SolidUtilities.UnityEngineInternals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionData {
    public float timestamp;
    public float oomph;
    public Collision2D collision;
    public ContactPoint2D[] contacts;
    public int numCollisions=0;
}

public class CollisionTracker {
    public Dictionary<GameObject, CollisionData> tracker = new Dictionary<GameObject, CollisionData>();

    void AddNewTarget(GameObject target) {
        if (tracker.ContainsKey(target) == false) {
            tracker[target] = new CollisionData();
            tracker[target].timestamp = Time.time;
        }
    }

    void AddCollisionData(GameObject target, Collision2D collision, float oomph) {
        tracker[target].collision = collision.ShallowCopy();
        tracker[target].contacts = collision.contacts;
        tracker[target].oomph = oomph;
        tracker[target].timestamp = Time.time;
        tracker[target].numCollisions++;
    }

    public void AddTarget(GameObject target, Collision2D collision, float oomph = 0) {
        AddNewTarget(target);
        AddCollisionData(target, collision, oomph);
    }

    public bool AddTarget(GameObject target, Collision2D collision, float oomph, float collisionTimeThreshold, float oomphThreshold) {
        AddNewTarget(target);
        if (Time.time - tracker[target].timestamp < collisionTimeThreshold) {
            if (oomph > tracker[target].oomph) {
                AddCollisionData(target, collision, oomph);
                return true;
            }
        }
        return false;
    }
}

public class CollisionDamageDealer : MonoBehaviour {

    public CollisionTracker collisionTracker = new CollisionTracker();
    int checkCollisions;
    float oomphThreshold;

    public bool dealDamage = true;

    static float collisionTimeThreshold = 0.1f;

    public CompositeCollisionDetector collisionDetector;

    public float damageMultiplier = 1;
    public float impulseThreshold = 4f;

    protected bool trace = false;

    protected virtual void Start() {
        if (collisionDetector == null) {
            collisionDetector = GetComponent<CompositeCollisionDetector>();
        }
        if (collisionDetector != null) {
            collisionDetector.OnComponentCollideEnter += HandleCollisionEnter;
        }
    }

    public void SetupCollisionDetector(CompositeCollisionDetector collisionDetector) {
        this.collisionDetector = collisionDetector;
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
        foreach (var target in collisionTracker.tracker) {
            if ((Time.time - target.Value.timestamp) > collisionTimeThreshold || target.Key == null) {
                removeList.Add(target.Key);
                checkCollisions--;
                // apply damage to the target
                if (target.Key != null) { 
                    target.Key.GetComponent<CollisionDamageReceiver>().Damage(new CollisionHitObject(target.Value.collision, target.Value.contacts, gameObject, target.Value.oomph * damageMultiplier));
                }
            }
        }

        foreach (var target in removeList) {
            collisionTracker.tracker.Remove(target);
        }
    }

    protected virtual void HandleCollisionEnter(GameObject src, Collision2D collision) {
        var target = collision.collider.gameObject;
        var subDetector = collision.collider.GetComponent<SubCollisionDetector>();
        if (subDetector != null) {
            target = subDetector.parentDetector.gameObject;
        }
        else {
            target = collision.collider.GetComponentInParent<CompositeCollisionDetector>()?.gameObject ?? target;
        }

        float oomph = src.GetComponent<OomphCalculator>().Oomph(collision, impulseThreshold);
        if (trace)
            print($"oomph {oomph} from {src} for target {target}");

        // if not enough oomph, just return
        if (oomph <= oomphThreshold)
            return;

        if (collisionTracker.AddTarget(target, collision, oomph, collisionTimeThreshold, oomphThreshold))
            checkCollisions++;
    }
}