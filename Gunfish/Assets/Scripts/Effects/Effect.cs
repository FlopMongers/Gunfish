using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum EffectType { FlopModify };


[Serializable]
public class Effect
{
    public Gunfish gunfish;
    public EffectType effectType;

    public Effect(Gunfish gunfish)
    {
        this.gunfish = gunfish;
    }

    public virtual void OnAdd()
    {
        // do something to that gunfish
    }

    public virtual void Update()
    {
        // keep doing something to that gunfish 
    }

    public virtual void Merge(Effect effect)
    {
        // eat that friggin effect
    }

    public virtual void OnRemove()
    {
        // stop doing what you were doing to that poor gunfish
    }
}

[Serializable]
public class FlopModify_Effect : Effect
{
    public float flopMultiplier;

    public FlopModify_Effect(Gunfish gunfish, float flopMultiplier) : base(gunfish)
    {
        this.flopMultiplier = flopMultiplier;
        effectType = EffectType.FlopModify;
    }

    public override void OnAdd()
    {
        base.OnAdd();
        gunfish.statusData.flopForce *= flopMultiplier;
    }

    public override void OnRemove()
    {
        base.OnRemove();
        gunfish.statusData.flopForce = gunfish.data.flopForce;
    }
}