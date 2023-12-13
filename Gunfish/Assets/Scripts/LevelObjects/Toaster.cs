using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toaster : Shootable
{
    public WaterInteractor waterInteractor;
    public FishDetector detector;

    bool zapping;

    HashSet<Gunfish> zappedFishes = new HashSet<Gunfish>();

    float zapWaitTimer, zapTimer, zapTimerDuration = 0.5f;
    Vector2 zapWaitTimerRange = new Vector2(6f, 10f);
    public Animator anim;
    public ParticleSystem zapEffector;
    int waterMask = 0;

    public GameObject toastPrefab;
    float toastForce = 1f;
    Collider2D toastInstance;
    public Collider2D ownCollider;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        waterMask = LayerMask.GetMask("Water");
        detector.OnFishCollideEnter += OnFishCollide;
        detector.OnFishTriggerEnter += OnFishTrigger;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!damaged) {
            if (waterInteractor.isUnderwater > 0) {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 1f, waterMask);
                if (hit.collider != null)
                    UpdateHealth(-health);
            }
            return;
        }

        if (zapWaitTimer <= 0 && !zapping) {
            if (anim != null) // don't know why ? isn't working...
                anim?.SetBool("Zap", true);
            if (waterInteractor.isUnderwater > 0) {
                if (zapEffector != null)
                    zapEffector?.Play();
            }
            foreach (var target in detector.fishes.Keys) {
                ZapFish(target);
                zappedFishes.Add(target);
            }
            zapTimer = zapTimerDuration;
        }
        if (zapping) {
            zapTimer -= Time.deltaTime;
            if (zapTimer <= 0) {
                if (anim != null)
                    anim?.SetBool("Zap", false);
                zapping = false;
                zappedFishes.Clear();
                if (zapEffector != null)
                    zapEffector?.Stop();
                zapWaitTimer = Random.Range(zapWaitTimerRange.x, zapWaitTimerRange.y);
            }
        }
    }


    void OnFishCollide(GunfishSegment segment, Collision2D collision) {
        if (zapping && zappedFishes.Contains(segment.gunfish) == false) {
            ZapFish(segment.gunfish);
            zappedFishes.Add(segment.gunfish);
        }
    }


    void OnFishTrigger(GunfishSegment segment, Collider2D collision) {
        if (zapping && waterInteractor.isUnderwater > 0 && zappedFishes.Contains(segment.gunfish) == false && segment.gunfish.anySegmentUnderwater > 0) {
            ZapFish(segment.gunfish);
            zappedFishes.Add(segment.gunfish);
        }
    }

    void ZapFish(Gunfish gunfish) {

    }

    protected override void Damage() {
        base.Damage();
        zapWaitTimer = 0;
        // instantiate toast
        toastInstance = Instantiate(toastPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, transform.up)).GetComponent<Collider2D>();
        toastInstance.gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * toastForce, ForceMode2D.Impulse);
        Physics2D.IgnoreCollision(ownCollider, toastInstance, true);
        Invoke("CollideToast", 1f);
    }

    void CollideToast() {
        if (toastInstance != null) {
            Physics2D.IgnoreCollision(ownCollider, toastInstance.GetComponent<Collider2D>(), false);
        }
    }
}
