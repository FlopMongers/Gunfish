using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class WaterZone : MonoBehaviour {
    public FishDetector detector;

    public WaterMaterialInterface waterMaterial;

    public Vector2 splashThresholdRange = new Vector2(5, 10);
    public Vector2 splashVolumeRange = new Vector2(0.6f, 1f);

    public FXType splashType = FXType.Splash;

    public  Dictionary<Shootable, int> submergedShootables = new Dictionary<Shootable, int>();

    // Start is called before the first frame update
    protected virtual void Start() {
        if (detector == null)
            detector = GetComponent<FishDetector>();

        detector.OnFishTriggerEnter += FishEnterSploosh;
        detector.OnFirstSegmentTriggerExit += FishExitSploosh;
    }

    Vector2 forceRange = new Vector2(0f, 10f);
    float forceScale = 1f;

    void PerturbNode(int nodeIdx, Vector2 force) {
        if (nodeIdx < 0 || nodeIdx >= waterMaterial.waterSurfaceNodes.Count - 1)
            return;
        waterMaterial.waterSurfaceNodes[nodeIdx].GetComponent<WaterSurfaceNode>().Sploosh(force);
    }

    public void Sploosh(Vector3 position, float force, bool up) {
        if (force < forceRange.x)
            return;
        force *= forceScale;
        force = Mathf.Clamp(force, forceRange.x, forceRange.y);
        Vector2 dir = (up) ? Vector2.up : Vector2.down;
        int nodeIdx = PiecewiseLinear.ClosestIndexBefore(
            waterMaterial.waterSurfaceNodes, position.x, PiecewiseLinear.transformPosition, true);
        PerturbNode(nodeIdx, dir * force);
        PerturbNode(nodeIdx + 1, dir * force);
        if (force > splashThresholdRange.x) {
            float normalizedForce = ExtensionMethods.GetNormalizedValueInRange(force, splashThresholdRange.x, splashThresholdRange.y);
            var splash = FX_Spawner.Instance.SpawnFX(
                splashType, position, Quaternion.identity, Mathf.Lerp(splashVolumeRange.x, splashVolumeRange.y, normalizedForce)).GetComponent<SplashEffect>();
            splash.SetSplashPower(normalizedForce);
        }
    }

    void FishEnterSploosh(GunfishSegment segment, Collider2D collider) {
        Sploosh(segment.transform.position, segment.rb.velocity.magnitude, false);
    }

    void FishExitSploosh(GunfishSegment segment, Collider2D collider) {
        Sploosh(segment.transform.position, segment.rb.velocity.magnitude, true);
    }

    public virtual void OnTriggerEnter2D(Collider2D other) {
        // if fish segment, tell the segment to be underwater
        if (other.isTrigger == true)
            return;

        var shootable = other.gameObject.GetComponentInParent<Shootable>();
        if (shootable != null) {
            if (!submergedShootables.ContainsKey(shootable)) {
                submergedShootables[shootable] = 0;
            }
            submergedShootables[shootable]++;
        }

        var fishSegment = other.gameObject.GetComponent<GunfishSegment>();
        if (fishSegment != null) {
            if (fishSegment.isGun && !fishSegment.gunfish.underwater) {
                //if (fishSegment.GetComponent<Rigidbody2D>().velocity.y < -5)
                FX_Spawner.Instance.SpawnFX(FXType.Bubbles, fishSegment.transform.position, Quaternion.identity, 0.1f, fishSegment.transform);
            }
            fishSegment.SetUnderwater(1);
        }
        var waterInteractor = other.gameObject.GetComponentInParent<WaterInteractor>();
        if (waterInteractor != null) {
            waterInteractor.SetUnderwater(1);
        } // TODO: change gunfish segment to just use a water interactor!
        if (other.GetComponentInParent<Rigidbody2D>() != null) {
            Sploosh(other.transform.position, other.GetComponentInParent<Rigidbody2D>().velocity.magnitude, false);
        }
    }

    public virtual void OnTriggerExit2D(Collider2D other) {
        if (other.isTrigger)
            return;

        var shootable = other.gameObject.GetComponentInParent<Shootable>();
        if (shootable != null && submergedShootables.ContainsKey(shootable)) {
            submergedShootables[shootable]--;
            if (submergedShootables[shootable] <= 0) {
                submergedShootables.Remove(shootable);
            }
        }

        // if fish, set not underwater
        var fishSegment = other.gameObject.GetComponent<GunfishSegment>();
        if (fishSegment != null) {
            fishSegment.SetUnderwater(-1);
        }
        var waterInteractor = other.gameObject.GetComponentInParent<WaterInteractor>();
        if (waterInteractor != null) {
            waterInteractor.SetUnderwater(-1);
        }
        else if (other.GetComponentInParent<Rigidbody2D>() != null) {
            Sploosh(other.transform.position, other.GetComponentInParent<Rigidbody2D>().velocity.magnitude, true);
        }
    }
}