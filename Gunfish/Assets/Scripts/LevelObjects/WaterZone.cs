using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class WaterZone : MonoBehaviour
{
    public FishDetector detector;

    // Start is called before the first frame update
    void Start()
    {
        if (detector == null)
            detector = GetComponent<FishDetector>();

        detector.OnFishTriggerEnter += OnFishEnter;
        detector.OnFishTriggerExit += OnFishExit;
    }

    void OnFishEnter(GunfishSegment segment, Collider2D collider) {
        segment.gunfish.AddEffect(new Underwater_Effect(segment.gunfish));
    }

    void OnFishExit(GunfishSegment segment, Collider2D collider) {
        segment.gunfish.AddEffect(new Underwater_Effect(segment.gunfish, -1));
    }
}
