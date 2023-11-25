using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class TransformTuple {
    public Vector2 position;
    public float rotation; // z rotation
}


[CreateAssetMenu(fileName = "New Gun Data", menuName = "Scriptable Objects/Gun Data")]
public class GunData : ScriptableObject {
    [Header("Gun")]
    public List<TransformTuple> gunBarrels = new List<TransformTuple>();
    public float kickback;
    public float range;
    public float damage;
    public float knockback;
    public int maxAmmo;

    public GameObject gunBarrelPrefab;
    public float fireCooldown;
    public float reload;
    public float reloadWait;
}