using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Shootable))]
public class SeaMine : MonoBehaviour {
    // the amount of force it takes to make the mine explode
    [SerializeField]
    float explodeForceThreshold;

    Shootable shootable;

    public GameObject explosion;

    // Start is called before the first frame update
    void Awake() {
        shootable = GetComponent<Shootable>();
        shootable.OnDead += OnDead;
    }

    void OnDead() {
        // if health <= 0, EXPLODE
        if (explosion == null)
            return;
        // spawn explosion
        Instantiate(explosion, transform.position, Quaternion.identity);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        float collisionForce = collision.relativeVelocity.magnitude;

        if (collisionForce > explodeForceThreshold) {
            shootable.UpdateHealth(-10_000_000f);
        }
    }
}