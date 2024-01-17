using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OomphCalculator : MonoBehaviour {
    public Rigidbody2D rb;

    [HideInInspector]
    public Vector2 lastVelocity;

    static float massScale = 1f, velocityScale = 1f;
    public float Momentum { get { return (rb.mass * massScale) * (lastVelocity.magnitude * velocityScale); } }

    float collisionAngleThreshold = 90f;

    const float defaultImpulseThreshold = 4f;



    // Start is called before the first frame update
    void Start() {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate() {
        lastVelocity = rb.velocity;
    }


    public bool OnTarget(Vector2 target) {
        //print($"Checking on target: {lastVelocity}, {target - (Vector2)transform.position}, {Vector3.Angle(lastVelocity, target - (Vector2)transform.position)}");
        return lastVelocity.magnitude > 0 && Vector3.Angle(lastVelocity, target - (Vector2)transform.position) <= collisionAngleThreshold;
    }

    public float Oomph(Collision2D collision, float oomphMultiplier = 1, float impulseThreshold=defaultImpulseThreshold) {

        var other = collision.rigidbody?.GetComponent<OomphCalculator>();

        if (other == null) {
            return 0;
        }

        float impulse = 0;
        int contactIndex = 0;
        for (int i = 0; i < collision.contacts.Length; i++) { 
            if (collision.contacts[i].normalImpulse > impulse) {
                impulse = collision.contacts[i].normalImpulse;
                contactIndex = i;
            }
        }
        //print($"Calulating impulse between {transform} and {other}, {impulse}");

        if (impulse  < impulseThreshold || !OnTarget(collision.contacts[contactIndex].point)) {
            return 0;
        }

        float oomph = Momentum;
        if (other.OnTarget(collision.contacts[contactIndex].point) && other.lastVelocity.magnitude > 0) {
            oomph = Momentum * Mathf.Min(1f, Momentum / (Mathf.Max(other.Momentum, other.rb.mass)));
        }
        else {
            oomph /= other.rb.mass;
        }
        return oomph * oomphMultiplier;
    }
}