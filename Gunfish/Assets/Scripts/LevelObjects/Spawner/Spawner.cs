using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public List<PickUpTuple> spawnPrefabProbabilityList = new List<PickUpTuple>();
    Dictionary<GameObject, float> spawnPrefabProbabilityMap = new Dictionary<GameObject, float>();

    public GameObject spawnPrefab;

    public bool useSpawnTimerOnStart;
    //[HideInInspector]
    public float spawnTimer;
    public Vector2 spawnTimerRange;

    public Collider2D spawnArea;

    public bool active = true;

    protected virtual void Start() {
        // from 0 to max
        spawnTimer = (useSpawnTimerOnStart) ? spawnTimer : Random.Range(0, spawnTimerRange.y);
        spawnArea = spawnArea ?? GetComponent<Collider2D>();
        if (spawnArea == null) {
            spawnArea = gameObject.AddComponent<BoxCollider2D>();
            spawnArea.isTrigger = true;
        }
        foreach (var pickUpTuple in spawnPrefabProbabilityList) {
            spawnPrefabProbabilityMap[pickUpTuple.pickup] = pickUpTuple.probability;
        }
        //print(spawnArea);
    }

    // Update is called once per frame
    protected virtual void Update() {
        // decrement spawnTimer
        // if 0, spawn the thing
        // subscribe the thing being picked up
        if (!active)
            return;
        UpdateSpawn();
    }

    protected virtual void UpdateSpawn() {
        if (spawnTimer >= 0) {
            spawnTimer -= Time.deltaTime;
        }
        if (spawnTimer < 0) {
            Spawn();
        }
    }

    protected virtual GameObject Spawn() {
        spawnTimer = spawnTimerRange.RandomInRange();
        return Instantiate(
            spawnPrefabProbabilityMap.Choose(spawnPrefab),
            spawnArea.bounds.RandomPointInBounds(),
            Quaternion.identity);
    }
}