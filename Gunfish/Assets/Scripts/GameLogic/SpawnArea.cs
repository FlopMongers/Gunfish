using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    public int spawnPointOrder;
    public List<Transform> spawnPoints = new List<Transform>();
    int lastSpawnPoint;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        lastSpawnPoint = Random.Range(0, spawnPoints.Count);
    }

    public Transform GetNextSpawnPoint() {
        Transform nextSpawnPoint = spawnPoints[lastSpawnPoint];
        lastSpawnPoint = (lastSpawnPoint + 1) % spawnPoints.Count;
        return nextSpawnPoint;
    }
}
