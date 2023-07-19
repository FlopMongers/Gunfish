using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sand : GroundMaterial
{
    public Effect_SO effect;

    public override void AddFish(Gunfish gunfish)
    {
        base.AddFish(gunfish);
        gunfish.AddEffect(effect.Create(gunfish));
    }
}
