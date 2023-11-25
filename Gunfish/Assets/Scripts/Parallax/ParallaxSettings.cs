using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Parallax Settings", menuName = "Scriptable Objects/Parallax Settings")]
public class ParallaxSettings : ScriptableObject {
    public float maxDistance = 100f;
    public bool ignoreYAxis = false;

    private void OnValidate() {
        maxDistance = Mathf.Max(0f, maxDistance);
    }
}