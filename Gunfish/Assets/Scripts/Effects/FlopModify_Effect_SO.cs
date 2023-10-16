using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFlopModifyEffect", menuName = "ScriptableObjects/FlopModifyEffect", order = 0)]
public class FlopModify_Effect_SO : Effect_SO {
    public float flopMultiplier;

    public override Effect Create(Gunfish gunfish) {
        return new FlopModify_Effect(gunfish, flopMultiplier);
    }
}