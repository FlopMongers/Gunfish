using DG.Tweening;
using FunkyCode;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxerCrab : MonoBehaviour
{
    public FishDetector detector;

    public float punchDamage = 2f;
    public float punchKnockback = 15f;

    public GameObject zapFX;
    GameObject currentZap;

    // Start is called before the first frame update
    void Start()
    {
        detector.OnFishCollideEnter += OnFishEnter;
    }

    private void OnFishEnter(GunfishSegment segment, Collision2D collision) {
        var direction = (segment.transform.position - transform.position).normalized;

        segment.gunfish.Hit(new FishHitObject(
            segment.index,
            collision.contacts[0].point,
            -collision.contacts[0].normal,
            gameObject,
            punchDamage,
            punchKnockback,
            HitType.Impact));
        segment.gunfish.AddEffect(new Zap_Effect(segment.gunfish, 1f));
        // TODO: play animation
        if (currentZap == null) {
            currentZap = Instantiate(zapFX, transform.position, Quaternion.identity);
        }
    }
}
