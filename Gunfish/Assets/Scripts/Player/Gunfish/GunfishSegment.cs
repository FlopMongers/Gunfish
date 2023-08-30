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
    public bool isUnderwater;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetUnderwater(bool underwater) {
        if (isUnderwater != underwater) {
            if (isGun) {
                // if gun, set the gunfish underwater
                gunfish.underwater = underwater;
            }
            rb.gravityScale = (underwater) ? 0f : 1f;
            rb.drag += (underwater) ? 1f : -1f;
            isUnderwater = underwater;
        }
    }
}
