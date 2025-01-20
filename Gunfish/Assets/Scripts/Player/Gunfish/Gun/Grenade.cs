using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public Shootable shootable;
    public GameObject explosion;

    public Gunfish sourceGunfish;

    public float duration = 2f;

    [HideInInspector]
    public float damageScale = -1f;
    [HideInInspector]
    public float fishDamageScale = -1f;


    private void Start() {
        shootable.OnDead += Explode;
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration < 0) {
            Explode();
        }
    }

    public void Explode() {
        // spawn the explosion
        var exp = Instantiate(explosion, transform.position, Quaternion.identity).GetComponent<Explosion>();
        exp.sourceGunfish = sourceGunfish;
        if (fishDamageScale < 0) {
            exp.fishDamageScale = fishDamageScale;
        }
        if (damageScale < 0) {
            exp.damageScale = damageScale;
        }
        //exp.Explode();
        // destroy grenade
        Destroy(gameObject);
    }
}
