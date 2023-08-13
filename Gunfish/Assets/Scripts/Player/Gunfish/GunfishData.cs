using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

[Serializable]
public class TransformTuple
{
    public Vector2 position;
    public float rotation; // z rotation
}


[CreateAssetMenu(menuName = "Gunfish/Gunfish Data", fileName = "New Gunfish Data")]
public class GunfishData : ScriptableObject {
    [Header("Input")]
    public InputActionAsset inputActions;

    [Header("Materials")]
    public Material spriteMat;
    public PhysicsMaterial2D physicsMaterial;
    public Sprite sprite;

    [Header("Physics")]
    [Range(1f, 100f)] public float mass = 1f;
    [Range(0f, 1f)] public float fixedJointDamping = 0f;
    public float fixedJointFrequency = 32f;
    [Range(0f, 1f)] public float springJointDamping = 0f;
    public float springJointFrequency = 32f;
    public float flopForce;
    public float groundTorque;
    public float airTorque;
    public float maxAerialAngularVelocity;
    public float maxUnderwaterVelocity;
    public float underwaterForce;
    public float underwaterTorque;
    public float gunKickback;

    [Header("Health")]
    public float maxHealth;
    public GameObject healthUI;

    [Header("Gun")]
    public List<TransformTuple> gunBarrels = new List<TransformTuple>();
    public float gunRange;
    public float gunDamage;
    public float gunKnockback;
    public GameObject gunBarrelPrefab;

    [Header("Dimensions")]
    [Range(0.5f, 5f)] public float length = 2f;
    public AnimationCurve width = AnimationCurve.Constant(0f, 1f, 1f);
    [Range(3, 33)] public int segmentCount = 8;

    [Header("Cooldowns")]
    [Range(0f, 1f)] public float flopCooldown;
    [Range(0f, 5f)] public float reloadTime;
}
