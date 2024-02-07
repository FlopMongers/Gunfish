using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public class RevUp : MonoBehaviour
{
    public AudioSource warmupSound;
    public GameObject warmupParticlesPrefab;
    ParticleSystem warmupParticles;
    public Vector2 volRange;
    public Vector2 pitchRange;

    TweenerCore<float, float, FloatOptions> fadeTween, pitchTween;

    public void SetWarmupParticles(bool turnOn, Transform barrel, float fadeTime) {
        if (warmupParticles == null) {
            var warmupInstance = Instantiate(warmupParticlesPrefab, barrel.position, Quaternion.LookRotation(Vector3.forward, barrel.right));
            warmupInstance.transform.parent = barrel;
            warmupParticles = warmupInstance.GetComponent<ParticleSystem>();
        }
        if (turnOn) {
            warmupParticles.Play();
            fadeTween = warmupSound.DOFade(volRange.y, fadeTime);
            pitchTween = warmupSound.DOPitch(pitchRange.y, fadeTime);
        }
        else {
            if (fadeTween != null) {
                fadeTween.Kill();
            }
            if (pitchTween != null) {
                pitchTween.Kill();
            }
            warmupParticles.Stop();
            warmupSound.DOFade(volRange.x, fadeTime);
            warmupSound.DOPitch(pitchRange.x, fadeTime);
        }
    }
}
