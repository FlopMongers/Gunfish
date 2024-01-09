using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Shootable))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Destroyer))]
public class ExplosiveBarrel : MonoBehaviour
{
    public Shootable shootable;
    public GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        shootable = shootable ?? GetComponent<Shootable>();
        shootable.OnHit += OnHit;
        shootable.OnHealthUpdated += OnHealthUpdated;
    }

    void OnHit(HitObject hit) {
        // if ballistic, max damage
        if (hit.hitType == HitType.Ballistic || hit.hitType == HitType.Explosive) {
            hit.damage = shootable.maxHealth;
        }
    }

    void OnHealthUpdated(float health) {
        // if health <= 0, EXPLODE
        if (explosion == null)
            return;
        if (health <= 0) {
            // spawn explosion
            Instantiate(explosion, transform.position, Quaternion.identity);
        }
    }
}
