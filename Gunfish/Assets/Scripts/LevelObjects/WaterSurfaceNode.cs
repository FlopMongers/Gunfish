using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSurfaceNode : MonoBehaviour
{
    public Rigidbody2D rb;
    public SpringJoint2D selfSpring;
    public SpringJoint2D prevSpring;

    public WaterZone zone;
    public FishDetector detector;

    public void OnTriggerEnter2D(Collider2D collision) {
        zone.OnTriggerEnter2D(collision);
        detector.OnTriggerEnter2D(collision);
    }

    public void OnTriggerExit2D(Collider2D collision) {
        zone.OnTriggerExit2D(collision);
        detector.OnTriggerExit2D(collision);
    }
}
