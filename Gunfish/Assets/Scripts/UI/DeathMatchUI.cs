using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathMatchUI : MonoBehaviour {
    [Header("UI References")]

    public List<DeathMatchUIPlayerWidget> playerWidgets;

    [Header("Preferences")]
    [SerializeField] private Color eliminatedColor;

    // Start is called before the first frame update
    void Awake() {
        foreach (var playerWidget in playerWidgets) {
            playerWidget.gameObject.SetActive(false);
        }
    }


    public void InitializeMatch(List<Player> players) {
        foreach (Player player in players) {
            playerWidgets[player.PlayerNumber].InitializeMatch(player);
            playerWidgets[player.PlayerNumber].OnScoreChange(0);
        }
    }

    public void InitializeLevel(List<Player> players, int initialStockCount) {
        foreach (Player player in players) {
            var playerWidget = playerWidgets[player.PlayerNumber];
            playerWidget.gameObject.SetActive(true);
            var color = PlayerManager.Instance.playerColors[player.PlayerNumber];
            playerWidget.SetColor(color);
            playerWidget.InitializeLevel(initialStockCount, player);
        }
    }

    public void OnStockChange(Player player, int newStockValue) {
        DeathMatchUIPlayerWidget playerWidget = playerWidgets.Find((pwidget) => pwidget.player == player);
        playerWidget.OnStockChange(newStockValue);
        if (newStockValue == 0) {
            playerWidget.SetColor(eliminatedColor);
        }
    }

    public void OnScoreChange(Player player, int newScoreValue) {
        playerWidgets.Find((pwidget) => pwidget.player == player)?.OnScoreChange(newScoreValue);
    }
}