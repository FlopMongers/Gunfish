using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Effect_SO : ScriptableObject
{
    public virtual Effect Create(Gunfish gunfish)
    {
        return new Effect(gunfish);
    }
}