using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeCollisionDetector : MonoBehaviour
{

    public List<SubCollisionDetector> detectors = new List<SubCollisionDetector>();
    public bool grabChildren; // SUS SUS SUS AMOGUS AMOGUS SUUUUUUUS!!!!!!!
    public bool generateSubCollisionDetectors;
    public bool oomphable;


    public CollisionEvent OnComponentCollideEnter, OnComponentCollideExit;
    public TriggerEvent OnComponentTriggerEnter, OnComponentTriggerExit;
    public bool DetectCollision = true, DetectTrigger = true;

    bool init;

    // Start is called before the first frame update
    void Start()
    {
        Init(this.grabChildren, this.generateSubCollisionDetectors, this.oomphable);
    }

    public void Init(bool grabChildren, bool generateSubCollisionDetectors, bool oomphable) {
        if (init)
            return;

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
                subDetector.OnComponentCollideEnter += delegate (GameObject src, Collision2D collision) { OnComponentCollideEnter?.Invoke(src, collision); };
                subDetector.OnComponentCollideExit += OnComponentCollideExit;
            }
            if (DetectTrigger) {
                subDetector.OnComponentTriggerEnter += OnComponentTriggerEnter;
                subDetector.OnComponentTriggerExit += OnComponentTriggerExit;
            }
        }
    }
}
