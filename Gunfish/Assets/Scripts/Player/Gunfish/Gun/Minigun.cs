using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigun : AutomaticGun
{
    // NOTE(Wyatt): since there will only be one minigun, I don't think we really need to make this a scriptable object yet.
    public float fireWarmUpTime = 2f;
    float fireWarmUpTimer;

    public AudioSource warmupSound;
    public GameObject warmupParticlesPrefab;
    ParticleSystem warmupParticles;
    public Vector2 volRange;
    public Vector2 pitchRange;

    // NOTE(Wyatt): maybe miniguns and lasers should have a constantly playing audio source that we just turn up or down depending on whether they're warming up?

    protected override bool CheckButtonStatus(ButtonStatus firingStatus) {
        bool result = base.CheckButtonStatus(firingStatus);
        // if pressed, start the timer
        if (firingStatus == ButtonStatus.Pressed) {
            fireWarmUpTimer = fireWarmUpTime;
            // start playing warm up sound
            // turn up warm up sound
            SetWarmupParticles(true);
            warmupSound.DOFade(volRange.y, fireWarmUpTime);
            warmupSound.DOPitch(pitchRange.y, fireWarmUpTime);
        }
        // if held, decrement the timer (play fx)
        else if (firingStatus == ButtonStatus.Holding && fireWarmUpTimer > 0 && reload_timer <= 0 && fireCooldown_timer <= 0) {
            
            fireWarmUpTimer -= Time.deltaTime;
        }
        if ((result && fireWarmUpTimer <= 0) || !result) {
            SetWarmupParticles(false);
            warmupSound.DOFade(volRange.x, 0.2f);
            warmupSound.DOPitch(pitchRange.x, 0.2f);
        }

        // else if playing warm up sound
        // stop playing warm up sound
        // if result and timer <= 0
        return result && fireWarmUpTimer <= 0;
    }

    void SetWarmupParticles(bool turnOn) {
        if (warmupParticles == null) {
            Transform barrel = gunfish.gun.barrels[0].transform;
            var warmupInstance = Instantiate(warmupParticlesPrefab, barrel.position, Quaternion.LookRotation(Vector3.forward, barrel.right));
            warmupInstance.transform.parent = barrel;
            warmupParticles = warmupInstance.GetComponent<ParticleSystem>();
        }
        if (turnOn) {
            warmupParticles.Play();
        }
        else {
            warmupParticles.Stop();
        }
    } 
}
