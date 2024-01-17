using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PelicanSpawner : Spawner
{

    // distance the pelican flies, can be based on longest distance between colliders?
    public float distance;

    public Vector2 originalSpeedRange = new Vector2();
    Vector2 pelicanSpeedRange = new Vector2();
    public float pelicanSpeedRangeIncreaseRate = 1f;
    public float pelicanSpeedRangeLimit = 30f;

    public Vector2 originalSpawnTimerRange = new Vector2();
    public float spawnTimerRangeDecreaseRate = 1f;
    public float spawnTimerRangeLowerLimit = 1f;

    // when you spawn a pelican, give them a start position and end position
    List<Collider2D> spawnZones = new List<Collider2D>();

    protected override void Start() {
        base.Start();
        active = false;
    }

    protected override void UpdateSpawn() {
        base.UpdateSpawn();
        if (pelicanSpeedRange.x < pelicanSpeedRangeLimit) {
            pelicanSpeedRange += Vector2.one * pelicanSpeedRangeIncreaseRate * Time.deltaTime;
        }
        if (spawnTimerRange.y > spawnTimerRangeLowerLimit) {
            spawnTimerRange -= Vector2.one * spawnTimerRangeDecreaseRate * Time.deltaTime;
        }
    }

    public void FetchSpawnZones() {
        spawnZones = new List<Collider2D>();
        spawnTimerRange = originalSpawnTimerRange;
        pelicanSpeedRange = originalSpeedRange;
        Vector3 maxPosition = Vector3.zero;
        Vector3 minPosition = Vector3.zero;
        foreach (var killbox in FindObjectsOfType<KillBox>()) {
            var coll = killbox.gameObject.GetComponent<Collider2D>();
            spawnZones.Add(coll);
            maxPosition = Vector3.Max(coll.bounds.max, maxPosition);
            minPosition = Vector3.Min (coll.bounds.min, minPosition);
        }
        distance = Vector3.Distance(maxPosition, minPosition);
    }

    protected override GameObject Spawn() {
        if (spawnZones.Count == 0) {
            return null;
        }

        List<Transform> targets = new List<Transform>();
        bool noFish = true;
        foreach (var player in GameModeManager.Instance.matchManagerInstance.parameters.activePlayers) {
            Gunfish gunfish = player.Gunfish;
            if (gunfish.RootSegment != null) {
                noFish = false;
                targets.Add(gunfish.RootSegment.transform);
            }
        }
        if (noFish == true) {
            targets.Add(transform);
            //return null;
        }
        Pelican pelican = base.Spawn().GetComponent<Pelican>();
        pelican.transform.position = spawnZones[Random.Range(0, spawnZones.Count)].bounds.RandomPointInBounds();
        pelican.endPosition = (targets[Random.Range(0, targets.Count)].position - pelican.transform.position).normalized * distance;
        pelican.pelicanSpeedRange = pelicanSpeedRange;
        pelican.zoomin = true;
        return pelican.gameObject;
    }
}
