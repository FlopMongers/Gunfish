using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.Rendering;

public class Gun : MonoBehaviour {
    public Gunfish gunfish;
    public List<GunBarrel> barrels = new List<GunBarrel>();

    public FloatGameEvent OnAmmoChanged;

    private int layerMask;

    public float ammo;
    protected float fireCooldown_timer, reload_timer, reloadWait_timer;

    // Start is called before the first frame update
    void Start() {
        layerMask = LayerMask.GetMask("Player1", "Player2", "Player3", "Player4", "Ground", "Default");
    }

    // Update is called once per frame
    protected virtual void Update() {
        if (gunfish == null || gunfish.statusData == null)
            return;

        // fire cooldown
        fireCooldown_timer = Mathf.Max(0, fireCooldown_timer - Time.deltaTime);

        // reload timer
        if (reload_timer > 0) {
            reload_timer = Mathf.Max(0, reload_timer - Time.deltaTime);
            OnAmmoChanged?.Invoke(1 - (reload_timer / gunfish.data.gun.reload));
            if (reload_timer <= 0) {
                ammo = gunfish.data.gun.maxAmmo;
                OnAmmoChanged?.Invoke(ammo / gunfish.data.gun.maxAmmo);
            }
        }
        // if ammo is not full
        else if (ammo != gunfish.data.gun.maxAmmo && reloadWait_timer > 0) {
            reloadWait_timer = Mathf.Max(0, reloadWait_timer - Time.deltaTime);
            if (reloadWait_timer <= 0)
                reload_timer = gunfish.data.gun.reload;
        }
    }

    public bool CheckFire() {
        if (!gunfish.statusData.CanFire)
            return false;

        if (fireCooldown_timer > 0)
            return false;

        if (ammo <= 0)
            return false;

        ammo -= 1;
        OnAmmoChanged?.Invoke(ammo / gunfish.data.gun.maxAmmo);

        fireCooldown_timer = gunfish.data.gun.fireCooldown;
        reloadWait_timer = gunfish.data.gun.reloadWait;
        reload_timer = 0;
        return true;
    }

    protected virtual bool CheckButtonStatus(ButtonStatus firingStatus) {
        return firingStatus == ButtonStatus.Pressed && !gunfish.underwater;
    }

    public virtual void Fire(ButtonStatus firingStatus) {
        // print($"Firing Status: {firingStatus}");

        // if pressed, then fire
        if (CheckButtonStatus(firingStatus) == false)
            return;

        if (!CheckFire())
            return;

        Kickback(gunfish.data.gun.kickback);
        _Fire();
    }

    protected virtual void _Fire() {
        Vector3 endPoint;

        FX_Spawner.Instance?.SpawnFX(
            FXType.Bang, barrels[0].transform.position, Quaternion.LookRotation(barrels[0].transform.forward, barrels[0].transform.up));

        foreach (GunBarrel barrel in barrels) {
            RaycastHit2D[] hits = Physics2D.RaycastAll(barrel.transform.position, barrel.transform.right, gunfish.data.gun.range, layerMask);
            endPoint = barrel.transform.position + barrel.transform.right * gunfish.data.gun.range;

            foreach (var hit in hits) {
                if (hit.collider != null && hit.collider.isTrigger == true) {
                    continue;
                }
                GunfishSegment fishSegment = hit.transform.GetComponentInParent<GunfishSegment>();
                Shootable shootable = hit.transform.GetComponentInParent<Shootable>();
                if (fishSegment != null) {
                    // NOTE(Wyatt): this is how team deathmatch prevents friendly fire :)
                    bool fishHit = (GameManager.Instance != null)
                        ? GameModeManager.Instance.matchManagerInstance.ResolveHit(this, fishSegment)
                        : ResolveHit(this, fishSegment);
                    if (fishHit) {
                        fishSegment.gunfish.Hit(
                            new FishHitObject(
                                fishSegment.index, 
                                hit.point, 
                                barrel.transform.right, 
                                gameObject, 
                                gunfish.data.gun.damage,
                                gunfish.data.gun.knockback,
                                HitType.Ballistic));
                        // this is temporary, but if it works let's just leave it
                        if (fishSegment.gunfish.statusData.health <= 0)
                            FX_Spawner.Instance?.BAM();
                        endPoint = hit.point;
                        break;
                    }
                }
                else if (shootable != null) {
                    shootable.Hit(new HitObject(
                        hit.point, 
                        barrel.transform.right, 
                        gameObject, 
                        gunfish.data.gun.damage, 
                        gunfish.data.gun.knockback,
                        HitType.Ballistic));
                    endPoint = hit.point;
                    break;
                }
                else {
                    // TODO: replace with generalized FX_CollisionHandler code
                    FX_Spawner.Instance?.SpawnFX(FXType.Ground_Hit, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
                    endPoint = hit.point;
                    break;
                }
                // if gunfish, hit
                // if ground, spawn fx
            }
            barrel.Flash(endPoint);
        }
    }

    public void Kickback(float kickback) {
        var direction = (gunfish.segments[1].transform.position - gunfish.segments[0].transform.position).normalized;
        // gun kickback
        gunfish.body.ApplyForceToSegment(0, direction * kickback, ForceMode2D.Impulse);
    }

    public virtual bool ResolveHit(Gun gun, GunfishSegment segment) {
        return gun.gunfish != segment.gunfish;
    }
}