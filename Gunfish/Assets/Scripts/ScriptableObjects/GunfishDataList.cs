using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gunfish Data List", menuName = "Scriptable Objects/Gunfish Data List")]
public class GunfishDataList : ScriptableObject {
    public List<GunfishData> gunfishes;
}
