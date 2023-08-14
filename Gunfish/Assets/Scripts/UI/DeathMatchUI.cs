using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMatchUI : MonoBehaviour {
    [SerializeField]
    private List<DeathMatchUIPlayerWidget> playerWidgets = new List<DeathMatchUIPlayerWidget>();
    [SerializeField]
    private LoadingCountdownUI loadingScreen;

    // Start is called before the first frame update
    void Start() {
        foreach (var playerWidget in playerWidgets)
        {
            playerWidget.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void InitializeMatch(List<Player> players) {
        for (int i = 0; i < playerWidgets.Count; i++) {
            if (players.Count > i && players[i] != null) {
                playerWidgets[i].gameObject.SetActive(true);
                playerWidgets[i].InitializeMatch(players[i]);
                playerWidgets[i].OnScoreChange(0);
            }
        }
    }

    public void InitializeLevel(List<Player> players, int initialStockCount) {
        for (int i = 0; i < playerWidgets.Count; i++) {
            if (players.Count > i && players[i] != null) {
                playerWidgets[i].gameObject.SetActive(true);
                playerWidgets[i].InitializeLevel(initialStockCount, players[i]);
            }
        }
    }

    public void OnStockChange(Player player, int newStockValue) {
        playerWidgets.Find((pwidget) => pwidget.player == player)?.OnStockChange(newStockValue);
    }

    public void OnScoreChange(Player player, int newScoreValue) {
        playerWidgets.Find((pwidget) => pwidget.player == player)?.OnScoreChange(newScoreValue);
    }

    public void OnLoadingStart() {
        loadingScreen.ShowLoadingScreen();
    }

    public void StartMatchCountdown(float duration) {
        loadingScreen.StartCountdown();
    }
}
