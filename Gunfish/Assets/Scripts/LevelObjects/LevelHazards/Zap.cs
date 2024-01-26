using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class Zap : MonoBehaviour
{
    public FishDetector fishDetector;

    public float zapDamage;
    public float zapDuration;

    [HideInInspector]
    public HashSet<Gunfish> zappedFishes = new HashSet<Gunfish>();

    // NOTE(Wyatt): this is passed by reference
    public Dictionary<Gunfish, float> fishZapMap = new Dictionary<Gunfish, float>();

    static float MIN_ZAP_TIME = 1f;

    // Start is called before the first frame update
    void Start()
    {
        fishDetector = fishDetector ?? GetComponent<FishDetector>();
        fishDetector.OnFishTriggerEnter += ZapFish;
    }

    private void ZapFish(GunfishSegment segment, Collider2D collision) {
        if (zappedFishes.Contains(segment.gunfish) == true || segment.gunfish.anySegmentUnderwater <= 0) {
            return;
        }
        zappedFishes.Add(segment.gunfish);
        var direction = (segment.transform.position - transform.position).normalized;
        segment.gunfish.Hit(new FishHitObject(segment.index, segment.transform.position, direction, gameObject, zapDamage, 0, HitType.Electric));
        if (!fishZapMap.ContainsKey(segment.gunfish) && (Time.time - fishZapMap[segment.gunfish]) < MIN_ZAP_TIME) {
            segment.gunfish.AddEffect(new Zap_Effect(segment.gunfish, zapDuration));
            fishZapMap[segment.gunfish] = Time.time;
        }
    }
}
