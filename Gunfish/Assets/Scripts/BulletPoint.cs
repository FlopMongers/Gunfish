using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoint : MonoBehaviour
{

    public LineRenderer bulletStreak;
    public ParticleSystem muzzleFlash;
    public AudioSource gunshotAudio;
    public Gun gun;

    public ContactFilter2D contactFilter;

    // Start is called before the first frame update
    void Start()
    {
        gun = GetComponentInParent<Gun>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire() {
        Debug.Log("Bang!");
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.right, gun.gunInfo.distance);
        if (hit) {
            if (hit.collider.GetComponent<IHittable>() != null) {
                hit.collider.GetComponent<IHittable>().Hit(-hit.normal, gun.gunInfo);
            }
            /*
            if (hit.collider.GetComponent<Destructable>()) {
                hit.collider.GetComponent<Destructable>().TakeDamage(gun.gunInfo.damage);
            }
            */
        }
    }
}
