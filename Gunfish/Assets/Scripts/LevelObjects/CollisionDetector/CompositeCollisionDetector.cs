using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeCollisionDetector : MonoBehaviour {

    public List<SubCollisionDetector> detectors = new List<SubCollisionDetector>();
    public bool grabChildren; // SUS SUS SUS AMOGUS AMOGUS SUUUUUUUS!!!!!!!
    public bool generateSubCollisionDetectors;
    public bool oomphable;


    public CollisionEvent OnComponentCollideEnter, OnComponentCollideExit;
    public TriggerEvent OnComponentTriggerEnter, OnComponentTriggerExit;
    public bool DetectCollision = true, DetectTrigger = true;

    bool init;

    // Start is called before the first frame update
    void Start() {
        Init(this.grabChildren, this.generateSubCollisionDetectors, this.oomphable);
    }

    public void Init(bool grabChildren, bool generateSubCollisionDetectors, bool oomphable) {
        if (init)
            return;
        this.oomphable = oomphable;

        init = true;
        if (grabChildren) {
            if (generateSubCollisionDetectors) {
                foreach (var rb in GetComponentsInChildren<Rigidbody2D>()) {
                    var detector = rb.gameObject.AddComponent<SubCollisionDetector>();
                    detector.parentDetector = this;
                    detectors.Add(detector);
                    if (oomphable)
                        rb.gameObject.AddComponent<OomphCalculator>();
                }
            }
        }
        foreach (var subDetector in detectors) {
            // don't forget to like and subscribe
            subDetector.parentDetector = this;
            if (DetectCollision) {
                // for some reason it doesn't work to directly subscribe, I have to use these delegates!
                // NOTE(Wyatt): note that src is the gameobject that raised the event, not the target object
                subDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) { OnComponentCollideEnter?.Invoke(src, collision); };
                subDetector.OnComponentCollideExit += delegate (GameObject src, Collision2D collision) { OnComponentCollideExit?.Invoke(src, collision); };
            }
            if (DetectTrigger) {
                subDetector.OnComponentTriggerEnter += delegate (GameObject src, Collider2D collision) { OnComponentTriggerEnter?.Invoke(src, collision); };
                subDetector.OnComponentTriggerExit += delegate (GameObject src, Collider2D collision) { OnComponentTriggerExit?.Invoke(src, collision); };
                ;
            }
        }
    }
}