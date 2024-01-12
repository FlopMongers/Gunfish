using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class HurtBox : MonoBehaviour {
    [SerializeField]
    float damage;

    HashSet<Gunfish> hurtFish = new HashSet<Gunfish>();

    // add start method to initialize fish detector
    public void Start() {
        var detector = GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += HandleFishTriggerEnter;
        detector.OnFishTriggerExit += HandleFishTriggerExit;
    }

    public void FixedUpdate() {

        // can't? remove dead fish while iterating collection
        List<Gunfish> fishesToRemove = new List<Gunfish>();
        foreach (Gunfish fish in hurtFish) {
            float frameDamage = damage * Time.deltaTime;
            //print("Hurting " + fish.name + " for " + frameDamage);
            fish.Hit(new FishHitObject(0, transform.position, Vector2.zero, gameObject, frameDamage, 0, HitType.Impact, true));
            //fish.UpdateHealth(-frameDamage);
            if (!fish.statusData.alive) {
                print("They dead");
                fishesToRemove.Add(fish);
            }
        }

        // remove dead fishes from set now that we're done
        foreach (Gunfish deadFish in fishesToRemove) {
            hurtFish.Remove(deadFish);
        }
    }

    protected void HandleFishTriggerEnter(GunfishSegment segment, Collider2D collider) {
        hurtFish.Add(segment.gunfish);
    }

    protected void HandleFishTriggerExit(GunfishSegment segment, Collider2D collider) {
        hurtFish.Remove(segment.gunfish);
    }

}