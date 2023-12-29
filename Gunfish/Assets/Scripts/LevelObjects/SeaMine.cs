using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Shootable))]
public class SeaMine : MonoBehaviour {
    // the amount of force it takes to make the mine explode
    [SerializeField]
    float explodeForceThreshold;

    [SerializeField]
    float explodeDamage;

    [SerializeField]
    float explodeForce;

    [SerializeField]
    float explodeRadius;

    // the percent of total damage that will be dealt at radius edge
    [SerializeField]
    float explodeFalloff;

    /// <summary>
    /// The probability that any given mine in range will be affected by this explosion, starting a chain reaction.
    /// </summary>
    [SerializeField]
    float chainingProbability;

    Shootable shootable;

    // Start is called before the first frame update
    void Awake() {
        shootable = GetComponent<Shootable>();
        shootable.OnDead += Explode;
    }

    private void Update() {
        if (GameManager.debug && Input.GetKeyDown(KeyCode.End)) {
            Shootable shootable = GetComponent<Shootable>();
            shootable.UpdateHealth(-1 * shootable.health);
        }
    }

    void Explode() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, explodeRadius, Vector2.zero);

        foreach (RaycastHit2D hit in hits) {
            if (hit.transform.gameObject.GetComponent<SeaMine>() == null || Random.value < chainingProbability) {
                if (hit.rigidbody != null)
                    hit.rigidbody.AddExplosionForce(explodeForce, transform.position, explodeRadius);

                Shootable shootable = hit.transform.gameObject.GetComponent<Shootable>();

                if (shootable != null) {
                    float damageReduction = (hit.distance / explodeRadius) * explodeFalloff;
                    float damageAmount = explodeDamage * (1f - damageReduction);
                    shootable.UpdateHealth(-1 * damageAmount);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        float collisionForce = collision.relativeVelocity.magnitude;

        if (collisionForce > explodeForceThreshold) {
            shootable.UpdateHealth(-10_000_000f);
        }
    }

}