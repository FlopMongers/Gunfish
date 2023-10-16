using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDamageReceiver : MonoBehaviour {

    // hook up to either shootable or gunfish
    // I know this is an anti-pattern.
    // I'm going to re-work damage and health in the normal, not-dumb way eventually.
    // or maybe I won't and we'll suffer this idiocy even after the sweet release of dea- the game
    [HideInInspector] public Gunfish gunfish;
    [HideInInspector] public Shootable shootable;
    private void Start() {
        shootable = GetComponent<Shootable>();
    }

    public void Damage(CollisionHitObject hitObject) {
        gunfish?.Hit(new FishHitObject(hitObject.collision.collider.GetComponent<GunfishSegment>().index, hitObject.position, hitObject.direction, hitObject.source, hitObject.damage, hitObject.knockback));
        shootable?.Hit(hitObject);
    }
}