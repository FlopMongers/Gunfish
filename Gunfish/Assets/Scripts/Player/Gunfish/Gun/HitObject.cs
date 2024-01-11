using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HitType { Impact, Ballistic, Electric, Explosive, Acid }

public class HitObject {
    public Vector2 position;
    public Vector2 direction;

    public GameObject source;

    public float damage;
    public float knockback;

    public bool ignoreMass;
    public bool ignoreFX;
    // public List<Effect> effects;
    public HitType hitType;

    public HitObject(Vector2 position, Vector2 direction, GameObject source, float damage, float knockback, HitType hitType, bool ignoreMass=false, bool ignoreFX=false) {
        this.position = position;
        this.direction = direction;
        this.source = source;
        this.damage = damage;
        this.knockback = knockback;
        this.ignoreMass = ignoreMass;
        this.ignoreFX = ignoreFX;
        this.hitType = hitType;
    }
}

public class FishHitObject : HitObject {
    public int segmentIndex;
    public FishHitObject(int segmentIndex, Vector2 position, Vector2 direction, GameObject source, float damage, float knockback, HitType hitType, bool ignoreFX=false) : base(position, direction, source, damage, knockback, hitType, true, ignoreFX) {
        this.segmentIndex = segmentIndex;
    }
}

public class CollisionHitObject : HitObject {
    public Collision2D collision;
    public CollisionHitObject(Collision2D collision, ContactPoint2D[] contacts, GameObject source, float oomph) : base(contacts[0].point, -contacts[0].normal, source, oomph, oomph, HitType.Impact) {
        this.collision = collision;
    }
}