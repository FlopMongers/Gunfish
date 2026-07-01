using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachBall : MonoBehaviour
{
    public Rigidbody2D rb;
    public Bounds bounds;
    public Vector3 spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!bounds.Contains(rb.position)) {
            rb.position = spawnPoint;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0.0f;
        }
    }
}
