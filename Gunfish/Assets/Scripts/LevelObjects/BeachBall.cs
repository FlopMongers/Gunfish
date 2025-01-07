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
        if (rb.position.x < bounds.min.x || 
            rb.position.y < bounds.min.y || 
            rb.position.x > bounds.max.x || 
            rb.position.y > bounds.max.y) {
            rb.position = spawnPoint;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0.0f;
        }
    }
}
