using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WaterInteractor))]
public class SeaUrchin : MonoBehaviour {
    [SerializeField]
    private FishDetector detector;

    [SerializeField]
    [Range(0f, 100f)]
    private float prickMagnitude = 10f;

    [SerializeField]
    [Range(0f, 10f)]
    private float prickDamage = 10f;

    public UnityEngine.Transform spriteTransform;
    Vector3 originalScale;
    Vector3 punchScale = new Vector2(0.1f, 0.1f);
    float bounceDuration = 0.25f;

    float dashTimer = 0;
    Vector2 dashTimerRange = new Vector2(1f, 5f);
    Vector3 dashForceRange = new Vector3(1f, 4f, 2f);
    public Rigidbody2D rb;

    public WaterInteractor waterInteractor;

    private void Start() {
        detector.OnFishCollideEnter += OnFishEnter;
        if (spriteTransform == null)
            spriteTransform = transform.FindDeepChild("Sprite");
        originalScale = spriteTransform.localScale;
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        dashTimer = UnityEngine.Random.Range(dashTimerRange.x, dashTimerRange.y);
        if (waterInteractor == null)
            waterInteractor = gameObject.CheckAddComponent<WaterInteractor>();
        waterInteractor.underwaterChangeEvent += OnUnderwaterChange;
    }

    private void Update() {
        // if enough time has passed,
        // reset timer
        // randomly push urchin, add small rotation
        if (dashTimer <= 0) {
            dashTimer = UnityEngine.Random.Range(dashTimerRange.x, dashTimerRange.y);
            rb.AddForce(UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(dashForceRange.x, dashForceRange.y), ForceMode2D.Impulse);
            rb.AddTorque(UnityEngine.Random.Range(-dashForceRange.z, dashForceRange.z), ForceMode2D.Impulse);
        }
        dashTimer -= Time.deltaTime;
    }

    public void OnUnderwaterChange(bool underwater) {
        // update gravity
        rb.gravityScale = underwater ? 0 : 1;
    }

    private void OnFishEnter(GunfishSegment segment, Collision2D collision) {
        var direction = (segment.transform.position - transform.position).normalized;

        Debug.Log(segment.rb);
        Debug.Log(direction);
        segment.gunfish.Hit(new FishHitObject(
            segment.index, 
            collision.contacts[0].point, 
            -collision.contacts[0].normal, 
            gameObject, 
            prickDamage, 
            prickMagnitude,
            HitType.Impact));
        spriteTransform.DOScale(originalScale + punchScale, bounceDuration)
        .SetEase(Ease.OutBounce)
        .OnComplete(() => {
            spriteTransform.DOScale(originalScale - punchScale, 0.1f);
        });
    }
}