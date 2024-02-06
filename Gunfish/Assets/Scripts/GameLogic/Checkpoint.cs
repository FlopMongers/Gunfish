using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class Checkpoint : SpawnArea
{
    public CheckpointEvent fishEnterEvent;

    public FishDetector detector;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        // hook up to detector
        detector.OnFishTriggerEnter += OnFishEnter;
    }

    void OnFishEnter(GunfishSegment segment, Collider2D collision) {
        fishEnterEvent?.Invoke(segment.gunfish, this);
    }
}
