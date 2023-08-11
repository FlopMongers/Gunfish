using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class GroundMaterial : ObjectMaterial
{

    // add start method to initialize fish detector
    public void Start()
    {
        var detector = GetComponent<FishDetector>();
        detector.OnFishCollideEnter += HandleFishCollisionEnter;
        detector.OnFishCollideExit += HandleFishCollisionExit;
    }

    protected virtual void HandleFishCollisionEnter(GunfishSegment segment, Collision2D collision)
    {

    }

    protected virtual void HandleFishCollisionExit(GunfishSegment segment, Collision2D collision)
    {

    }

}
