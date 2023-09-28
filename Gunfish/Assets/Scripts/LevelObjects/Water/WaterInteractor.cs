using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WaterInteractor : MonoBehaviour
{

    int isUnderwater;
    public Action<bool> underwaterChangeEvent;

    public void SetUnderwater(int underwater) {
        // if gun and 

        bool change = (isUnderwater == 1 && underwater == -1) || (isUnderwater == 0 && underwater == 1);
        isUnderwater += underwater;

        if (change) {
            underwaterChangeEvent?.Invoke(isUnderwater == 1);
        }
    }

}
