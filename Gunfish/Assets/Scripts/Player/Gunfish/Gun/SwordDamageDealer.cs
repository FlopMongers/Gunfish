using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordDamageDealer : CollisionDamageDealer
{

    public Gunfish gunfish;

    protected override void Start() {
        base.Start();
    }

    protected override void HandleCollisionEnter(GameObject src, Collision2D collision) {
        // check if src is sword and target is not self
        if (src != collisionDetector.gameObject || collision.collider.GetComponent<GunfishSegment>()?.gunfish == gunfish) {
            return;
        }
        print($"{src}, {collision.rigidbody}");
        trace = true;
        base.HandleCollisionEnter(src, collision);
    }
}
