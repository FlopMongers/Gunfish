using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : Spawner
{
    PowerUp spawnedPowerUp;

    protected override GameObject Spawn() {
        active = false;
        spawnedPowerUp = base.Spawn().GetComponent<PowerUp>();
        spawnedPowerUp.detector.OnFishTriggerEnter += OnFishPickUp;
        return spawnedPowerUp.gameObject;
    }

    public void OnFishPickUp(GunfishSegment segment, Collider2D collider) {
        // unsubscribe and restart timer
        active = true;
        spawnedPowerUp.detector.OnFishTriggerEnter -= OnFishPickUp;
        spawnTimer = Random.Range(spawnTimerRange.x, spawnTimerRange.y);
    }
}