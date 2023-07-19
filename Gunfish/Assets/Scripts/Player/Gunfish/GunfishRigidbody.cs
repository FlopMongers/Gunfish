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
    public List<FishSegment> segments;
    private int groundMask;
    public bool underwater;
    public GroundMaterial currentGroundMaterial;

    public bool Grounded {
        get {
            foreach (var segment in segments) {
                var collider = Physics2D.Raycast(segment.body.position, Vector2.down, segment.collider.radius * 1.1f, groundMask);
                if (collider) return true;
            }
            return false;
        }
    }

    // set underwater
    public void SetUnderwater(bool underwater)
    {
        if (underwater != this.underwater) {
            foreach (var segment in segments)
            {
                segment.body.gravityScale = (underwater) ? 0f : 1f;
                segment.body.drag += (underwater) ? 1f: -1f;
            }
            this.underwater = underwater;
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

    public void ApplyForceToSegment(int index, Vector2 force, ForceMode2D forceMode=ForceMode2D.Force) {
        segments[index].body.AddForce(force, forceMode);
    }

    public void ApplyTorqueToSegment(int index, float torque, ForceMode2D forceMode=ForceMode2D.Force) {
        segments[index].body.AddTorque(torque, forceMode);
    }
}
