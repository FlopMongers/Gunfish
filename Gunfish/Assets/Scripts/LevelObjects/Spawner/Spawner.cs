using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public List<PickUpTuple> spawnPrefabProbabilityList = new List<PickUpTuple>();
    Dictionary<GameObject, float> spawnPrefabProbabilityMap = new Dictionary<GameObject, float>();

    public GameObject spawnPrefab;

    public bool useSpawnTimerOnStart;
    //[HideInInspector]
    public float spawnTimer;
    public Vector2 spawnTimerRange;

    public Collider2D spawnArea;

    public bool active = true;

    public bool useSpawnForce;
    public Vector2 spawnForceMagnitudeRange;
    // degrees from Vector3.up, Unity rotation convention: 0 = up, 90 = left, 180 = down, 270 = right
    public Vector2 spawnForceDirectionRange;
    public Vector2 spawnForceTorqueRange;

    protected virtual void Start() {
        // from 0 to max
        spawnTimer = (useSpawnTimerOnStart) ? spawnTimer : Random.Range(0, spawnTimerRange.y);
        spawnArea = spawnArea ?? GetComponent<Collider2D>();
        if (spawnArea == null) {
            spawnArea = gameObject.AddComponent<BoxCollider2D>();
            spawnArea.isTrigger = true;
        }
        foreach (var pickUpTuple in spawnPrefabProbabilityList) {
            spawnPrefabProbabilityMap[pickUpTuple.pickup] = pickUpTuple.probability;
        }
        //print(spawnArea);
    }

    // Update is called once per frame
    protected virtual void Update() {
        // decrement spawnTimer
        // if 0, spawn the thing
        // subscribe the thing being picked up
        if (!active)
            return;
        UpdateSpawn();
    }

    protected virtual void UpdateSpawn() {
        if (spawnTimer >= 0) {
            spawnTimer -= Time.deltaTime;
        }
        if (spawnTimer < 0) {
            Spawn();
        }
    }

    public virtual GameObject Spawn() {
        spawnTimer = spawnTimerRange.RandomInRange();
        print("spawn");
        GameObject instance = Instantiate(
            spawnPrefabProbabilityMap.Choose(spawnPrefab),
            spawnArea.bounds.RandomPointInBounds(),
            Quaternion.identity);

        if (useSpawnForce && instance.TryGetComponent(out Rigidbody2D rb)) {
            float angle = spawnForceDirectionRange.RandomInRange();
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            rb.AddForce(direction * spawnForceMagnitudeRange.RandomInRange(), ForceMode2D.Impulse);
            rb.AddTorque(spawnForceTorqueRange.RandomInRange(), ForceMode2D.Impulse);
        }

        return instance;
    }

    private void OnDrawGizmos() {
        if (!useSpawnForce) return;

        Vector3 origin = (spawnArea != null) ? spawnArea.bounds.center : transform.position;

        // arc spanning the direction range, drawn at a fixed 1 unit radius
        float startAngle = spawnForceDirectionRange.x;
        float endAngle = spawnForceDirectionRange.y;
        int segments = 24;
        Gizmos.color = Color.cyan;
        Vector3 previousPoint = origin + Quaternion.Euler(0f, 0f, startAngle) * Vector3.up;
        Gizmos.DrawLine(origin, previousPoint);
        for (int i = 1; i <= segments; i++) {
            float angle = Mathf.Lerp(startAngle, endAngle, (float)i / segments);
            Vector3 currentPoint = origin + Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        Gizmos.DrawLine(origin, previousPoint);

        // min/max force magnitude at the midpoint angle, 1 unit distance per 1 unit of force
        Vector3 midDirection = Quaternion.Euler(0f, 0f, (startAngle + endAngle) / 2f) * Vector3.up;
        DrawForceArrow(origin, midDirection * spawnForceMagnitudeRange.x, Color.green);
        DrawForceArrow(origin, midDirection * spawnForceMagnitudeRange.y, Color.red);
    }

    private void DrawForceArrow(Vector3 origin, Vector3 offset, Color color) {
        if (offset.sqrMagnitude < 0.0001f) return;

        Gizmos.color = color;
        Vector3 tip = origin + offset;
        Gizmos.DrawLine(origin, tip);

        Vector3 direction = offset.normalized;
        Vector3 rightHead = Quaternion.Euler(0f, 0f, 150f) * direction * 0.15f;
        Vector3 leftHead = Quaternion.Euler(0f, 0f, -150f) * direction * 0.15f;
        Gizmos.DrawLine(tip, tip + rightHead);
        Gizmos.DrawLine(tip, tip + leftHead);
    }
}