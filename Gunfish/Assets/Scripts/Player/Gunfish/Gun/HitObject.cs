using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitObject {
    public Vector2 position;
    public Vector2 direction;

    public GameObject source;

    public float damage;
    public float knockback;

    public bool ignoreMass;
    // public List<Effect> effects;

    public HitObject(Vector2 position, Vector2 direction, GameObject source, float damage, float knockback, bool ignoreMass=false) {
        this.position = position;
        this.direction = direction;
        this.source = source;
        this.damage = damage;
        this.knockback = knockback;
        this.ignoreMass = ignoreMass;
    }
}

public class FishHitObject : HitObject {
    public int segmentIndex;
    public FishHitObject(int segmentIndex, Vector2 position, Vector2 direction, GameObject source, float damage, float knockback) : base(position, direction, source, damage, knockback, true) {
        this.segmentIndex = segmentIndex;
    }
}

public class CollisionHitObject : HitObject {
    public Collision2D collision;
    public CollisionHitObject(Collision2D collision, ContactPoint2D[] contacts, GameObject source, float oomph) : base(contacts[0].point, -contacts[0].normal, source, oomph, oomph) {
        this.collision = collision;
    }
}