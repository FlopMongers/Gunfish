using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class Checkpoint : MonoBehaviour
{
    public int checkpointOrder;
    public CheckpointEvent fishEnterEvent;

    public FishDetector detector;

    public List<Transform> spawnPoints = new List<Transform>();
    int lastSpawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        // hook up to detector
        detector.OnFishTriggerEnter += OnFishEnter;
        lastSpawnPoint = Random.Range(0, spawnPoints.Count);
    }

    void OnFishEnter(GunfishSegment segment, Collider2D collision) {
        fishEnterEvent?.Invoke(segment.gunfish, this);
    }

    public Transform GetNextSpawnPoint() {
        Transform nextSpawnPoint = spawnPoints[lastSpawnPoint];
        lastSpawnPoint = (lastSpawnPoint+1) % spawnPoints.Count;
        return nextSpawnPoint;
    }
}
