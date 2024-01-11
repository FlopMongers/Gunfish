using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : PowerUp
{
    public float healthAmount;

    public override void PickUp(Gunfish gunfish) {
        gunfish.UpdateHealth(healthAmount);
        base.PickUp(gunfish);
    }
}
