
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum MaterialType { Wood, Fish, Inflatable, Metal, Sand, };

[Serializable]
public class EnumTuple<T, K>
{
    public T key;
    public K value;
}


public class FX_CollisionHandler : Singleton<FX_CollisionHandler>
{

    public List<EnumTuple<MaterialType, FXType>> defaultCollisionMapList = new List<EnumTuple<MaterialType, FXType>>();
    public List<EnumTuple<EnumTuple<MaterialType, MaterialType>, FXType>> collisionMapList = new List<EnumTuple<EnumTuple<MaterialType, MaterialType>, FXType>>();

    // map from material to fx type for default collision
    Dictionary<MaterialType, FXType> defaultCollisionMap = new Dictionary<MaterialType, FXType>();
    // matrix of material to material 
    Dictionary<MaterialType, Dictionary<MaterialType, FXType>> collisionMap = new Dictionary<MaterialType, Dictionary<MaterialType, FXType>>();

    float impactThreshold = 15f;
    float maxThreshold = 50f;

    public bool softLimit = false;

    public void Start()
    {
        foreach (var elem in defaultCollisionMapList)
        {
            defaultCollisionMap[elem.key] = elem.value;
        }

        foreach (var elem in collisionMapList)
        {
            if (collisionMap.ContainsKey(elem.key.key) == false)
            {
                collisionMap[elem.key.key] = new Dictionary<MaterialType, FXType>();
            }
            collisionMap[elem.key.key][elem.key.value] = elem.value;
        }
    }

    public void HandleDefaultCollision(ObjectMaterial mat, Collision2D collision)
    {
        if (defaultCollisionMap.ContainsKey(mat.materialType) == false)
            return;
        PlayCollisionFX(defaultCollisionMap[mat.materialType], collision);
    }

    public void HandleCollision(ObjectMaterial mat1, ObjectMaterial mat2, Collision2D collision)
    {
        if (collisionMap.ContainsKey(mat1.materialType) && collisionMap[mat1.materialType].ContainsKey(mat2.materialType)) 
        {
            PlayCollisionFX(collisionMap[mat1.materialType][mat2.materialType], collision);
            return;
        }
        if (collisionMap.ContainsKey(mat2.materialType) && collisionMap[mat2.materialType].ContainsKey(mat1.materialType))
        {
            PlayCollisionFX(collisionMap[mat2.materialType][mat1.materialType], collision);
            return;
        }
        if (defaultCollisionMap.ContainsKey(mat1.materialType))
        {
            HandleDefaultCollision(mat1, collision);
            return;
        }
        if (defaultCollisionMap.ContainsKey(mat2.materialType))
        {
            HandleDefaultCollision(mat2, collision);
            return;
        }
    }

    public void PlayCollisionFX(FXType fxType, Collision2D collision)
    {
        if ((collision.contacts[0].normalImpulse > impactThreshold && !softLimit) || softLimit)
        {
            float vol = Mathf.Clamp01(collision.contacts[0].normalImpulse / maxThreshold);

            FX_Spawner.instance.SpawnFX(fxType, collision.contacts[0].point, collision.contacts[0].normal, vol);
        }
    }
}