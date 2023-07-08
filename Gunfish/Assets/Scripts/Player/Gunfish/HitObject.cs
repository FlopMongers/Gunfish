using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitObject
{
    public Vector2 position;
    public Vector2 direction;

    public GameObject source;

    public float damage;
    public float knockback;
    // public List<Effect> effects;

    public HitObject(Vector2 position, Vector2 direction, GameObject source, float damage, float knockback)
    {
        this.position = position;
        this.direction = direction;
        this.source = source;
        this.damage = damage;
        this.knockback = knockback;
    }
}

public class FishHitObject : HitObject
{
    public int segmentIndex;
    public FishHitObject(int segmentIndex, Vector2 position, Vector2 direction, GameObject source, float damage, float knockback) : base(position, direction, source, damage, knockback)
    {
        this.segmentIndex = segmentIndex;
    }
}
