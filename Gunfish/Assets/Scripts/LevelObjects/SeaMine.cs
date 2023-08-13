using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Shootable))]
public class SeaMine : MonoBehaviour
{

    [SerializeField]
    float explodeDamage;

    [SerializeField]
    float explodeForce;

    [SerializeField]
    float explodeRadius;

    // the percent of total damage that will be dealt at radius edge
    [SerializeField]
    float explodeFalloff;

    // Start is called before the first frame update
    void Awake()
    {
        Shootable shootable = GetComponent<Shootable>();
        shootable.OnDead += Explode;
    }

    private void Update()
    {
        if (GameManager.debug && Input.GetKeyDown(KeyCode.End))
        {
            Shootable shootable = GetComponent<Shootable>();
            shootable.UpdateHealth(-1 * shootable.health);
        }
    }

    void Explode()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, explodeRadius, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.rigidbody != null)
                hit.rigidbody.AddExplosionForce(explodeForce, transform.position, explodeRadius);

            Shootable shootable = hit.transform.gameObject.GetComponent<Shootable>();

            if (shootable != null)
            {
                float damageReduction = (hit.distance / explodeRadius) * explodeFalloff;
                float damageAmount = explodeDamage * (1f - damageReduction);
                shootable.UpdateHealth(-1 * damageAmount);
            }

        }
    }

}
