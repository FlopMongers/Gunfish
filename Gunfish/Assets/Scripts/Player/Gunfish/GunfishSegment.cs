using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunfishSegment : ObjectMaterial
{

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
        bool change = (isUnderwater == 1 && underwater == -1) || (isUnderwater == 0 && underwater == 1);
        isUnderwater += underwater;
        if (change) {
            if (isGun) {
                // if gun, set the gunfish underwater
                gunfish.underwater = isUnderwater == 1;
            }
            rb.gravityScale = (isUnderwater == 1) ? 0f : 1f;
            rb.drag += (isUnderwater == 1) ? 1f : -1f;
        }
    }
}
