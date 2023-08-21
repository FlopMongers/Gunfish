using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Gun : MonoBehaviour {
    public Gunfish gunfish;
    public List<GunBarrel> barrels = new List<GunBarrel>();
    
    private int layerMask;

    // Start is called before the first frame update
    void Start() {
        layerMask = LayerMask.GetMask("Player1", "Player2", "Player3", "Player4", "Ground", "Default");   
    }

    // Update is called once per frame
    void Update() {
        DecrementTimers(Time.deltaTime);
    }

    void DecrementTimers(float delta)
    {
        if (gunfish == null || gunfish.statusData == null) return;
        gunfish.statusData.reloadTimer = Mathf.Max(0f, gunfish.statusData.reloadTimer - delta);
    }



    float ammo, fireCooldown_timer;
    public bool CheckFire() 
    {
        if (!gunfish.statusData.CanFire)
            return false;

        if (fireCooldown_timer > 0)
            return false;

        if (ammo <= 0)
            return false;

        // decrement ammo
        // set fire cooldown
        // reset ammo reload timer
        return true;
    }


    public void Fire()
    {
        if (!gunfish.statusData.CanFire) return;

        // check fire ability
        // check if fish can fire
        // check gun fire cooldown
        // check gun ammo

        // Reset fire timer
        gunfish.statusData.reloadTimer = gunfish.data.reloadTime;
        gunfish.Kickback(gunfish.data.gunKickback);
        Vector3 endPoint;

        FX_Spawner.instance?.SpawnFX(FXType.Bang, barrels[0].transform.position, Quaternion.LookRotation(barrels[0].transform.forward, barrels[0].transform.up));

        foreach (GunBarrel barrel in barrels) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(barrel.transform.position, barrel.transform.right, gunfish.data.gunRange, layerMask);
            endPoint = barrel.transform.position + barrel.transform.right * gunfish.data.gunRange;

            foreach (var hit in hits) {

                GunfishSegment fishSegment = hit.transform.GetComponent<GunfishSegment>();
                Shootable shootable = hit.transform.GetComponent<Shootable>();
                if (fishSegment != null) {
                    bool fishHit = (GameManager.instance != null) 
                        ? GameManager.instance.MatchManager.ResolveHit(this, fishSegment)
                        : ResolveHit(this, fishSegment);
                    if (fishHit) {
                        fishSegment.gunfish.Hit(new FishHitObject(fishSegment.index, hit.point, barrel.transform.right, gameObject, gunfish.data.gunDamage, gunfish.data.gunKnockback));
                        // this is temporary, but if it works let's just leave it
                        if (fishSegment.gunfish.statusData.health <= 0)
                            FX_Spawner.instance?.BAM();
                        endPoint = hit.point;
                        break;
                    }
                }
                else if (shootable != null) {
                    shootable.Hit(new HitObject(hit.point, barrel.transform.right, gameObject, gunfish.data.gunDamage, gunfish.data.gunKnockback));
                    endPoint = hit.point;
                    break;
                }
                else {
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

    public virtual bool ResolveHit(Gun gun, GunfishSegment segment) {
        return gun.gunfish != segment.gunfish;
    }
}
