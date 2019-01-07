using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;

public class Nameplate : MonoBehaviour {

    public TextMeshPro text;

    public PositionConstraint pc;


    private void Start() {
        text = GetComponent<TextMeshPro>();
        pc = GetComponent<PositionConstraint>();
    }


    public void SetName(string playerName) {
        text.text = playerName;
    }

    public void SetOwner(GameObject owner) {
        ConstraintSource cs = new ConstraintSource();
        cs.sourceTransform = owner.transform;
        cs.weight = 1;
        pc.SetSource(0, cs);
        pc.translationOffset = new Vector3(0, 1.5f, -1);
        pc.locked = true;
    }
}
