using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    bool destroying;

    public FXType destroyFX = FXType.Poof;
    public SpriteRenderer spriteRenderer;

    float barticleToPoofRatio = 0.5f;
    float barticleExplosionForce = 5f;

    public bool destroyFromStart;
    public Vector2 destroyDelay;

    private void Start() {
        if (destroyFromStart) {
            Invoke("ActuallyGettem", destroyDelay.RandomInRange());
        }
    }

    public void GETTEM(float timer=-1) {
        if (timer < 0) {
            ActuallyGettem();
        }
        else {
            StartCoroutine(CoGETTEM(timer));
        }
    }

    IEnumerator CoGETTEM(float timer) {
        yield return new WaitForSeconds(timer);
        ActuallyGettem();
    }

    // NOTE(Wyatt): I know these names are moronic. Remember the KISS principle (keep it stupid, stupid)
    void ActuallyGettem() {
        if (destroying)
            return;
        destroying = true;

        if (spriteRenderer != null && FX_Spawner.Instance != null) {
            var fx = FX_Spawner.Instance.SpawnFX(destroyFX, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            // get bounds
            var shape = fx.shape;
            float shortSide = Mathf.Min(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y);
            float longSide = Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y);
            shape.scale = Vector3.one * shortSide;
            var em = fx.emission;
            em.burstCount *= Mathf.RoundToInt((spriteRenderer.bounds.size.x * spriteRenderer.bounds.size.y) + 1 - (longSide / shortSide));
            for (int i = 0; i < em.burstCount * barticleToPoofRatio; i++) {
                var barticle = Instantiate(FX_Spawner.Instance.barticles.GetRandom(), spriteRenderer.bounds.RandomPointInBounds(), Quaternion.identity);
                barticle.transform.FindDeepChild("Mask").transform.localScale *= shortSide;
                var barticleRenderer = barticle.GetComponentInChildren<SpriteRenderer>();
                barticleRenderer.sprite = spriteRenderer.sprite;
                barticleRenderer.transform.position = barticleRenderer.bounds.RandomPointInBounds();
                barticleRenderer.transform.Rotate(Vector3.forward * Random.Range(0, 360));
                barticle.GetComponent<Rigidbody2D>().AddExplosionForce(barticleExplosionForce, transform.position, shortSide);
                var barticleFader = barticle.GetComponent<Fader>();
                //barticleFader.baseFadeSpeed = 0.5f;
                barticleFader.FadeAndDestroy();
            }
            // spawn barticles
            // scale up barticles (*= shortSide)
            // apply sprite texture
            // offset sprite randomly and apply rotation
            // apply explosive force
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
            fader.FadeAndDestroy();
        }
        else {
            Destroy(gameObject);
        }
    }
}
