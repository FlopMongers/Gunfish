using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(FishDetector))]
public class Pelican : MonoBehaviour
{
    public Vector3 endPosition;

    Vector2 currentDir;

    float killThreshold = 0.5f;

    public Vector2 pelicanSpeedRange = new Vector2(1f, 5f);
    float speed = 1f;

    [HideInInspector]
    public bool zoomin;

    public Rigidbody2D rb;

    public FishDetector fishDetector;

    // Start is called before the first frame update
    void Start()
    {
        rb = rb ?? GetComponent<Rigidbody2D>();
        fishDetector = fishDetector ?? GetComponent<FishDetector>();
        fishDetector.OnFishTriggerEnter += HitFish;
        speed = Random.Range(pelicanSpeedRange.x, pelicanSpeedRange.y);
    }

    // Update is called once per frame
    void Update()
    {
        // if zoomin and endPosition reached, kill the friggin bird
        // face direction of movement
        if (!zoomin) {
            return;
        }
        if (Vector3.Distance(transform.position, endPosition) <= killThreshold) {
            Destroy(gameObject);
            zoomin = false;
            return;
        }
        currentDir = (endPosition - transform.position).normalized;
        rb.velocity = currentDir * speed;
        transform.up = currentDir;
    }

    public void HitFish(GunfishSegment segment, Collider2D collision) {
        segment.gunfish.Hit(new FishHitObject(segment.index, transform.position, segment.transform.position - transform.position, gameObject, segment.gunfish.statusData.health, 100f));
    }
}
