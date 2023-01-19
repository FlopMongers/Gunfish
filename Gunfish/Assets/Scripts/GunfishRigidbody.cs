using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishRigidbody {
    private List<GameObject> segments;
    private List<Rigidbody2D> bodies;
    public GunfishRigidbody(List<GameObject> segments) {
        this.segments = segments;
        bodies = new List<Rigidbody2D>(this.segments.Count);
        segments.ForEach(segment => bodies.Add(segment.GetComponent<Rigidbody2D>()));
    }

    public void ApplyForceToSegment(int index, Vector2 force) {
        bodies[index].AddForce(force);
    }
}
