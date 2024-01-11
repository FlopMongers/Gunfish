using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;

public class Shootable : MonoBehaviour, IHittable {
    public float maxHealth;
    [Range(0f, 1f)]
    public float damagedThreshold;
    public Sprite damagedSprite;
    protected bool damaged;

    public bool indestructible = false;

    public float health;
    public FloatGameEvent OnHealthUpdated;
    public HitEvent OnHit;
    public GameEvent OnDead;

    public FXType destroyFX, hitFX, damagedFX;

    Rigidbody2D rb;

    public bool handleCollisionDamage = true;

    public bool addDestroyer;
    public Destroyer destroyer;

    bool dead;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;

        // TODO init properly
        var healthBar = GetComponent<HealthUI>();
        if (healthBar == null && FX_Spawner.Instance != null) {
            healthBar = Instantiate(FX_Spawner.Instance.healthUIPrefab, transform).GetComponent<HealthUI>();
        }
        if (healthBar != null) {
            healthBar.transform.parent = null;
            healthBar.Init(this);
        }

        if (addDestroyer) {
            destroyer = gameObject.CheckAddComponent<Destroyer>();
            gameObject.CheckAddComponent<Fader>();
        }
        destroyer = destroyer ?? GetComponent<Destroyer>();

        // check for collision damage handler and receiver and collision detectors
        if (handleCollisionDamage) {
            gameObject.CheckAddComponent<CollisionDamageDealer>();
            gameObject.CheckAddComponent<CollisionDamageReceiver>();
            if (gameObject.GetComponent<CompositeCollisionDetector>() == null) {
                gameObject.CheckAddComponent<CompositeCollisionDetector>().Init(true, true, true);
            }
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (dead)
            return;
        if (health <= 0 && !indestructible)
        {
            dead = true;
            OnDead?.Invoke();
            FX_Spawner.Instance?.SpawnFX(destroyFX, transform.position, transform.up);
            if (destroyer != null) {
                destroyer.GETTEM();
            }
            else {
                Destroy(gameObject);
            }
        }
    }

    public void Hit(HitObject hit) {
        // reduce health
        if (hit.ignoreMass) {
            hit.knockback *= rb.mass;
        }
        if (rb != null) {
            rb.AddForceAtPosition(hit.direction * hit.knockback, hit.position, ForceMode2D.Impulse);
        }
        OnHit?.Invoke(hit);
        FX_Spawner.Instance?.SpawnFX(hitFX, hit.position, -hit.direction);
        UpdateHealth(-hit.damage);
    }

    public void UpdateHealth(float amount) {
        health = Mathf.Clamp(health+amount, 0, maxHealth);
        if (!damaged && (health > 0 || indestructible) && health <= maxHealth * damagedThreshold)
        {
            Damage();
        }
        OnHealthUpdated?.Invoke(health);
    }

    protected virtual void Damage() {
        if (damagedSprite != null) {
            var r = GetComponentInChildren<SpriteRenderer>();
            if (r != null)
                r.sprite = damagedSprite;
            FX_Spawner.Instance?.SpawnFX(damagedFX, transform.position, Quaternion.identity);
        }
        damaged = true;
    }
}
