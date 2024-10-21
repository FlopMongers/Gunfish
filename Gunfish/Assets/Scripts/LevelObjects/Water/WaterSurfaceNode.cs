using UnityEngine;

public class WaterSurfaceNode : MonoBehaviour {
    public Rigidbody2D rb;
    public SpringJoint2D selfSpring;
    public SpringJoint2D prevSpring;

    public WaterZone zone;
    public FishDetector detector;

    float splooshTimestamp = -1f, splooshDuration = 0.5f;
    Vector2 currentSplooshAmount;
    float extraSplooshThreshold = 0.75f;
    float splooshHeightThreshold = 2f; // measure distance from 

    public void Sploosh(Vector2 force) {
        if ((Time.time - splooshTimestamp) < splooshDuration)
            return;
        // if height too far off and same direction, no sploosh
        if (Vector2.Distance(transform.position, selfSpring.connectedAnchor) > splooshHeightThreshold &&
            Mathf.Abs(Vector2.SignedAngle(currentSplooshAmount, force)) > 10) {
            return;
        }
        float magnitude = force.magnitude;
        // check what percentage greater than the current sploosh
        // if larger than extraSplooshThreshold, apply sploosh
        if (currentSplooshAmount.magnitude > 0 &&
            Mathf.Abs(magnitude / currentSplooshAmount.magnitude) < extraSplooshThreshold) {
            return;
        }
        splooshTimestamp = Time.time;
        currentSplooshAmount = force;
        rb.AddForce(currentSplooshAmount, ForceMode2D.Impulse);
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        zone.OnTriggerEnter2D(collision);
        detector.OnTriggerEnter2D(collision);
    }

    public void OnTriggerExit2D(Collider2D collision) {
        zone.OnTriggerExit2D(collision);
        detector.OnTriggerExit2D(collision);
    }
}