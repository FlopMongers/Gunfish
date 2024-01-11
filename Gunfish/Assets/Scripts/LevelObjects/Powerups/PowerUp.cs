using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
[RequireComponent(typeof(Fader))]
public class PowerUp : MonoBehaviour
{
    public float maxLifetime = 15;
    public float threshold = 5;
    float lifetimer;

    public FishDetector detector;
    public FXType PickupFX;

    public Fader fader;
    bool fading;

    static Vector2 fadeRange = new Vector2(0.2f, 0.75f);

    public GameEvent OnPowerUpGone;

    void Start() {
        detector = detector ?? GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += OnFishEnter;
        lifetimer = maxLifetime;
        fader = fader ?? gameObject.CheckAddComponent<Fader>();
    }

    public void OnFishEnter(GunfishSegment segment, Collider2D collider) {
        PickUp(segment.gunfish);
    }

    public virtual void PickUp(Gunfish gunfish) {
        // todo: play some effect
        FX_Spawner.Instance.SpawnFX(PickupFX, transform.position, Quaternion.identity);
        OnPowerUpGone?.Invoke();
        Destroy(gameObject);
    }

    public virtual void Update() {
        if (maxLifetime < 0)
            return;
        lifetimer -= Time.deltaTime;
        if (lifetimer <= 0) {
            fader.SetTarget(Vector2.right, FadeMode.Fade);
            fader.OnFadeDone += delegate { OnPowerUpGone?.Invoke(); Destroy(gameObject); };
        }
        else if (fading == false && lifetimer < threshold) {
            fading = true;
            fader.SetTarget(fadeRange, FadeMode.PingPong);
        }
    }


}
