using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    Dictionary<Rigidbody2D, int> rbMap = new Dictionary<Rigidbody2D, int>();
    Dictionary<Rigidbody2D, Transform> rbParentMap = new Dictionary<Rigidbody2D, Transform>();
    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = rb ?? GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.right * 0.1f;
    }

    // get all objects with rigidbodies in zone
    // apply
    private void OnTriggerEnter2D(Collider2D collision) {
        var r = collision.attachedRigidbody;
        if (r == null)
            return;
        if (rbMap.ContainsKey(r) == false) {
            rbParentMap[r] = r.transform.parent;
            r.transform.parent = transform;
            rbMap[r] = 0;
        }
        rbMap[r]++;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        var r = collision.attachedRigidbody;
        if (r == null)
            return;
        if (rbMap.ContainsKey(r) == false)
            return;
        rbMap[r]--;
        if (rbMap[r] <= 0) {
            r.transform.parent = rbParentMap[r];
            rbParentMap.Remove(r);
            rbMap.Remove(r);
        }
    }
}
