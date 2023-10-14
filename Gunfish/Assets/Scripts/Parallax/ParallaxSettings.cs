using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ParallaxSettings", fileName = "New ParallaxSettings")]
public class ParallaxSettings : ScriptableObject {
    public float maxDistance = 100f;
    public bool ignoreYAxis = false;

    private void OnValidate() {
        maxDistance = Mathf.Max(0f, maxDistance);
    }
}
