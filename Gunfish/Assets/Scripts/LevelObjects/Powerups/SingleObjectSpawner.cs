using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleObjectSpawner : Spawner
{
    GameObject spawnedObject;

    protected override GameObject Spawn() {
        active = false;
        spawnedObject = base.Spawn();
        return spawnedObject.gameObject;
    }

    protected override void Update() {
        base.Update();
        active = spawnedObject == null;
    }
}