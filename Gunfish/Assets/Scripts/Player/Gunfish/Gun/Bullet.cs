using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Destroyer))]
[RequireComponent(typeof(Fader))]
[RequireComponent(typeof(WaterInteractor))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(FishDetector))]
public class Bullet : MonoBehaviour
{
    protected bool destroyed, starting=true;
    public bool velocityFalloff = true;

    public Gunfish gunfish;

    public Vector2 speedRange = new Vector2(5f, 20f);
    public float maxSpeed = 40;

    public Rigidbody2D rb;
    public FishDetector detector;
    public WaterInteractor waterInteractor;
    public Destroyer destroyer;
    public ParticleSystem bubbles;
    public Collider2D col;

    public Vector2 lastSpeed;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        // hook up to fish detector
        detector.OnFishCollideEnter += OnFishHit;
        waterInteractor.underwaterChangeEvent += OnUnderwaterChange;
    }

    // Update is called once per frame
    void Update()
    {
        // check rb speed
        if (!starting && !destroyed && rb.linearVelocity.magnitude <= speedRange.x) {
            Gettem();
        }
        else if (!destroyed) {
            lastSpeed = rb.linearVelocity;
        }
        else {
            rb.linearVelocity = Vector2.zero;
        }
        // if less than range, then destroy the bullet
    }

    public void SetSpeed(Vector2 dir,float percent) {
        starting = false;
        rb.linearVelocity = dir * maxSpeed * percent; //Mathf.Clamp(maxSpeed * percent, speedRange.x, maxSpeed);
    }

    HashSet<Gunfish> hitFish = new HashSet<Gunfish>();
    void OnFishHit(GunfishSegment segment, Collision2D collision) {
        // if fast enough and not destroyed and not sourceGunfish, WHACK THE FISH
        if (!destroyed && hitFish.Contains(segment.gunfish) == false && segment.gunfish != gunfish && collision.relativeVelocity.magnitude > speedRange.x) {
            hitFish.Add(segment.gunfish);
            float relVel = Mathf.Clamp(collision.relativeVelocity.magnitude, 0, speedRange.y);
            float damageRatio = (velocityFalloff) ? ExtensionMethods.GetNormalizedValueInRange(relVel, speedRange.x, speedRange.y, clamp:true) : 1f;
            segment.gunfish.Hit(
                new FishHitObject(
                    segment.index,
                    collision.contacts[0].point,
                    -collision.contacts[0].normal,
                    (gunfish == null || gunfish.gun == null) ? gameObject : gunfish.gun.gameObject,
                    gunfish.data.gun.damage * damageRatio,
                    gunfish.data.gun.knockback * damageRatio,
                    HitType.Ballistic));
        }
        Gettem();
    }

    void OnUnderwaterChange(bool underwater) {
        if (underwater == true) {
            bubbles.Stop();
        }
        else {
            bubbles.Play();
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) {
        if (collision.rigidbody == null || destroyed == true) {
            Gettem();
            return;
        }
        var shootable = collision.rigidbody.GetComponent<Shootable>();
        var bullet = collision.rigidbody.GetComponent<Bullet>();
        var hitGunfish = collision.rigidbody.GetComponent<Gunfish>();
        var hitSegment = (detector.DetectGun == false) ? collision.collider.GetComponent<GunfishSegment>() : collision.collider.GetComponentInParent<GunfishSegment>();

        

        if (shootable != null && !destroyed && collision.relativeVelocity.magnitude > speedRange.x) {
            float relVel = Mathf.Clamp(collision.relativeVelocity.magnitude, 0, speedRange.y);
            float damageRatio = (velocityFalloff) ? ExtensionMethods.GetNormalizedValueInRange(relVel, speedRange.x, speedRange.y, clamp:true) : 1f;
            shootable.Hit(new HitObject(
                collision.contacts[0].point,
                -collision.contacts[0].normal,
                (gunfish != null && gunfish.gun != null) ? gunfish.gun.gameObject : null,
                gunfish.data.gun.damage * damageRatio,
                gunfish.data.gun.knockback * damageRatio,
                HitType.Ballistic));
            Gettem();
        }
        else if (hitGunfish && hitFish.Contains(hitGunfish) == false && hitGunfish != gunfish) {
            hitFish.Add(hitGunfish);
            hitGunfish.Hit(
                new FishHitObject(
                    -1,
                    collision.contacts[0].point,
                    -collision.contacts[0].normal,
                    (gunfish == null || gunfish.gun == null) ? gameObject : gunfish.gun.gameObject,
                    gunfish.data.gun.damage,
                    gunfish.data.gun.knockback,
                    HitType.Ballistic));
        }
        else if (hitSegment != null && hitFish.Contains(hitSegment.gunfish) == false && hitSegment.gunfish != gunfish) {
            float relVel = Mathf.Clamp(collision.relativeVelocity.magnitude, 0, speedRange.y);
            float damageRatio = (velocityFalloff) ? ExtensionMethods.GetNormalizedValueInRange(relVel, speedRange.x, speedRange.y, clamp:true) : 1f;
            hitFish.Add(hitSegment.gunfish);
            hitSegment.gunfish.Hit(
                new FishHitObject(
                    hitSegment.index,
                    collision.contacts[0].point,
                    -collision.contacts[0].normal,
                    (gunfish == null || gunfish.gun == null) ? gameObject : gunfish.gun.gameObject,
                    gunfish.data.gun.damage * damageRatio,
                    gunfish.data.gun.knockback * damageRatio,
                    HitType.Ballistic));
        }
        else if (!(bullet != null || hitGunfish == gunfish || hitSegment?.gunfish == gunfish)) {
            Gettem();
        }
    }

    protected virtual void Gettem() {
        if (destroyed)
            return;
        col.enabled = false;
        destroyed = true;
        destroyer.GETTEM();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
