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

    [HideInInspector]
    public List<Gunfish> gunfishes = new List<Gunfish>();

    protected override void Start() {
        base.Start();
        active = false;
        // initialize spawn zones
        gunfishes = FindObjectsOfType<Gunfish>().ToList();
    }

    public void FetchSpawnZones() {
        spawnZones = new List<Collider2D>();
        foreach (var killbox in FindObjectsOfType<KillBox>()) {
            spawnZones.Add(killbox.gameObject.GetComponent<Collider2D>());
        }
    }

    protected override GameObject Spawn() {
        if (spawnZones.Count == 0) {
            return null;
        }
        List<Transform> targets = new List<Transform>();
        foreach (var gunfish in gunfishes) {
            bool noFish = true;
            if (gunfish.segments.Count > 0) {
                noFish = false;
                targets.Add(gunfish.RootSegment.transform);
            }
            if (noFish) {
                return null;
            }
        }
        Pelican pelican = base.Spawn().GetComponent<Pelican>();
        pelican.transform.position = spawnZones[Random.Range(0, spawnZones.Count)].bounds.RandomPointInBounds();
        pelican.endPosition = (targets[Random.Range(0, targets.Count)].position - pelican.transform.position).normalized * distance;
        pelican.zoomin = true;
        return pelican.gameObject;
    }
}
