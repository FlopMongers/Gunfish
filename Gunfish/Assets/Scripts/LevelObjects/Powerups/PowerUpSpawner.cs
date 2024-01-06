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
        spawnedPowerUp.OnPowerUpGone += RestartSpawner;
        return spawnedPowerUp.gameObject;
    }

    public void OnFishPickUp(GunfishSegment segment, Collider2D collider) {
        RestartSpawner();
    }

    void RestartSpawner() {
        // unsubscribe and restart timer
        active = true;
        if (spawnedPowerUp != null) {
            spawnedPowerUp.detector.OnFishTriggerEnter -= OnFishPickUp;
            spawnedPowerUp.OnPowerUpGone -= RestartSpawner;
        }
        spawnTimer = Random.Range(spawnTimerRange.x, spawnTimerRange.y);
    }
}