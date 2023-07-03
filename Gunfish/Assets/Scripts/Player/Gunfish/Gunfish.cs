using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

public class Gunfish : MonoBehaviour {
    public static Gunfish Instantiate(GunfishData data, Vector3 position, LayerMask layer) {
        var instance = new GameObject($"Player{layer.value - 5}GunfishHandler");
        instance.transform.SetPositionAndRotation(position, Quaternion.identity);
        var gunfish = instance.AddComponent<Gunfish>();
        gunfish.Spawn(data, layer);
        return gunfish;
    }

    public GunfishStatusData statusData;
    public GunfishData data;
    public bool debug = false;

    private List<GameObject> segments;
    private GunfishGenerator generator;
    private new GunfishRenderer renderer;
    private GunfishRigidbody body;

    private InputActionMap inputHandler;
    

    private Vector2 movement;

    private void Start() {
        if (debug) {
            Spawn(data, LayerMask.NameToLayer("Player1"));
        }

        inputHandler = GetComponent<PlayerInput>().actions.FindActionMap("Player");
        inputHandler.FindAction("Fire").performed += ctx => Fire();
    }

    private void Update() {
        renderer?.Render();
        DecrementTimers(Time.deltaTime);

        Move(inputHandler.FindAction("Move").ReadValue<Vector2>());
    }

    private void FixedUpdate() {
        Movement();
    }

    private void DecrementTimers(float delta) {
        statusData.stunTimer = Mathf.Max(0f, statusData.stunTimer - delta);
        statusData.reloadTimer = Mathf.Max(0f, statusData.reloadTimer - delta);
        statusData.flopTimer = Mathf.Max(0f, statusData.flopTimer - delta);
    }

    private void Movement() {
        DebugMovement();

        if (statusData.IsStunned || !statusData.CanFlop) return;

        if (movement.sqrMagnitude > Mathf.Epsilon) {
            if (body.Grounded) {
                GroundedMovement(movement);
            } else {
                AerialMovement(movement);
            }

        }
    }

    private void DebugMovement() {
        if (!debug) return;

        if (Input.GetMouseButton(0)) {
            var targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var targetSegment = segments[segments.Count / 2];
            targetSegment.GetComponent<Rigidbody2D>().AddForce(data.mass * 5f * (targetPos - targetSegment.transform.position));
        }
    }

    private void GroundedMovement(Vector2 input) {
        var index = movement.x > 0f ? Mathf.RoundToInt(segments.Count * 0.25f) : Mathf.RoundToInt(segments.Count * 0.75f);
        var direction = movement.x > 0f ? new Vector2(1f, 1f).normalized : new Vector2(-1f, 1f).normalized;
        body.ApplyForceToSegment(index, direction * 1000f);
    }

    private void AerialMovement(Vector2 input) {
        var index = segments.Count / 2;
        var direction = Mathf.Sign(movement.x);
        body.ApplyTorqueToSegment(index, -direction * 3f);
    }

    public void Move(Vector2 movement) {
        this.movement = movement;
    }

    public void Fire() {
        if (!statusData.CanFire) return;

        var direction = (segments[1].transform.position - segments[0].transform.position).normalized;
        body.ApplyForceToSegment(0, direction * 1000f);
    }

    public void Spawn(GunfishData data, LayerMask layer) {
        if (data.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {data.segmentCount}. Must be greater than or equal to 3.");
        }
        this.data = data;
        this.statusData = new GunfishStatusData();

        generator = new GunfishGenerator(this);

        segments = generator.Generate(layer);

        renderer = new GunfishRenderer(data.spriteMat, segments);
        body = new GunfishRigidbody(segments);
    }

    public void Despawn(bool animated) {
        DespawnSegments(animated);
    }

    private void DespawnSegments(bool animated) {
        while (segments.Count > 0) {
            var segment = segments[0];
            segments.Remove(segment);
            DespawnSegment(segment, animated);
        }
    }

    private void DespawnSegment(GameObject segment, bool animated) {
        if (animated) {
            // Todo: replace with cool animated fish guts or something
            Destroy(segment);
        } else {
            Destroy(segment);
        }
    }
}
