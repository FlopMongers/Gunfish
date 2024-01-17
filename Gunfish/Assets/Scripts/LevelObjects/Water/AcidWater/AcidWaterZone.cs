using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageRateTuple {
    public MaterialType matType;
    public float damageRate;
}

public class AcidWaterZone : WaterZone
{
    public bool acidifying = true;
    // how many seconds does the acidification last
    public float acidifyingDuration = 45;
    float acidTimer = 0;
    // after how many seconds does it start being acidic
    public float acidThreshold = 10;

    public Vector2 baseDamageScaleRange;
    public float defaultDamageRate;
    public Vector2 bubbleSpawnRateRange = new Vector2(5, 20);
    public Color baseColor = Color.blue;
    public Color acidColor = Color.green;

    public SpriteRenderer waterRenderer;

    public ParticleSystem bubbles;

    public List<DamageRateTuple> damageRateList = new List<DamageRateTuple>();
    Dictionary<MaterialType, float> damageRateMap = new Dictionary<MaterialType, float>();


    // Start is called before the first frame update
    protected override void Start()
    {
        foreach (DamageRateTuple damageTuple in damageRateList) {
            damageRateMap[damageTuple.matType] = damageTuple.damageRate;
        }
        waterRenderer.material.color = baseColor;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (acidifying) {
            acidTimer = Mathf.Clamp(acidTimer + Time.deltaTime, 0, acidifyingDuration);
            float pogress = acidTimer / acidifyingDuration;
            if (acidTimer >= acidThreshold) {
                splashType = FXType.AcidSplash;
                var em = bubbles.emission;
                em.rateOverTime = Mathf.Lerp(bubbleSpawnRateRange.x, bubbleSpawnRateRange.y, pogress);
                // iterate over submerged fish and shootables and damage them :imp:
                DamageSubmergedObjects();
            }
            waterRenderer.material.color = Color.Lerp(baseColor, acidColor, pogress);
        }
    }

    void DamageSubmergedObjects() {
        if (acidTimer < acidThreshold)
            return;
        float damage = Time.deltaTime * Mathf.Lerp(
            baseDamageScaleRange.x, baseDamageScaleRange.y, ExtensionMethods.GetNormalizedValueInRange(acidTimer, acidThreshold, acidifyingDuration));
        foreach ((Gunfish fish, int count) in detector.fishes) {
            // damage according to fish type
            fish.Hit(new FishHitObject(
                0,
                fish.RootSegment.transform.position,
                Vector2.zero,
                gameObject,
                damage * damageRateMap.GetValueOrDefault(MaterialType.Fish, defaultDamageRate),
                0,
                HitType.Acid,
                ignoreFX:true)
            );
        }
        foreach ((Shootable shootable, int count) in submergedShootables) {
            // damage according to material type
            float damageRate = defaultDamageRate;
            ObjectMaterial objMat = shootable.GetComponent<ObjectMaterial>();
            if (objMat != null) {
                damageRate = damageRateMap.GetValueOrDefault(objMat.materialType, defaultDamageRate);
            }
            shootable.Hit(new HitObject(
                shootable.transform.position,
                Vector2.zero,
                gameObject,
                damage * damageRate,
                0,
                HitType.Acid,
                ignoreFX: true)
            );
        }
    }
}
