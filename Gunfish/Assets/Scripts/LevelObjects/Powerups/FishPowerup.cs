using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPowerup : PowerUp
{
    public GunfishData fishData;

    public override void PickUp(Gunfish gunfish) {
        //gunfish.UpdateHealth(healthAmount);
        gunfish.SwapFish(fishData);
        base.PickUp(gunfish);
    }
}
