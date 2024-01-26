using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class LaserGun : Gun
{
    // firecooldown? yeah

    // NOTE(Wyatt): I'm just going to use 'ammo' as the charge amount.
    float chargeAmount, hitReduction=.25f, range;
    public Vector2 radiusRange = new Vector2();

    public Color laserColor;

    protected override void Start() {
        base.Start();
        ammo = 0;
        OnAmmoChanged?.Invoke(0);
        foreach (var barrel in barrels) {
            barrel.lr.startColor = laserColor;
            barrel.lr.endColor = laserColor;
            barrel.lr.widthMultiplier = 1f;
        }
    }

    // Update is called once per frame
    protected override void Update() {
        if (gunfish == null || gunfish.statusData == null)
            return;

        // fire cooldown
        fireCooldown_timer = Mathf.Max(0, fireCooldown_timer - Time.deltaTime);

        /*
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
        */
    }

    public override bool CheckFire() {
        if (!gunfish.statusData.CanFire)
            return false;

        // cooling down
        if (fireCooldown_timer > 0) {
            fireCooldown_timer = Mathf.Max(0, fireCooldown_timer - Time.deltaTime);
            OnAmmoChanged?.Invoke(fireCooldown_timer / gunfish.data.gun.fireCooldown);
            return false;
        }

        /*
        if (ammo <= 0)
            return false;

        ammo -= 1;
        OnAmmoChanged?.Invoke(ammo / gunfish.data.gun.maxAmmo);
        */
        /*
        reloadWait_timer = gunfish.data.gun.reloadWait;
        reload_timer = 0;
        */
        return true;
    }

    protected override bool CheckButtonStatus(ButtonStatus firingStatus) {
        return firingStatus != ButtonStatus.Up;// && !gunfish.underwater;
    }

    public override void Fire(ButtonStatus firingStatus) {
        // print($"Firing Status: {firingStatus}");

        if (!CheckFire())
            return;

        if ((firingStatus == ButtonStatus.Pressed && ammo == 0) || firingStatus == ButtonStatus.Holding) {
            ammo = Mathf.Min(gunfish.data.gun.maxAmmo, ammo + Time.deltaTime);
        }

        if (firingStatus == ButtonStatus.Released || ammo >= gunfish.data.gun.maxAmmo) {
            chargeAmount = ammo / gunfish.data.gun.maxAmmo;
            range = gunfish.data.gun.range * chargeAmount;
            ammo = 0;
            fireCooldown_timer = gunfish.data.gun.fireCooldown;
            // kickback is based on release
            Kickback(gunfish.data.gun.kickback * chargeAmount);
            _Fire();
        }
        OnAmmoChanged?.Invoke(ammo / gunfish.data.gun.maxAmmo);
    }

    protected override void _Fire() {
        Vector3 endPoint;

        FX_Spawner.Instance?.SpawnFX(
            FXType.Bang, barrels[0].transform.position, Quaternion.LookRotation(barrels[0].transform.forward, barrels[0].transform.up));

        HashSet<Gunfish> hitGunfishes = new HashSet<Gunfish>();

        Dictionary<Transform, RaycastHit2D> hitMap = new Dictionary<Transform, RaycastHit2D>();

        foreach (GunBarrel barrel in barrels) {
            AnimationCurve curve = new AnimationCurve();
            bool brokenStreak = false;
            hitGunfishes.Clear();
            foreach (var hit in Physics2D.RaycastAll(barrel.transform.position,
                barrel.transform.right,
                range,
                layerMask)) {
                hitMap[hit.transform] = hit;
            }
            RaycastHit2D[] hits = Physics2D.CircleCastAll(
                barrel.transform.position, 
                radiusRange.GetValueInRange(chargeAmount), 
                barrel.transform.right, 
                range, 
                layerMask);
            endPoint = barrel.transform.position + barrel.transform.right * range;
            barrel.ResetLR();
            curve.AddKey(0, radiusRange.GetValueInRange(chargeAmount));
            barrel.lr.widthCurve = curve;
            foreach (var hit in hits) {
                HitCounter hitCounter = hit.transform.GetComponentInParent<HitCounter>();
                if (hitCounter) {
                    hitCounter.TakeHit(gunfish);
                }
                if (hit.collider != null && hit.collider.isTrigger == true) {
                    continue;
                }
                GunfishSegment fishSegment = hit.transform.GetComponentInParent<GunfishSegment>();
                Shootable shootable = hit.transform.GetComponentInParent<Shootable>();
                ObjectMaterial objMat = hit.transform.GetComponentInParent<ObjectMaterial>();
                //barrel.lr.positionCount += 1;
                if (fishSegment != null) {
                    if (hitGunfishes.Contains(fishSegment.gunfish)) {
                        continue;
                    }
                    hitGunfishes.Add(fishSegment.gunfish);
                    // NOTE(Wyatt): this is how team deathmatch prevents friendly fire :)
                    bool fishHit = (GameManager.Instance != null)
                        ? GameModeManager.Instance.matchManagerInstance.ResolveHit(this, fishSegment)
                        : ResolveHit(this, fishSegment);
                    if (fishHit) {
                        // scale damage and knockback by charge amount
                        fishSegment.gunfish.Hit(
                            new FishHitObject(
                                fishSegment.index,
                                hit.point,
                                barrel.transform.right,
                                gameObject,
                                gunfish.data.gun.damage * chargeAmount,
                                gunfish.data.gun.knockback * chargeAmount,
                                HitType.Ballistic));
                        // this is temporary, but if it works let's just leave it
                        if (fishSegment.gunfish.statusData.health <= 0) {
                            FX_Spawner.Instance?.BAM();
                        }
                        endPoint = hit.point;
                        if (UpdateChargeAmount(barrel, endPoint, curve) == false) {
                            brokenStreak = true;
                            break;
                        }
                    }
                }
                else if (shootable != null) {
                    shootable.Hit(new HitObject(
                        hit.point,
                        barrel.transform.right,
                        gameObject,
                        gunfish.data.gun.damage * chargeAmount,
                        gunfish.data.gun.knockback * chargeAmount,
                        HitType.Ballistic));
                    endPoint = hit.point;
                    if (hitMap.ContainsKey(hit.transform)) {
                        endPoint = hitMap[hit.transform].point;
                    }
                    if (UpdateChargeAmount(barrel, endPoint, curve) == false) {
                        brokenStreak= true;
                        break;
                    }
                }
                else if (objMat != null) {
                    // TODO: replace with generalized FX_CollisionHandler code
                    Rigidbody2D otherRB = objMat.GetComponent<Rigidbody2D>();
                    FX_Spawner.Instance?.SpawnFX(FXType.Ground_Hit, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
                    endPoint = hit.point;
                    bool directHit = false;
                    if (hitMap.ContainsKey(hit.transform)) {
                        endPoint = hitMap[hit.transform].point;
                        directHit = true;
                    }
                    objMat.Shoot();
                    if (otherRB != null) {
                        otherRB.AddForceAtPosition(gunfish.data.gun.knockback * -hit.normal * chargeAmount, hit.point, ForceMode2D.Impulse);
                        if (UpdateChargeAmount(barrel, endPoint, curve) == false) {
                            brokenStreak = true;
                            break;
                        }
                    }
                    else if (directHit) {
                        endPoint = hitMap[hit.transform].point;
                        brokenStreak = true;
                        break;
                    }
                }
                else {
                    // TODO: replace with generalized FX_CollisionHandler code
                    FX_Spawner.Instance?.SpawnFX(FXType.Ground_Hit, hit.point, Quaternion.LookRotation(Vector3.forward, hit.normal));
                    endPoint = hit.point;
                    if (hitMap.ContainsKey(hit.transform)) {
                        endPoint = hitMap[hit.transform].point;
                        brokenStreak = true;
                        break;
                    }
                    else if (UpdateChargeAmount(barrel, endPoint, curve) == false) {
                        brokenStreak = true;
                        break;
                    }
                }
                // if gunfish, hit
                // if ground, spawn fx
            }
            if (!brokenStreak) {
                endPoint = barrel.transform.position + (barrel.transform.right * range);
                curve.AddKey(1, radiusRange.GetValueInRange(chargeAmount));
            }
            barrel.lr.widthCurve = curve;
            barrel.Flash(endPoint);
        }
    }

    public bool UpdateChargeAmount(GunBarrel barrel, Vector2 hitPoint, AnimationCurve curve) {
        chargeAmount -= hitReduction;
        if (chargeAmount <= 0) {
            return false;
        }
        // update lr
        float dist = (hitPoint - (Vector2)barrel.transform.position).magnitude;
        barrel.lr.SetPosition(barrel.lr.positionCount-1, barrel.transform.position + (barrel.transform.right * dist));
        curve.AddKey(dist / range, radiusRange.GetValueInRange(chargeAmount));
        barrel.lr.positionCount += 1;
        return true;
    }
}
