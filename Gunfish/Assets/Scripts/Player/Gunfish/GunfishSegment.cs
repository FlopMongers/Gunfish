using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishSegment : ObjectMaterial {

    [HideInInspector]
    public Gunfish gunfish;

    [HideInInspector]
    public int index;

    [HideInInspector]
    public Rigidbody2D rb;

    public bool isGun;
    public int isUnderwater;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetUnderwater(int underwater) {
        // if gun and 

        bool change = (isUnderwater == 1 && underwater == -1) || (isUnderwater == 0 && underwater == 1);
        isUnderwater += underwater;
        if (change) {
            if (isGun) {
                /*if (isUnderwater <= 0) {
                    if (Vector3.Angle(Vector2.up, rb.velocity) < gunfish.data.waterZoomAngleThreshold && rb.velocity.magnitude > gunfish.data.waterZoomSpeedThreshold) {
                        gunfish.body.ApplyForceToSegment(index, ((rb.velocity.normalized + Vector2.up) / 2) * gunfish.data.waterZoomForce, ForceMode2D.Impulse);
                    }
                    // if upwards velocity is high enough, then LAUNCH the fish
                }*/
                // if gun, set the gunfish underwater
                gunfish.underwater = isUnderwater == 1;
            }
            rb.gravityScale = (isUnderwater == 1) ? 0f : 1f;
            rb.drag += (isUnderwater == 1) ? 1f : -1f;
        }
    }
}