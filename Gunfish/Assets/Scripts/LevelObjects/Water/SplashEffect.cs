using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashEffect : MonoBehaviour
{
    public ParticleSystem splashFX;

    public Vector2 particleNumberRange = new Vector2(3, 10);
    // min, max, variance percent
    public Vector3 particleSpeedRange = new Vector3(1.5f, 3f, 0.1f);
    public Vector3 particleSizeRange = new Vector3(0.1f, 0.5f, 0.1f);

    // power ranges from 0 to 1
    public void SetSplashPower(float power) {
        // set number, size and velocity of particles
        var em = splashFX.emission;
        em.burstCount = Mathf.RoundToInt(Mathf.Lerp(particleNumberRange.x, particleNumberRange.y, power));
        var main = splashFX.main;
        float size = Mathf.Lerp(particleSizeRange.x, particleSizeRange.y, power);
        main.startSize = new ParticleSystem.MinMaxCurve(size - (size * particleSizeRange.z), size + (size * particleSizeRange.z));
        float speed = Mathf.Lerp(particleSpeedRange.x, particleSpeedRange.y, power);
        main.startSpeed = new ParticleSystem.MinMaxCurve(size - (size * particleSpeedRange.z), size + (size * particleSpeedRange.z));
        splashFX.Play();
    }
}
