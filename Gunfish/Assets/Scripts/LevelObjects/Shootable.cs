using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shootable : MonoBehaviour
{
    public float health;
    public FloatGameEvent OnHealthUpdated;

    public FXType destroyFX, hitFX;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health < 0)
        {
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
        UpdateHealth(hit.damage);
    }

    public void UpdateHealth(float amount)
    {
        health += amount;
        OnHealthUpdated?.Invoke(health);
    }
}
