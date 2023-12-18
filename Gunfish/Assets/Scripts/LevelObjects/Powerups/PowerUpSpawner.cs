using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject powerUpPrefab;
    PowerUp spawnedPowerUp;
    public Collider2D spawnArea;

    [HideInInspector]
    public float spawnTimer;
    public Vector2 spawnTimerRange;

    // Start is called before the first frame update
    void Start()
    {
        // from 0 to max
        spawnTimer = Random.Range(0, spawnTimerRange.y);
        spawnArea = spawnArea ?? GetComponent<Collider2D>() ?? gameObject.AddComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // decrement spawnTimer
        // if 0, spawn the thing
        // subscribe the thing being picked up
        if (spawnTimer > 0) {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer < 0) {
                spawnedPowerUp = Instantiate(powerUpPrefab, transform.position, Quaternion.identity).GetComponent<PowerUp>();
                spawnedPowerUp.detector.OnFishTriggerEnter += OnFishPickUp;
            }
        }
    }

    public void OnFishPickUp(GunfishSegment segment, Collider2D collider) {
        // unsubscribe and restart timer
        spawnedPowerUp.detector.OnFishTriggerEnter -= OnFishPickUp;
        spawnTimer = Random.Range(spawnTimerRange.x, spawnTimerRange.y);
    }
}
