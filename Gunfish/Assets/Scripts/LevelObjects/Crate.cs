using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PickUpTuple {
    public GameObject pickup;
    public float probability;
}

public class Crate : MonoBehaviour
{
    public List<PickUpTuple> pickupTuples = new List<PickUpTuple>();
    Dictionary<GameObject, float> pickupMap = new Dictionary<GameObject, float>();

    public GameObject defaultPickup;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Shootable>().OnDead += OnDead;
        foreach (var pair in pickupTuples)
            pickupMap[pair.pickup] = pair.probability;
    }

    void OnDead() {
        // chance of spawning powerup
        var pickup = pickupMap.Choose(defaultPickup);
        if (pickup == null)
            return;
        Instantiate(pickup, transform.position, Quaternion.identity);
    }
}
