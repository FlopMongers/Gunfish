using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGun : Gun
{  
    protected override bool CheckButtonStatus(ButtonStatus firingStatus) {
        return firingStatus == ButtonStatus.Holding || firingStatus == ButtonStatus.Pressed;
    }
}
