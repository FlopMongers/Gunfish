using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin : MonoBehaviour {
    [SerializeField]
    private FishDetector detector;

    [SerializeField]
    [Range(0f, 100f)]
    private float prickMagnitude = 10f;
    
    [SerializeField]
    [Range(0f, 10f)]
    private float prickDamage = 10f;

    private void Start() {
        detector.OnFishTriggerEnter += OnFishEnter;
        detector.OnFishTriggerExit += OnFishExit;
    }

    private void OnFishEnter(GunfishSegment segment, Collider2D collision) {
        var direction = (segment.transform.position - transform.position).normalized;

        Debug.Log(segment.rb);
        Debug.Log(direction);
        segment.rb.AddForce(direction * prickMagnitude * 100f);
        segment.gunfish.UpdateHealth(-prickDamage);
    }

    private void OnFishExit(GunfishSegment segment, Collider2D collision) {

    }
}
