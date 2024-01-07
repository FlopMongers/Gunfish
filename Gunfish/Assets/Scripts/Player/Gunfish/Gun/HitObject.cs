using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitObject {
    public Vector2 position;
    public Vector2 direction;

    public GameObject source;

    public float damage;
    public float knockback;

    public ForceMode2D force = ForceMode2D.Force;
    // public List<Effect> effects;

    public HitObject(Vector2 position, Vector2 direction, GameObject source, float damage, float knockback, ForceMode2D force=ForceMode2D.Force) {
        this.position = position;
        this.direction = direction;
        this.source = source;
        this.damage = damage;
        this.knockback = knockback;
        this.force = force;
    }
}

public class FishHitObject : HitObject {
    public int segmentIndex;
    public FishHitObject(int segmentIndex, Vector2 position, Vector2 direction, GameObject source, float damage, float knockback, ForceMode2D force=ForceMode2D.Force) : base(position, direction, source, damage, knockback, force) {
        this.segmentIndex = segmentIndex;
    }
}

public class CollisionHitObject : HitObject {
    public Collision2D collision;
    public CollisionHitObject(Collision2D collision, ContactPoint2D[] contacts, GameObject source, float oomph) : base(contacts[0].point, -contacts[0].normal, source, oomph, oomph) {
        this.collision = collision;
    }
}