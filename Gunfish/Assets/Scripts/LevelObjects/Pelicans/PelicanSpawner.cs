using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PelicanSpawner : Spawner
{

    // distance the pelican flies, can be based on longest distance between colliders?
    public float distance;

    // when you spawn a pelican, give them a start position and end position
    List<Collider2D> spawnZones = new List<Collider2D>();

    protected override void Start() {
        base.Start();
        active = false;
    }

    public void FetchSpawnZones() {
        spawnZones = new List<Collider2D>();
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

        // TODO INCREASE PELICAN SPAWN RATE AND SPEED OVER TIME

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
        pelican.zoomin = true;
        return pelican.gameObject;
    }
}
