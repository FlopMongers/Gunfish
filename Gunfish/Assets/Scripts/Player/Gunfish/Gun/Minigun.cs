using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Minigun : AutomaticGun
{
    // NOTE(Wyatt): since there will only be one minigun, I don't think we really need to make this a scriptable object yet.
    float revValue;
    public Vector2 revRange = new Vector2();
    public Vector2 fireCooldown_modifier_range = new Vector2();

    public RevUp revUp;

    // NOTE(Wyatt): maybe miniguns and lasers should have a constantly playing audio source that we just turn up or down depending on whether they're warming up?


    protected override bool CheckButtonStatus(ButtonStatus firingStatus) {

        //print($"{revValue}, {revRange}, {fireCoolDown_modifier}");

        fireCoolDown_modifier = Mathf.Lerp(
            fireCooldown_modifier_range.x, 
            fireCooldown_modifier_range.y, 
            ExtensionMethods.GetNormalizedValueInRange(revValue, revRange.x, revRange.y, clamp: true));
        //print(firingStatus);

        bool result = base.CheckButtonStatus(firingStatus);
        // if pressed, start the timer
        if (firingStatus == ButtonStatus.Pressed) {
            // start playing warm up sound
            // turn up warm up sound
            revUp.SetWarmupParticles(true, barrels[0].transform, Mathf.Clamp(revRange.x - revValue, 0, revRange.x));
        }
        // if held, decrement the timer (play fx)
        else if (firingStatus == ButtonStatus.Holding && revValue < revRange.y) {
            //print("WHAT THE FUCK");
            revValue += Time.deltaTime;
        }
        else if (revValue > 0) {
            revValue -= Time.deltaTime;
        }
        if ((result && revValue >= revRange.x) || !result) {
            if (barrels != null && barrels.Count > 0 && barrels[0] != null && revUp != null) {
                revUp.SetWarmupParticles(false, barrels[0].transform, 0.2f);
            }
        }

        // else if playing warm up sound
        // stop playing warm up sound
        // if result and timer <= 0
        return result && revValue >= revRange.x;
    }
}
