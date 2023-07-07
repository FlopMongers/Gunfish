using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    int layerMask;

    public Gunfish gunfish;

    public List<Transform> barrels = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Gunfish", "Ground", "Default");   
    }

    // Update is called once per frame
    void Update()
    {
        DecrementTimers(Time.deltaTime);
    }

    void DecrementTimers(float delta)
    {
        // TODO: move to gun
        gunfish.statusData.reloadTimer = Mathf.Max(0f, gunfish.statusData.reloadTimer - delta);
    }

    public void Fire()
    {
        if (!gunfish.statusData.CanFire) return;

        // reset fire timer
        gunfish.statusData.reloadTimer = gunfish.data.reloadTime;
        gunfish.Kickback(gunfish.data.gunKickback);

        foreach (Transform barrel in barrels)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(barrel.position, barrel.right, gunfish.data.gunRange, layerMask);

            foreach (var hit in hits) { 

                if (hit.transform?.gameObject.GetComponent<GunfishSegment>()?.gunfish != gunfish)
                {
                    GunfishSegment segment = hit.transform.gameObject.GetComponent<GunfishSegment>();
                    segment.gunfish.Hit(segment.index);
                    break;
                }
                // if gunfish, hit
                // if ground, spawn fx
            }
        }

        /*
        RaycastHit2D hit;
        if (Physics2D.Raycast())
        {
            // if gunfish, hit
            // if ground, spawn fx
        }
        */

        // TODO: implement this
        // reload logic
        // perform kickback on gunfish
        // fx stuff
        // raycast
        // handle hit logic
    }
}
