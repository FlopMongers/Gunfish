using UnityEngine;
using System.Collections.Generic;

public class Gunfish : MonoBehaviour
{
    public GunfishData data;
    public bool debug = false;

    private List<GameObject> segments;
    private GunfishGenerator generator;
    private new GunfishRenderer renderer;
    private GunfishRigidbody body;
    private GunfishInput input;

    private void Start() {
        if (data.segmentCount < 3) {
            throw new UnityException($"Invalid number of segments for Gunfish: {data.segmentCount}. Must be greater than or equal to 3.");
        }
        generator = new GunfishGenerator(this);
        input = new GunfishInput(this);

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
}
