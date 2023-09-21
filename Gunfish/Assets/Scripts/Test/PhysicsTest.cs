using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTest : MonoBehaviour
{
    Rigidbody2D rb;
    [HideInInspector]
    public Vector2 lastVelocity;

    static float massScale=1f, velocityScale=1f;
    public float Momentum { get { return (rb.mass * massScale) * (lastVelocity.magnitude * velocityScale); } }

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        lastVelocity = rb.velocity;
    }

    float impulseThreshold = 4f;
    float collisionAngleThreshold = 30f;

    public bool OnTarget(Vector2 target) {
        return lastVelocity.magnitude > 0 && Vector3.Angle(lastVelocity, target - (Vector2)transform.position) <= collisionAngleThreshold;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        print($"I am {gameObject.name}");
        /*print($"Normal Impulse: {collision.contacts[0].normalImpulse}");
        print($"Tangent Impulse {collision.contacts[0].tangentImpulse}");
        print($"Relative Velocity {collision.contacts[0].relativeVelocity}");
        print($"Self Velocity: {rb.velocity}");
        print($"Self Last Velocity: {lastVelocity}");
        print($"Other Velocity {collision.contacts[0].rigidbody.velocity}");*/

        var other = collision.gameObject.GetComponent<PhysicsTest>();

        if (other == null) {
            return;
        }

        float impulse = collision.contacts[0].normalImpulse;

        if (impulse < impulseThreshold || !OnTarget(collision.contacts[0].point)) {
            return;
        }

        float momentum = Momentum;
        if (other.OnTarget(collision.contacts[0].point) && other.lastVelocity.magnitude > 0) {
            momentum = Momentum * Mathf.Min(1f, Momentum / other.Momentum);
        }
        print($"OOMPH: {momentum}");
        
    }
}
