using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperLaser : MonoBehaviour
{
    public LineRenderer laser;
    public float range;
    int layerMask;

    // Start is called before the first frame update
    void Start()
    {
        laser = laser ?? GetComponentInChildren<LineRenderer>();
        layerMask = LayerMask.GetMask("Player1", "Player2", "Player3", "Player4", "Ground", "Default", "Floaty");
    }

    // Update is called once per frame
    void Update()
    {
        laser.SetPosition(0, transform.position);
        // fire laser from tip until you run into somethin'.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, range, layerMask);
        if (hit.collider == null) {
            laser.SetPosition(1, transform.position + (transform.up * range));
        }
        else {
            laser.SetPosition(1, hit.point);
        }
    }
}
