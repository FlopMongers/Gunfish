using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class WaterZone : MonoBehaviour
{
    public FishDetector detector;

    public WaterMaterialInterface waterMaterial;

    // Start is called before the first frame update
    void Start()
    {
        if (detector == null)
            detector = GetComponent<FishDetector>();

        detector.OnFishTriggerEnter += FishEnterSploosh;
        detector.OnFirstSegmentExit += FishExitSploosh;
    }

    Vector2 forceRange = new Vector2(0f, 10f);
    float forceScale = 1f;

    void PerturbNode(int nodeIdx, Vector2 force) {
        if (nodeIdx == waterMaterial.waterSurfaceNodes.Count - 1)
            return;
        waterMaterial.waterSurfaceNodes[nodeIdx].GetComponent<WaterSurfaceNode>().Sploosh(force);
    }

    void Sploosh(Vector3 position, float force, bool up) {
        if (force < forceRange.x)
            return;
        force *= forceScale;
        force = Mathf.Clamp(force, forceRange.x, forceRange.y);
        Vector2 dir = (up) ? Vector2.up: Vector2.down;
        int nodeIdx = PiecewiseLinear.ClosestIndexBefore(
            waterMaterial.waterSurfaceNodes, position.x, PiecewiseLinear.transformPosition, true);
        PerturbNode(nodeIdx, dir * force);
        PerturbNode(nodeIdx + 1, dir * force);
        // TODO: splash FX
    }

    void FishEnterSploosh(GunfishSegment segment, Collider2D collider) {
        Sploosh(segment.transform.position, segment.rb.velocity.magnitude, false);
    }

    void FishExitSploosh(GunfishSegment segment, Collider2D collider) {
        Sploosh(segment.transform.position, segment.rb.velocity.magnitude, true);
    }

    public void OnTriggerEnter2D(Collider2D other) {
        // if fish segment, tell the segment to be underwater
        var fishSegment = other.gameObject.GetComponent<GunfishSegment>();
        if (fishSegment != null) {
            fishSegment.SetUnderwater(1);
        }
        else if (other.GetComponentInParent<Rigidbody2D>() != null) {
            print("SPLOOSH DOWN!!");
            print(other.name);
            Sploosh(other.transform.position, other.GetComponentInParent<Rigidbody2D>().velocity.magnitude, false);
        }
    }

    public void OnTriggerExit2D(Collider2D other) {
        // if fish, set not underwater
        var fishSegment = other.gameObject.GetComponent<GunfishSegment>();
        if (fishSegment != null) {
            fishSegment.SetUnderwater(-1);
        }
        else if (other.GetComponentInParent<Rigidbody2D>() != null) {
            print("SPLOOSH UP!!!");
            print(other.name);
            Sploosh(other.transform.position, other.GetComponentInParent<Rigidbody2D>().velocity.magnitude, true);
        }
    }
}
