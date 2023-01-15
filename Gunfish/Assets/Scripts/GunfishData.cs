using UnityEngine;

public class GunfishData : ScriptableObject
{
    [Header("Materials")]
    public Material spriteMat;
    public PhysicsMaterial2D physicsMaterial;
    [Header("Physics")]
    [Range(1f, 100f)] public float mass;
    [Range(20f, 30f)] public float maxBend;
    [Range(0f, 1f)] public float fixedJointDamping = 0f;
    public float fixedJointFrequency = 32f;
    [Range(0f, 1f)] public float springJointDamping = 0f;
    public float springJointFrequency = 32f;
    [Header("Dimensions")]
    [Range(0.5f, 5f)] public float length;
    public AnimationCurve width = AnimationCurve.Constant(0f, 1f, 1f);
    [Range(3, 33)] public int segmentCount;
}
