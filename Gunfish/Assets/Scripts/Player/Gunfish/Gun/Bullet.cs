using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(Destroyer))]
[RequireComponent(typeof(Fader))]
[RequireComponent(typeof(WaterInteractor))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(FishDetector))]
public class Bullet : MonoBehaviour
{
    bool destroyed, starting=true;

    public Gunfish gunfish;

    public Vector2 speedRange = new Vector2(5f, 20f);
    public float maxSpeed = 40;

    public Rigidbody2D rb;
    public FishDetector detector;
    public WaterInteractor waterInteractor;
    public Destroyer destroyer;
    public ParticleSystem bubbles;

    public Vector2 lastSpeed;


    // Start is called before the first frame update
    void Start()
    {
        // hook up to fish detector
        detector.OnFishCollideEnter += OnFishHit;
        waterInteractor.underwaterChangeEvent += OnUnderwaterChange;
        
    }

    // Update is called once per frame
    void Update()
    {
        // check rb speed
        if (!starting && !destroyed && rb.velocity.magnitude <= speedRange.x) {
            destroyed = true;
            destroyer.GETTEM();
        }
        else if (!destroyed) {
            lastSpeed = rb.velocity;
        }
        // if less than range, then destroy the bullet
    }

    public void SetSpeed(Vector2 dir,float percent) {
        starting = false;
        rb.velocity = dir * maxSpeed * percent;
    }

    void OnFishHit(GunfishSegment segment, Collision2D collision) {
        // if fast enough and not destroyed and not sourceGunfish, WHACK THE FISH
        if (!destroyed && segment.gunfish != gunfish && collision.relativeVelocity.magnitude > speedRange.x) {
            float relVel = Mathf.Clamp(collision.relativeVelocity.magnitude, 0, speedRange.y);
            float damageRatio = ExtensionMethods.GetNormalizedValueInRange(relVel, speedRange.x, speedRange.y);
            segment.gunfish.Hit(
                new FishHitObject(
                    segment.index,
                    collision.contacts[0].point,
                    -collision.contacts[0].normal,
                    gunfish.gun.gameObject,
                    gunfish.data.gun.damage * damageRatio,
                    gunfish.data.gun.knockback * damageRatio,
                    HitType.Ballistic));
        }
        destroyed = true;
        destroyer.GETTEM();
    }

    void OnUnderwaterChange(bool underwater) {
        if (underwater == true) {
            bubbles.Stop();
        }
        else {
            bubbles.Play();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.rigidbody == null)
            return;
        var shootable = collision.rigidbody.GetComponent<Shootable>();
        if (shootable != null && !destroyed && collision.relativeVelocity.magnitude > speedRange.x) {
            float relVel = Mathf.Clamp(collision.relativeVelocity.magnitude, 0, speedRange.y);
            float damageRatio = ExtensionMethods.GetNormalizedValueInRange(relVel, speedRange.x, speedRange.y);
            shootable.Hit(new HitObject(
                collision.contacts[0].point,
                -collision.contacts[0].normal,
                gunfish.gun.gameObject,
                gunfish.data.gun.damage * damageRatio,
                gunfish.data.gun.knockback * damageRatio,
                HitType.Ballistic));
        }
    }
}
