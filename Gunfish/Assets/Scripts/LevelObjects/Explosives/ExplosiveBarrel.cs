using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Shootable))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Destroyer))]
public class ExplosiveBarrel : MonoBehaviour
{
    public Shootable shootable;
    public GameObject explosion;

    public HitCounter hitCounter;

    // Start is called before the first frame update
    void Start()
    {
        shootable = shootable ?? GetComponent<Shootable>();
        shootable.OnHit += OnHit;
        shootable.OnDead += OnDead;
        hitCounter = hitCounter ?? GetComponent<HitCounter>();
    }

    void OnHit(HitObject hit) {
        // if ballistic, max damage
        if (hit.hitType == HitType.Ballistic || hit.hitType == HitType.Explosive) {
            hit.damage = shootable.maxHealth;
        }
    }

    void OnDead() {
        // if health <= 0, EXPLODE
        if (explosion == null)
            return;
        // spawn explosion
        var explodey = Instantiate(explosion, transform.position, Quaternion.identity);
        if (hitCounter != null) {
            Explosion exp = explodey.GetComponent<Explosion>();
            exp.sourceGunfish = hitCounter.lastHitter;
            exp.ignoreSourceGunfish = true;
        }
    }
}
