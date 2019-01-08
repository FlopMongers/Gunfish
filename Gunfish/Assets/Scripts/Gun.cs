using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    public GunInfo gunInfo;
    public List<BulletPoint> bulletPoints = new List<BulletPoint>();

    public void Awake() {
        foreach (Transform t in transform) {
            bulletPoints.Add(t.GetComponent<BulletPoint>());
        }
    }

    public void FireSubscribe(Gunfish gunfish) {
        foreach (BulletPoint bp in bulletPoints) {
            gunfish.FireEvent += bp.Fire;
        }
    }
}
