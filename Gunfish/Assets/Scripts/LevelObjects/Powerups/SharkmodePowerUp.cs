using UnityEngine;

public class SharkmodePowerUp : PowerUp
{
    // NOTE(Wyatt): I'm not going to bother making this a scriptable object.
    // just make a sub-class and have it impart whatever effect you want.

    public float sharkmodeDuration; 


    public override void PickUp(Gunfish gunfish) {
        gunfish.AddEffect(new Sharkmode_Effect(gunfish, sharkmodeDuration));
        base.PickUp(gunfish);
    }
    
}
