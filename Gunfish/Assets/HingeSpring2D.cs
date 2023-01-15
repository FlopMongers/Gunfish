using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeSpring2D : MonoBehaviour {
 
    public HingeJoint2D hinge;
    private Rigidbody2D rb;
 
    public float force = 10;
    public float minAngle = 5;
 
    void Start ()
    {
        hinge = GetComponent<HingeJoint2D> ();
        rb = GetComponent<Rigidbody2D>();
    }
 
    void FixedUpdate () {
 
        float targetForce = hinge.referenceAngle - hinge.jointAngle;
        targetForce = Mathf.Sign (targetForce) * Mathf.Max(0,Mathf.Abs (targetForce) - minAngle);
 
        AddTorqueAtPosition(force * targetForce,transform.TransformPoint( hinge.anchor));
    }

    private void AddTorqueAtPosition(float torque, Vector2 rotationPoint, ForceMode2D forceMode = ForceMode2D.Force)
    {
        rb.AddForceAtPosition (-Vector2.up * torque * rb.inertia, rotationPoint + Vector2.right, forceMode);
        rb.AddForceAtPosition (Vector2.up * torque * rb.inertia, rotationPoint - Vector2.right, forceMode);
    }
}
