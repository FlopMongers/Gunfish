using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitCounter : MonoBehaviour
{
    public float timestamp = -1;
    public Gunfish lastHitter;

    public void TakeHit(Gunfish gunfish) {
        if (Time.time > timestamp) {
            lastHitter = gunfish;
            timestamp = Time.time;
        }
    }
}
