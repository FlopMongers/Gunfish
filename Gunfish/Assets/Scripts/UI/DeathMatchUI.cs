using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMatchUI : MonoBehaviour {
    [SerializeField]
    private List<DeathMatchUIPlayerWidget> playerWidgets = new List<DeathMatchUIPlayerWidget>();

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void Initialize(List<Player> players, int initialStockCount) {
        for (int i = 0; i < playerWidgets.Count; i++) {
            if (players.Count > i && players[i] != null) {
                playerWidgets[i].Initialize(initialStockCount, players[i]);
            }
        }
    }

    public void OnStockChange(Player player, int newStockValue) {
        playerWidgets.Find((pwidget) => pwidget.player == player)?.OnStockChange(newStockValue);
    }
}
