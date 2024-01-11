using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishSegment : ObjectMaterial {

    [HideInInspector]
    public Gunfish gunfish;

    [HideInInspector]
    public int index;

    [HideInInspector]
    public Rigidbody2D rb;

    public bool isGun;
    public int isUnderwater;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetUnderwater(int underwater) {
        // if gun and 

        bool change = (isUnderwater == 1 && underwater == -1) || (isUnderwater == 0 && underwater == 1);
        isUnderwater += underwater;
        if (change) {
            gunfish.anySegmentUnderwater += (isUnderwater > 0) ? 1 : -1;
            if (isGun) {
                if (isUnderwater <= 0) {
                    if (Vector3.Angle(Vector2.up, rb.velocity) < gunfish.data.waterZoomAngleThreshold && rb.velocity.magnitude > gunfish.data.waterZoomSpeedThreshold) {
                        gunfish.body.ApplyForceToSegment(index, ((rb.velocity.normalized + Vector2.up) / 2) * gunfish.data.waterZoomForce, ForceMode2D.Impulse);
                    }
                    // if upwards velocity is high enough, then LAUNCH the fish
                }
                // if gun, set the gunfish underwater
                gunfish.underwater = isUnderwater == 1;
            }
            rb.gravityScale = (isUnderwater == 1) ? 0f : 1f;
            rb.drag += (isUnderwater == 1) ? 1f : -1f;
        }
    }

    public Scrunglifactor.Scrunglifacts Scrunglify() {
        Scrunglifactor.Scrunglifacts facts = new() {
            facted = false
        };

        if (TryGetComponent<FixedJoint2D>(out var fixedJoint)
            && TryGetComponent<SpringJoint2D>(out var springJoint)
            && TryGetComponent<CircleCollider2D>(out var circleCollider)
        ) {
            // these will always be the same but whatever
            facts.facted = true;
            facts.originalColliderRadius = circleCollider.radius;

            facts.originalFixedDampening = fixedJoint.dampingRatio;
            facts.originalFixedFrequency = fixedJoint.frequency;

            facts.originalSpringAutoDistance = springJoint.autoConfigureDistance;
            facts.originalSpringDistance = springJoint.distance;
            facts.originalSpringDampening = springJoint.dampingRatio;
            facts.originalSpringFrequency = springJoint.frequency;

            circleCollider.radius = 0.01f;

            fixedJoint.dampingRatio = 0f;
            fixedJoint.frequency = 1;

            springJoint.autoConfigureDistance = false;
            springJoint.distance = 0.01f;
            springJoint.dampingRatio = 0f;
            springJoint.frequency = 1;
        }
        return facts;
    }

    public void UnScrunglify(Scrunglifactor.Scrunglifacts facts) {
        if (TryGetComponent<FixedJoint2D>(out var fixedJoint)
            && TryGetComponent<SpringJoint2D>(out var springJoint)
            && TryGetComponent<CircleCollider2D>(out var circleCollider)
        ) {
            circleCollider.radius = facts.originalColliderRadius;

            fixedJoint.dampingRatio = facts.originalFixedDampening;
            fixedJoint.frequency = facts.originalFixedFrequency;

            springJoint.autoConfigureDistance = facts.originalSpringAutoDistance;
            springJoint.distance = facts.originalSpringDistance;
            springJoint.dampingRatio = facts.originalSpringDampening;
            springJoint.frequency = facts.originalSpringFrequency;
        }
    }
}