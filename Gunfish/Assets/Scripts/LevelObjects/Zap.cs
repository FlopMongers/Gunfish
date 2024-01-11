using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zap : MonoBehaviour
{
    public FishDetector fishDetector;

    public float zapDamage;
    public float zapDuration;

    HashSet<Gunfish> zappedFishes = new HashSet<Gunfish>();

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
        segment.gunfish.AddEffect(new Zap_Effect(segment.gunfish, zapDuration));
    }
}
