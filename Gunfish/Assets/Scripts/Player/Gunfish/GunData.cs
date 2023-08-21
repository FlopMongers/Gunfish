using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

[Serializable]
public class TransformTuple {
    public Vector2 position;
    public float rotation; // z rotation
}


[CreateAssetMenu(menuName = "Gunfish/Gun Data", fileName = "New Gun Data")]
public class GunData : ScriptableObject {
    [Header("Gun")]
    public List<TransformTuple> gunBarrels = new List<TransformTuple>();
    public float gunKickback;
    public float gunRange;
    public float gunDamage;
    public float gunKnockback;
    public GameObject gunBarrelPrefab;
    [Range(0f, 5f)] public float reloadTime;
}
