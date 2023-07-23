using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMatchUI : MonoBehaviour
{
    public DeathMatchUIPlayerWidget p1widget;
    public DeathMatchUIPlayerWidget p2widget;
    public DeathMatchUIPlayerWidget p3widget;
    public DeathMatchUIPlayerWidget p4widget;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(GameParameters parameters, DeathMatchManager matchManager)
    {

    }

    public void OnStockChange(int playerNum, int newStockValue)
    {
        // TODO
    }
}
