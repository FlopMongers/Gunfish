using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Gun
{
    bool fireCanceled;

    protected override bool CheckButtonStatus(ButtonStatus firingStatus) {

        if (firingStatus != ButtonStatus.Holding) {
            fireCanceled = false;
        }

        if (gunfish.underwater) {
            fireCanceled = true;
        }

        return (firingStatus == ButtonStatus.Holding || firingStatus == ButtonStatus.Pressed) && !fireCanceled;
    }
}
