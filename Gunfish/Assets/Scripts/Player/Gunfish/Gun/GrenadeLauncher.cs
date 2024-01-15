using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GrenadeLauncher : Gun
{
    public GameObject grenadePrefab;
    Grenade grenade;
    FishDetector detector;

    public float spikeDamage = 1.5f;
    public float spikeKnockback = 10f;

    public float grenadeForceMultiplier = 2f;

    protected override void Start() {
        base.Start();
        // hook into fish detector
        detector = gunfish.RootSegment.AddComponent<FishDetector>();
        detector.DetectCollision = false;
        detector.DetectTrigger = false;
        gunfish.RootSegment.GetComponent<CompositeCollisionDetector>().OnComponentCollideEnter += OnCompositeCollideEnter;
        gunfish.RootSegment.GetComponent<CompositeCollisionDetector>().OnComponentCollideExit += OnCompositeCollideExit;
    }

    void OnCompositeCollideEnter(GameObject src, Collision2D collision) {
        // if fish
        GunfishSegment segment = collision.otherCollider.GetComponent<GunfishSegment>();
        if (segment == null || segment.gunfish == gunfish)
            return;

        if (detector.DetectFishEnter(segment)) {
            // ATTACK
            segment.gunfish.Hit(new FishHitObject(
                segment.index,
                collision.contacts[0].point,
                -collision.contacts[0].normal,
                gameObject,
                spikeDamage,
                spikeKnockback,
                HitType.Impact));
        }
    }

    void OnCompositeCollideExit(GameObject src, Collision2D collision) {
        // if fish
        GunfishSegment segment = collision.otherCollider.GetComponent<GunfishSegment>();
        if (segment == null || segment.gunfish == gunfish)
            return;

        detector.DetectFishExit(segment);
    }

    public override bool CheckFire() {
        if (!gunfish.statusData.CanFire)
            return false;

        if (fireCooldown_timer > 0)
            return false;

        if (ammo <= 0 && grenade == null)
            return false;

        if (grenade == null) {
            ammo -= 1;
            OnAmmoChanged?.Invoke(ammo / gunfish.data.gun.maxAmmo);
        }

        fireCooldown_timer = gunfish.data.gun.fireCooldown;
        reloadWait_timer = gunfish.data.gun.reloadWait;
        reload_timer = 0;
        return true;
    }

    protected override void _Fire() {

        if (grenade != null) {
            grenade.Explode();
            grenade = null;
        }
        else {
            // fireFX
            FX_Spawner.Instance?.SpawnFX(
                FXType.Bang, barrels[0].transform.position, Quaternion.LookRotation(barrels[0].transform.forward, barrels[0].transform.up));
            // spawn grenade
            var nadeObj = Instantiate(grenadePrefab, barrels[0].transform.position, Quaternion.LookRotation(barrels[0].transform.forward, barrels[0].transform.up));
            var nade = nadeObj.GetComponent<Grenade>();
            nade.sourceGunfish = gunfish;
            grenade = nade;
            // NOTE: give grenade duration here?
            // give it some force
            nadeObj.GetComponent<Rigidbody2D>().AddForce(barrels[0].transform.right * gunfish.data.gun.range * grenadeForceMultiplier, ForceMode2D.Impulse);
        }
    }
}
