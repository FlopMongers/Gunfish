using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.InputSystem.InputAction;

public class Gunfish : MonoBehaviour
{
    public GunfishData data;
    public bool debug = false;

    private List<GameObject> segments;
    private GunfishGenerator generator;
    private new GunfishRenderer renderer;
    private GunfishRigidbody body;

    private Vector2 movement;

    private void Start() {
        if (data.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {data.segmentCount}. Must be greater than or equal to 3.");
        }
        generator = new GunfishGenerator(this);

        segments = generator.Generate();

        renderer = new GunfishRenderer(data.spriteMat, segments);
        body = new GunfishRigidbody(segments);
    }

    private void Update() {

        renderer.Render();

        if (!debug) return;

        if (Input.GetMouseButton(0)) {
            var targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var targetSegment = segments[segments.Count / 2];
            targetSegment.GetComponent<Rigidbody2D>().AddForce(1 * (targetPos - targetSegment.transform.position));
        }
    }

    private void FixedUpdate() {
        if (movement.sqrMagnitude > Mathf.Epsilon) {
            if (body.Grounded) {
                GroundedMovement(movement);
            } else {
                AerialMovement(movement);
            }

        }
    }

    private void GroundedMovement(Vector2 input) {
        var index = movement.x > 0f ? Mathf.RoundToInt(segments.Count * 0.25f) : Mathf.RoundToInt(segments.Count * 0.75f);
        var direction = movement.x > 0f ? new Vector2(0.70710678118f, 0.70710678118f) : new Vector2(-0.70710678118f, 0.70710678118f);
        body.ApplyForceToSegment(index, direction * 100f);
    }

    private void AerialMovement(Vector2 input) {
        var index = segments.Count / 2;
        var direction = Mathf.Sign(movement.x);
        body.ApplyTorqueToSegment(index, -direction * 3f);
    }

    public void OnMove(CallbackContext context) {
        var movement = context.ReadValue<Vector2>();
        this.movement = movement;
    }

    public void OnFire(CallbackContext context) {
        var performed = context.performed;
        if (performed) {
            var direction = (segments[1].transform.position - segments[0].transform.position).normalized;
            body.ApplyForceToSegment(0, direction * 1000f);
        }
    }

    public static Gunfish Instantiate(GunfishData data, Vector3 position) {
        var instance = new GameObject("Gunfish");
        // var gunfish = instance.AddComponent
        return null;
    }
}
