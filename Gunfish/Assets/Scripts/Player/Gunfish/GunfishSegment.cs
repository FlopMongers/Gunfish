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

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
}
