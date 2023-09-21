using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour {

    Dictionary<Transform, Vector4> startingPos = new Dictionary<Transform, Vector4>();

    public float speed = 10f, amount = 0.15f;
    float radius = 0;
    [HideInInspector]
    public bool shaking;
    [HideInInspector]
    public bool shooketh;

    public List<Transform> targets = new List<Transform>();

    private void Awake() {
        radius = amount / 2f;
    }

    public void Activate(float time = float.NaN) {
        if (shaking)
            return;
        if (!shooketh)
            SetPositions();
        StartCoroutine(ActivateCo(time));
    }

    public void SetPositions() {
        shooketh = true;
        foreach (var target in targets) {
            startingPos[target] = new Vector4(
                target.position.x - radius,
                target.position.y - radius,
                Random.Range(0, 1f),
                Random.Range(0, 1f));
        }
    }

    IEnumerator ActivateCo(float time = float.NaN) {
        shaking = true;
        while (float.IsNaN(time) || time > 0) {
            foreach (var target in startingPos.Keys) {
                if (!target)
                    continue;
                target.position = (Vector2)startingPos[target] + new Vector2(
                    Mathf.PerlinNoise(Time.time * speed, startingPos[target].z) * amount,
                    Mathf.PerlinNoise(Time.time * speed, startingPos[target].w) * amount);
            }
            time -= Time.deltaTime;
            yield return null;
        }
        ResetPositions();
        shaking = false;
    }

    void ResetPositions() {
        foreach (var target in startingPos.Keys) {
            if (!target)
                continue;
            target.position = (Vector2)startingPos[target] + new Vector2(radius, radius);
        }
    }

    public void Deactivate() {
        StopAllCoroutines();
        ResetPositions();
        shaking = false;
    }

}