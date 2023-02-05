using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FishSegment {
    public GameObject obj;
    public Rigidbody2D body;
    public CircleCollider2D collider;

    public FishSegment(GameObject obj, Rigidbody2D body, CircleCollider2D collider)
    {
        this.obj = obj;
        this.body = body;
        this.collider = collider;
    }
}

public class GunfishRigidbody {
    private List<FishSegment> segments;
    private int groundMask;

    public bool Grounded {
        get {
            foreach (var segment in segments) {
                var collider = Physics2D.Raycast(segment.body.position, Vector2.down, segment.collider.radius * 1.1f, groundMask);
                if (collider) return true;
            }
            return false;
        }
    }

    public GunfishRigidbody(List<GameObject> segments) {
        groundMask = LayerMask.GetMask("Ground");
        this.segments = new List<FishSegment>(segments.Count);
        segments.ForEach(segment => {
            this.segments.Add(
                new FishSegment(
                    segment,
                    segment.GetComponent<Rigidbody2D>(),
                    segment.GetComponent<CircleCollider2D>()
                )
            );
        });
    }

    public void ApplyForceToSegment(int index, Vector2 force) {
        segments[index].body.AddForce(force);
    }

    public void ApplyTorqueToSegment(int index, float torque) {
        segments[index].body.AddTorque(torque);
    }
}
