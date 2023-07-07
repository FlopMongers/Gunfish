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
    private Gun gun;

    private InputActionMap inputHandler;
    

    private Vector2 movement;

    private void Start() {
        gun = GetComponent<Gun>();
        gun.gunfish = this;

        if (debug) {
            Spawn(data, LayerMask.NameToLayer("Player1"));
        }

        inputHandler = GetComponent<PlayerInput>().actions.FindActionMap("Player");
        inputHandler.FindAction("Fire").performed += ctx => { gun?.Fire(); };
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
        // reset flop timer
        statusData.flopTimer = data.flopCooldown;
        var index = segments.Count / 2; //movement.x > 0f ? Mathf.RoundToInt(segments.Count * 0.25f) : Mathf.RoundToInt(segments.Count * 0.75f);
        var direction = movement.x > 0f ? new Vector2(1f, 1f).normalized : new Vector2(-1f, 1f).normalized;
        // flop force
        body.ApplyForceToSegment(index, direction * data.flopForce, ForceMode2D.Impulse);
        AerialMovement(input, data.groundTorque, ForceMode2D.Impulse);
    }

    private void AerialMovement(Vector2 input, float? airTorque=null, ForceMode2D forceMode = ForceMode2D.Force) {
        var index = segments.Count / 2;
        var direction = Mathf.Sign(input.x);
        // rotation speed
        body.ApplyTorqueToSegment(index, -direction * airTorque.GetValueOrDefault(data.airTorque), forceMode);
    }

    public void Move(Vector2 movement) {
        this.movement = movement;
    }

    public void Fire()
    {
        gun.Fire();
    }

    public void Hit(int index)
    {
        // TODO: define this
        // perform knockback
        // damage health
        // apply any effects
        // spawn fx
    }

    public void Kickback(float kickback) { 
        var direction = (segments[1].transform.position - segments[0].transform.position).normalized;
        // gun kickback
        body.ApplyForceToSegment(0, direction * kickback, ForceMode2D.Impulse);
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

        // TODO: initialize gun position and rotation
        foreach (TransformTuple tuple in data.gunBarrels)
        {
            // spawn 
            var barrel = new GameObject("barrel").transform;
            barrel.parent = segments[0].transform;
            barrel.localPosition = tuple.position;
            barrel.localEulerAngles = Vector3.forward * tuple.rotation;
            gun.barrels.Add(barrel);
        }
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
