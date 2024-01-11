using UnityEngine;

public class InvincibilityPowerUp : PowerUp {

    public float invincibilityDuration;

    public override void PickUp(Gunfish gunfish) {
        gunfish.AddEffect(new Invincibility_Effect(gunfish, invincibilityDuration));
        base.PickUp(gunfish);
    }

}
