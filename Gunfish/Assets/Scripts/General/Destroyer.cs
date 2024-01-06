using SolidUtilities.UnityEngineInternals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    bool destroying;

    public FXType destroyFX = FXType.Poof;
    public SpriteRenderer spriteRenderer;

    public void GETTEM() {
        if (destroying)
            return;
        destroying = true;

        if (spriteRenderer != null) {
            var fx = FX_Spawner.Instance.SpawnFX(destroyFX, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            // get bounds
            var shape = fx.shape;
            float shortSide = Mathf.Min(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y);
            float longSide = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y);
            shape.scale = Vector3.one * shortSide;
            var em = fx.emission;
            em.burstCount *= Mathf.RoundToInt((spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y)+1 - (longSide / shortSide));
            fx.Play();
        }

        foreach (var rb in GetComponentsInChildren<Rigidbody2D>()) {
            rb.gravityScale = 0f;
        }
        foreach (var coll in GetComponentsInChildren<Collider2D>()) {
            coll.enabled = false;
        }
        var fader = gameObject.GetComponent<Fader>();
        if (fader != null) {
            fader.SetTarget(new Vector2(1, 0));
            fader.OnFadeDone += delegate { Destroy(gameObject); };
        }
        else {
            Destroy(gameObject);
        }
    }
}
