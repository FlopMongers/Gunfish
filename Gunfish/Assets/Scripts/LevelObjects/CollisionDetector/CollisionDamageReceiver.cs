using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDamageReceiver : MonoBehaviour, IHittable {

    // hook up to either shootable or gunfish
    // I know this is an anti-pattern.
    // I'm going to re-work damage and health in the normal, not-dumb way eventually.
    // or maybe I won't and we'll suffer this idiocy even after the sweet release of dea- the game
    [HideInInspector] public Gunfish gunfish;
    [HideInInspector] public Shootable shootable;

    public float oomphResistance = 1f;
    public float oomphScale = 1f;

    private void Start() {
        shootable = GetComponent<Shootable>();
    }

    public void Hit(CollisionHitObject hitObject) {
        hitObject.damage *= oomphScale;
        if (hitObject.damage <= oomphResistance) {
            return;
        }
        gunfish?.Hit(new FishHitObject(
            hitObject.collision.rigidbody.GetComponent<GunfishSegment>().index, 
            hitObject.position, 
            hitObject.direction, 
            hitObject.source, 
            hitObject.damage, 
            hitObject.knockback,
            HitType.Impact));
        shootable?.Hit(hitObject);
    }
}