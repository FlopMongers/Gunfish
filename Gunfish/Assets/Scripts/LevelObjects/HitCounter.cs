using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCounter : MonoBehaviour
{
    public float timestamp = -1;
    private Gunfish lastHitter;

    static float lastHitThreshod = 3f;

    public void TakeHit(Gunfish gunfish) {
        if (Time.time > timestamp) {
            lastHitter = gunfish;
            timestamp = Time.time;
        }
    }

    public Gunfish GetLastHitter() {
        return (timestamp > 0 && Time.time - timestamp < lastHitThreshod) ? lastHitter : null;
    }
}
