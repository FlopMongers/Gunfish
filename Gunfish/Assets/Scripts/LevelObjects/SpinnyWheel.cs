using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinnyWheel : MonoBehaviour
{
    public Rigidbody2D rb;

    public float angularVelocity;

    // Start is called before the first frame update
    void Start()
    {
        rb  = rb ?? GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.angularVelocity = angularVelocity;
    }

    public void ReverseDir() {
        angularVelocity *= -1;
    }
}
