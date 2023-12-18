using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigun : AutomaticGun
{
    // NOTE(Wyatt): since there will only be one minigun, I don't think we really need to make this a scriptable object yet.
    public float fireWarmUpTime = 2f;
    float fireWarmUpTimer;

    // NOTE(Wyatt): maybe miniguns and lasers should have a constantly playing audio source that we just turn up or down depending on whether they're warming up?

    protected override bool CheckButtonStatus(ButtonStatus firingStatus) {
        bool result = base.CheckButtonStatus(firingStatus);
        // if pressed, start the timer
        if (firingStatus == ButtonStatus.Pressed) {
            fireWarmUpTimer = fireWarmUpTime;
        }
        // if held, decrement the timer (play fx)
        else if (firingStatus == ButtonStatus.Holding && fireWarmUpTimer > 0 && reload_timer <= 0 && fireCooldown_timer <= 0) {
            fireWarmUpTimer -= Time.deltaTime;
        }
        // if result and timer <= 0
        return result && fireWarmUpTimer < Time.deltaTime;
    }
}
