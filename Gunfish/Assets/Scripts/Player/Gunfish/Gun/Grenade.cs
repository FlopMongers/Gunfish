using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject explosion;

    public Gunfish sourceGunfish;

    public float duration = 2f;

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration < 0) {
            // spawn the explosion
            var exp = Instantiate(explosion, transform.position, Quaternion.identity).GetComponent<Explosion>();
            exp.sourceGunfish = sourceGunfish;
            //exp.Explode();
            // destroy grenade
            Destroy(gameObject);
        }
    }
}
