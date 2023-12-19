using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBarrel : MonoBehaviour {

    public LineRenderer lr;
    public float flashTimer;

    public void Flash(Vector3 endPoint) {
        StartCoroutine(CoFlash(endPoint));
    }

    IEnumerator CoFlash(Vector3 endPoint) {
        // turn on lr
        lr.enabled = true;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, endPoint);
        var t = flashTimer;
        while (t > 0) {
            t -= Time.deltaTime;
            yield return null;
        }
        lr.enabled = false;
    }
}