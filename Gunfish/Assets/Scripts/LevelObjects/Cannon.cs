using DG.Tweening;
using System.Collections;
using System.Drawing.Text;
using UnityEngine;

public class Cannon : MonoBehaviour {
    [SerializeField] private FishDetector detector;
    [SerializeField] private PointEffector2D effector;
    [SerializeField] private Transform barrelSpriteTransform;
    [SerializeField] private float power;

    bool gottemSpottem;
    float cooldownTimer, cooldown = 2f;
    float shoostTimer, shoostDuration = 1f;

    float ignoreTimer = 1f;

    // Start is called before the first frame update
    void Start() {
        // get detector and hook into trigger collide
        detector.OnFishTriggerEnter += OnFishTrigger;
        detector.OnFishTriggerExit += OnFishLeave;
    }

    void OnFishTrigger(GunfishSegment segment, Collider2D fishCollider) {
        // freeze that fuckin fish
        segment.gunfish.AddEffect(new NoMove_Effect(segment.gunfish));
        if (gottemSpottem)
            return;
        gottemSpottem = true;
        shoostTimer = shoostDuration;
        // if we haven't started the shoost, then do so
    }

    void OnFishLeave(GunfishSegment segment, Collider2D fishCollider) {
        segment.gunfish.AddEffect(new NoMove_Effect(segment.gunfish, -1));
    }

    void BlastEm() {
        // iterate over fishes in detector and blow them to hell
        cooldownTimer = cooldown;
        gottemSpottem = false;
        GetComponent<AudioSource>().Play();
        barrelSpriteTransform.DOPunchScale(new Vector3(0.5f, 0.05f, 0f), 0.5f, 8, 0.5f);
        FX_Spawner.Instance.BAM(0);
        foreach (var fish in detector.fishes.Keys) {
            // launch the fuckers
            fish.AddEffect(new Flame_Effect(fish, 2));
            fish.Hit(new FishHitObject(fish.MiddleSegmentIndex, detector.transform.position, detector.transform.up, gameObject, 0, power, HitType.Impact, true));
            IgnoreFish(fish);
        }
    }

    // Update is called once per frame
    void Update() {
        // if gottem spottem, subtract shoost_timer
        if (gottemSpottem) {
            shoostTimer = Mathf.Max(0, shoostTimer - Time.deltaTime);
            if (shoostTimer <= 0) {
                BlastEm();
            }
        }

        if (cooldownTimer > 0) {
            cooldownTimer = Mathf.Max(0, cooldownTimer - Time.deltaTime);
            if (cooldownTimer <= 0 && detector.fishes.Count > 0) {
                gottemSpottem = true;
                shoostTimer = shoostDuration;
            }
        }
    }

    IEnumerator IgnoreFish(Gunfish gunfish) {
        effector.colliderMask &= ~(1 << gunfish.player.layer);
        yield return new WaitForSeconds(ignoreTimer);
        effector.colliderMask |= (1 << gunfish.player.layer);
    }
}