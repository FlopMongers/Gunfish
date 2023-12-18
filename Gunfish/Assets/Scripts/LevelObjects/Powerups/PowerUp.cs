using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class PowerUp : MonoBehaviour
{

    public FishDetector detector;
    public FXType PickupFX;

    void Start() {
        detector = detector ?? GetComponent<FishDetector>();
        detector.OnFishTriggerEnter += OnFishEnter;
    }

    public void OnFishEnter(GunfishSegment segment, Collider2D collider) {
        PickUp(segment.gunfish);
    }

    public virtual void PickUp(Gunfish gunfish) {
        // todo: play some effect
        FX_Spawner.Instance.SpawnFX(PickupFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
