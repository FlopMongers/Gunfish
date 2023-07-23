using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMatchUIPlayerWidget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(bool isEnabled, int stockValue, Player player)
    {
        // TODO
    }

    public void OnStockChange(int newStockValue)
    {
        // TODO update stock value, trigger anim
    }

    public void OnScoreChange(int newScoreValue)
    {
        // TODO update score value, trigger anim
        if (newScoreValue == 0)
        {
            OnPlayerEliminated();
        }
    }

    private void OnPlayerEliminated()
    {
        // TODO gray out player display
    }
}
