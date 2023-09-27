using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class Shootable : MonoBehaviour
{
    public float maxHealth;
    [Range(0f, 1f)]
    public float damagedThreshold;
    public Sprite damagedSprite;
    bool damaged;


    public float health;
    public FloatGameEvent OnHealthUpdated;
    public GameEvent OnDead;

    public FXType destroyFX, hitFX, damagedFX;

    Rigidbody2D rb;

    public bool handleCollisionDamage = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;

        // TODO init properly
        var healthBar = GetComponent<HealthUI>();
        if (healthBar == null && FX_Spawner.instance != null) 
        {
            healthBar = Instantiate(FX_Spawner.instance.healthUIPrefab, transform).GetComponent<HealthUI>();
        }
        if (healthBar != null) {
            healthBar.transform.parent = null;
            healthBar.Init(this);
        }

        // check for collision damage handler and receiver and collision detectors
        if (handleCollisionDamage) {
            gameObject.CheckAddComponent<CollisionDamageHandler>();
            gameObject.CheckAddComponent<CollisionDamageReceiver>();
            if (gameObject.GetComponent<CompositeCollisionDetector>() == null) {
                gameObject.CheckAddComponent<CompositeCollisionDetector>().Init(true, true, true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            OnDead?.Invoke();
            FX_Spawner.instance?.SpawnFX(destroyFX, transform.position, transform.up);
            Destroy(gameObject);
        }
    }

    public void Hit(HitObject hit)
    {
        // reduce health
        if (rb != null)
        {
            rb.AddForceAtPosition(hit.direction*hit.knockback, hit.position);
        }
        FX_Spawner.instance?.SpawnFX(hitFX, hit.position, -hit.direction);
        UpdateHealth(-hit.damage);
    }

    public void UpdateHealth(float amount)
    {
        health += amount;
        if (!damaged && health > 0 && health < maxHealth * damagedThreshold)
        {
            if (damagedSprite != null)
            {
                var r = GetComponent<SpriteRenderer>();
                if (r != null) r.sprite = damagedSprite;
                FX_Spawner.instance?.SpawnFX(damagedFX, transform.position, Quaternion.identity);
                damaged = true;
            }
        }
        OnHealthUpdated?.Invoke(health);
    }
}
