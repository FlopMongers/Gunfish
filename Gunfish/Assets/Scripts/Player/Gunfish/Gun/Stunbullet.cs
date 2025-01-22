using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Stunbullet : Bullet
{
    public GameObject zapFX;

    // NOTE(Wyatt): this is passed by reference
    public Dictionary<Gunfish, float> fishZapMap = new Dictionary<Gunfish, float>();

    protected override void Start() {
        base.Start();
    }

    protected override void Gettem() {
        if (destroyed)
            return;
        base.Gettem();
        // spawn zap, pass the zap map and the ignore fishes
        var zap = Instantiate(zapFX, transform.position, Quaternion.identity).GetComponent<Zap>();
        zap.zappedFishes.Add(gunfish);
        if (gunfish != null && gunfish.gameObject != null) { 
            zap.owner = gunfish.gameObject;
        }
        zap.fishZapMap = fishZapMap;
    }
}
