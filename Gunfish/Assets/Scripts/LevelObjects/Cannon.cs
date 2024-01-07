using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour {
    public FishDetector detector;
    public PointEffector2D effector;
    [SerializeField] private float power;
    bool gottemSpottem;
    float coolDown_timer, coolDown = 2f;
    float shoost_timer, shoostDuration = 2f;

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
        shoost_timer = shoostDuration;
        // if we haven't started the shoost, then do so
    }

    void OnFishLeave(GunfishSegment segment, Collider2D fishCollider) {
        segment.gunfish.AddEffect(new NoMove_Effect(segment.gunfish, -1));
    }

    void BlastEm() {
        // iterate over fishes in detector and blow them to hell
        coolDown_timer = coolDown;
        gottemSpottem = false;
        foreach (var fish in detector.fishes.Keys) {
            // launch the fuckers
            fish.Hit(new FishHitObject(fish.MiddleSegmentIndex, detector.transform.position, detector.transform.up, gameObject, 0, power, true));
            IgnoreFish(fish);
        }
    }

    // Update is called once per frame
    void Update() {
        // if gottem spottem, subtract shoost_timer
        if (gottemSpottem) {
            shoost_timer = Mathf.Max(0, shoost_timer - Time.deltaTime);
            if (shoost_timer <= 0) {
                BlastEm();
            }
        }

        if (coolDown_timer > 0) {
            coolDown_timer = Mathf.Max(0, coolDown_timer - Time.deltaTime);
            if (coolDown_timer <= 0 && detector.fishes.Count > 0) {
                gottemSpottem = true;
                shoost_timer = shoostDuration;
            }
        }
    }

    IEnumerator IgnoreFish(Gunfish gunfish) {
        effector.colliderMask &= ~(1 << gunfish.player.layer);
        yield return new WaitForSeconds(ignoreTimer);
        effector.colliderMask |= (1 << gunfish.player.layer);
    }
}