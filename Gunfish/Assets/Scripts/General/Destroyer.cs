using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    bool destroying;

    public void GETTEM() {
        if (destroying)
            return;
        destroying = true;
        foreach (var rb in GetComponentsInChildren<Rigidbody2D>()) {
            rb.gravityScale = 0f;
        }
        foreach (var coll in GetComponentsInChildren<Collider2D>()) {
            coll.enabled = false;
        }
        var fader = gameObject.GetComponent<Fader>();
        if (fader != null) {
            fader.SetTarget(new Vector2(1, 0));
            fader.OnFadeDone += delegate { Destroy(gameObject); };
        }
        else {
            Destroy(gameObject);
        }
    }
}
