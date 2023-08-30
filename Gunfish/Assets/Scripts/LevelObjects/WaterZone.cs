using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishDetector))]
public class WaterZone : MonoBehaviour
{
    public FishDetector detector;

    // Start is called before the first frame update
    void Start()
    {
        if (detector == null)
            detector = GetComponent<FishDetector>();

        detector.OnFirstSegmentExit += FishSploosh;
        detector.OnFishTriggerEnter += FishSploosh;
    }

    void FishSploosh(GunfishSegment segment, Collider2D collider) {
        // sploosh the darn fish
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        // if fish segment, tell the segment to be underwater
        var fishSegment = collision.gameObject.GetComponent<GunfishSegment>();
        if (fishSegment != null) {
            print(fishSegment);
            fishSegment.SetUnderwater(true);
        }
        else {
            // sploosh
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        // if fish, set not underwater
        var fishSegment = collision.gameObject.GetComponent<GunfishSegment>();
        if (fishSegment != null) {
            fishSegment.SetUnderwater(false);
        }
        else {
            // sploosh
        }
    }
}
