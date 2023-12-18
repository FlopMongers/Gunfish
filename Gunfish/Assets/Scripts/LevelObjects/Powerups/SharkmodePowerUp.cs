using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class SharkmodePowerUp : PowerUp
{
    // NOTE(Wyatt): I'm not going to bother making this a scriptable object.
    // just make a sub-class and have it impart whatever effect you want.

    public static float sharkmodeDuration; 

    public override void PickUp(Gunfish gunfish) {
        base.PickUp(gunfish);
        gunfish.AddEffect(new Sharkmode_Effect(gunfish, 5f));
    }
    
}
