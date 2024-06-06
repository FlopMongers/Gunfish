using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup playerPanelsGroup;
    [SerializeField] private List<PlayerPanel> playerPanels;

    [SerializeField] private TextMeshProUGUI winnerText;

    [SerializeField] private TextMeshProUGUI scoreColumnText;

    void ClearPlayerPanels() {
        foreach (var panel in playerPanels) {
            panel.highlightCanvasGroup.alpha = 0;
            panel.tiebreakerText.text = "";
            panel.panel.SetActive(false);
        }
    }

    public void ShowStats<PlayerReferenceType>(
        string text, 
        List<PlayerReferenceType> players,
        TeamReference winningTeam, 
        Dictionary<PlayerReferenceType, string> tiebreakerTextMap,
        string scoreColumnText="",
        Func<PlayerReferenceType, string> scoreLambda=null) where PlayerReferenceType : PlayerReference 
    {
        if (scoreColumnText != "") {
            this.scoreColumnText.text = scoreColumnText;
        }

        winnerText.text = text;
        if (winningTeam != null) {
            winnerText.color = winningTeam.teamColor;
        }

        ClearPlayerPanels();

        for (int i = 0; i < players.Count; i++) {
            if (players[i].team == winningTeam) {
                playerPanels[i].highlightCanvasGroup.alpha = 1;
            }
            playerPanels[i].playerName.text = $"Player {players[i].player.VisiblePlayerNumber} (Team {players[i].team.VisibleTeamNumber})";
            playerPanels[i].playerImg.sprite = players[i].player.gunfishData.sprite;
            playerPanels[i].playerScore.text = (scoreLambda != null) ? scoreLambda(players[i]) : players[i].GetStatsText();
            playerPanels[i].panelColor.color = players[i].team.teamColor;
            if (tiebreakerTextMap.ContainsKey(players[i])) {
                playerPanels[i].tiebreakerText.text = tiebreakerTextMap[players[i]];
            }
            playerPanels[i].panel.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(CoShowStats(true));
    }

    public void CloseStats() {
        StopAllCoroutines();
        StartCoroutine(CoShowStats(false));
    }

    IEnumerator CoShowStats(bool show) {
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
