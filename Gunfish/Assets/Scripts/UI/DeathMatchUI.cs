using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class PlayerPanel {
    public GameObject panel;
    public TextMeshProUGUI playerName;
    public Image playerImg;
    public TextMeshProUGUI playerScore;
    public Image highlight;

    public PlayerPanel(GameObject panel, TextMeshProUGUI playerName, Image playerImg, TextMeshProUGUI playerScore, Image highlight) {
        this.panel = panel;
        this.playerName = playerName;
        this.playerImg = playerImg;
        this.playerScore = playerScore;
        this.highlight = highlight;
    }
}

public class DeathMatchUI : MonoBehaviour {
    [SerializeField]
    private List<DeathMatchUIPlayerWidget> playerWidgets = new List<DeathMatchUIPlayerWidget>();
    //[SerializeField]
    //private LoadingCountdownUI loadingScreen;

    TextMeshProUGUI winnerText;
    List<PlayerPanel> playerPanels = new List<PlayerPanel>();
    CanvasGroup playerPanelsGroup;

    // Start is called before the first frame update
    void Awake() {
        foreach (var playerWidget in playerWidgets)
        {
            playerWidget.gameObject.SetActive(false);
        }

        winnerText = transform.FindDeepChild("WinnerText").GetComponent<TextMeshProUGUI>();
        playerPanelsGroup = transform.FindDeepChild("LevelStats").GetComponent<CanvasGroup>();
        for (int i = 0; i < 4; i++) {
            var panel = transform.FindDeepChild($"PlayerPanel{i}");
            playerPanels.Add(
                new PlayerPanel(
                    panel.gameObject, 
                    panel.FindDeepChild("PlayerName").GetComponent<TextMeshProUGUI>(),
                    panel.FindDeepChild("PlayerImg").GetComponent<Image>(),
                    panel.FindDeepChild("PlayerWins").GetComponent<TextMeshProUGUI>(),
                    panel.FindDeepChild("Highlight").GetComponent<Image>()
                    ));
        }
    }


    public void InitializeMatch(List<Player> players) {
        for (int i = 0; i < playerWidgets.Count; i++) {
            if (players.Count > i && players[i] != null) {
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

    void ClearPlayerPanels() {
        foreach (var panel in playerPanels) {
            panel.highlight.enabled = false;
            panel.panel.SetActive(false);
        }
    }

    public void ShowLevelStats(Player player, Dictionary<Player, int> playerScores) {
        winnerText.text = (player == null) ? "No one wins!" : $"Player {player.playerNumber} wins!";

        ClearPlayerPanels();

        int playerIdx = 0;
        foreach (var playerScore in playerScores.OrderByDescending(x => x.Value)) {
            playerPanels[playerIdx].playerName.text = $"Player {playerScore.Key.playerNumber}";
            playerPanels[playerIdx].playerImg.sprite = playerScore.Key.gunfishData.sprite;
            playerPanels[playerIdx].playerScore.text = playerScore.Value.ToString();
            playerPanels[playerIdx].panel.SetActive(true);
            playerIdx++;
        }
        StopAllCoroutines();
        StartCoroutine(CoShowLevelStats(true));
    }

    public void CloseLevelStats() {
        StopAllCoroutines();
        StartCoroutine(CoShowLevelStats(false));
    }

    public void ShowFinalScores(Dictionary<Player, int> playerScores) {
        ClearPlayerPanels();

        int playerIdx = 0;
        int topScore = 0;
        List<Player> winners = new List<Player>();
        foreach (var playerScore in playerScores.OrderByDescending(x => x.Value)) {
            playerPanels[playerIdx].playerName.text = $"Player {playerScore.Key.playerNumber}";
            playerPanels[playerIdx].playerImg.sprite = playerScore.Key.gunfishData.sprite;
            playerPanels[playerIdx].playerScore.text = playerScore.Value.ToString();
            if (playerScore.Value >= topScore) {
                playerPanels[playerIdx].highlight.enabled = true;
                winners.Add(playerScore.Key);
                topScore = playerScore.Value;
            }
            playerPanels[playerIdx].panel.SetActive(true);
            playerIdx++;
        }

        if (winners.Count == 0) {
            winnerText.text = "No one wins?";
        }
        else if (winners.Count == 1) {
            winnerText.text = $"Player {winners[0].playerNumber} wins!!!";
        }
        else {
            winnerText.text = "It's a tie!";
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
