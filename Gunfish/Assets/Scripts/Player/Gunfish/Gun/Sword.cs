using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(SwordDamageDealer))]
public class Sword : Gun
{
    // Start is called before the first frame update
    protected void Start()
    {
        var damageDealer = GetComponent<SwordDamageDealer>();
        if (damageDealer.collisionDetector == null)
            damageDealer.SetupCollisionDetector(gunfish.RootSegment.GetComponent<CompositeCollisionDetector>());
        damageDealer.gunfish = gunfish;
    }

    protected override void _Fire() {
        // don't do anything?
    }
}
