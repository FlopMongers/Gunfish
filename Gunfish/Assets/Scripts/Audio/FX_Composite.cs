using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FX_Composite : FX_Object
{
    new void Awake()
    {
        parent = GetComponentInParent<FX_Object>() == null;
    }

    protected void Start()
    {
        if (parent)
        {
            foreach (var fx_object in GetComponentsInChildren<FX_Object>())
            {
                MaxLifetime = Mathf.Max(MaxLifetime, fx_object.MaxLifetime);
            }
            foreach (var fx_object in GetComponentsInChildren<FX_Object>())
            {
                fx_object.lifetime = MaxLifetime;
            }
        }
    }

    public override void Play()
    {
        foreach (var fx_object in GetComponentsInChildren<FX_Object>())
        {
            fx_object.Play();
        }
    }

    public override void Replay()
    {
        foreach (var fx_object in GetComponentsInChildren<FX_Object>())
        {
            fx_object.Replay();
        }
    }
}
