using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    int layerMask;

    public Gunfish gunfish;

    public List<GunBarrel> barrels = new List<GunBarrel>();

    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("Player1", "Player2", "Player3", "Player4", "Ground", "Default");   
    }

    // Update is called once per frame
    void Update()
    {        
        DecrementTimers(Time.deltaTime);
    }

    void DecrementTimers(float delta)
    {
        if (gunfish == null || gunfish.statusData == null) return;
        gunfish.statusData.reloadTimer = Mathf.Max(0f, gunfish.statusData.reloadTimer - delta);
    }

    public void Fire()
    {
        if (!gunfish.statusData.CanFire) return;

        // reset fire timer
        gunfish.statusData.reloadTimer = gunfish.data.reloadTime;
        gunfish.Kickback(gunfish.data.gunKickback);
        Vector3 endPoint;

        foreach (GunBarrel barrel in barrels)
        {
            FX_Spawner.instance?.SpawnFX(FXType.Bang, barrel.transform.position, Quaternion.LookRotation(barrel.transform.forward, barrel.transform.up));
            RaycastHit2D[] hits = Physics2D.RaycastAll(barrel.transform.position, barrel.transform.right, gunfish.data.gunRange, layerMask);
            endPoint = barrel.transform.position + barrel.transform.right * gunfish.data.gunRange;

            foreach (var hit in hits) {

                GunfishSegment fishSegment = hit.transform.GetComponent<GunfishSegment>();
                if (fishSegment != null) {
                    bool fishHit = (MatchManager.instance != null) ? MatchManager.instance.ResolveHit(this, fishSegment) : fishSegment.gunfish != gunfish;
                    if (fishHit) // gameObject.GetComponent<GunfishSegment>()?.gunfish != gunfish)
                    {
                        fishSegment.gunfish.Hit(new FishHitObject(fishSegment.index, hit.point, -hit.normal, gameObject, gunfish.data.gunDamage, gunfish.data.gunKnockback));
                        endPoint = hit.point;
                        break;
                    }
                }
                else
                {
                    FX_Spawner.instance?.SpawnFX(FXType.Ground_Hit, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
                    endPoint = hit.point;
                    break;
                }
                // if gunfish, hit
                // if ground, spawn fx
            }
            barrel.Flash(endPoint);
        }
    }
}
