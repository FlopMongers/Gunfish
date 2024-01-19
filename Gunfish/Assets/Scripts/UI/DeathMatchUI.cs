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
    [SerializeField] private CanvasGroup playerPanelsGroup;
    [SerializeField] private List<PlayerPanel> playerPanels;

    [Header("Preferences")]
    [SerializeField] private Color eliminatedColor;
    [SerializeField] private TextMeshProUGUI winnerText;

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

    void ClearPlayerPanels() {
        foreach (var panel in playerPanels) {
            panel.highlight.enabled = false;
            panel.panel.SetActive(false);
        }
    }

    public void ShowLevelStats(string text, Dictionary<Player, PlayerReference> playerRefs, string tiebreakerText) {
        winnerText.text = text;
        //(playerNumber == -1) ? "No one wins!" : $"{winnerEntity} {playerNumber} wins!";

        ClearPlayerPanels();

        int panelIdx = 0;
        foreach ((Player player, PlayerReference playerRef) in playerRefs.OrderByDescending(x => x.Value.score)) {
            playerPanels[panelIdx].playerName.text = $"Player {player.VisiblePlayerNumber}";
            playerPanels[panelIdx].playerImg.sprite = player.gunfishData.sprite;
            playerPanels[panelIdx].playerScore.text = playerRef.score.ToString();
            playerPanels[panelIdx].panel.SetActive(true);
            panelIdx++;
        }
        if (tiebreakerText != "") {
            playerPanels[0].tiebreakerText.text = tiebreakerText;
        }
        StopAllCoroutines();
        StartCoroutine(CoShowLevelStats(true));
    }

    public void CloseLevelStats() {
        StopAllCoroutines();
        StartCoroutine(CoShowLevelStats(false));
    }

    public void ShowFinalScores(string text, Dictionary<Player, PlayerReference> playerRefs, List<Player> winners, string tiebreakerText) {
        ClearPlayerPanels();

        int panelIdx = 0;
        foreach ((Player player, PlayerReference playerRef) in playerRefs.OrderByDescending(x => x.Value.score)) {
            playerPanels[panelIdx].playerName.text = $"Player {player.VisiblePlayerNumber}";
            playerPanels[panelIdx].playerImg.sprite = player.gunfishData.sprite;
            playerPanels[panelIdx].playerScore.text = playerRef.score.ToString();
            if (winners.Contains(player)) {
                playerPanels[panelIdx].highlight.enabled = true;
            }
            playerPanels[panelIdx].panel.SetActive(true);
            panelIdx++;
        }
        winnerText.text = text;
        if (tiebreakerText != "") {
            playerPanels[0].tiebreakerText.text = tiebreakerText;
        }

        StopAllCoroutines();
        StartCoroutine(CoShowLevelStats(true));
    }

    IEnumerator CoShowLevelStats(bool show) {
        float targetAlpha = (show) ? 1 : 0;
        float startAlpha = playerPanelsGroup.alpha;
        float timerDuration = 0.5f;
        float timer = timerDuration;
        while (timer > 0) {
            playerPanelsGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, 1 - (timer / timerDuration));

            timer -= Time.deltaTime;
            yield return null;
        }
        playerPanelsGroup.alpha = targetAlpha;
    }
}