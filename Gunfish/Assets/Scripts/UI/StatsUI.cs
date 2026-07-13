 using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup playerPanelsGroup;
    [SerializeField] public List<PlayerPanel> playerPanels;

    [SerializeField] private TextMeshProUGUI winnerText;

    [SerializeField] private TextMeshProUGUI scoreColumnText;
    [SerializeField] private Transform feedbackPanel;

    void ClearPlayerPanels() {
        foreach (var panel in playerPanels) {
            panel.panelComponent.BorderColor = Color.black;
            panel.tiebreakerText.text = "";
            panel.gameObject.SetActive(false);
        }
    }

    public void ShowStats<PlayerReferenceType>(
        string text, 
        List<PlayerReferenceType> players,
        TeamReference winningTeam, 
        Dictionary<PlayerReferenceType, string> tiebreakerTextMap,
        string scoreColumnText="",
        bool final=false,
        Func<PlayerReferenceType, string> scoreLambda=null,
        bool showTeam=true) where PlayerReferenceType : PlayerReference 
    {
        if (scoreColumnText != "") {
            this.scoreColumnText.text = scoreColumnText;
        }

        winnerText.text = text;
        if (winningTeam != null) {
            winnerText.color = winningTeam.teamColor;
        }

        feedbackPanel.gameObject.SetActive(final);

        ClearPlayerPanels();

        for (int i = 0; i < players.Count; i++) {
            playerPanels[i].playerName.text = $"Player {players[i].player.VisiblePlayerNumber}" + (showTeam ? $"(Team {players[i].team.VisibleTeamNumber})" : "");
            playerPanels[i].playerImg.sprite = players[i].player.gunfishData.sprite;
            playerPanels[i].playerScore.text = (scoreLambda != null) ? scoreLambda(players[i]) : players[i].GetStatsText();
            playerPanels[i].panelComponent.BorderColor = players[i].team.teamColor;
            playerPanels[i].SetInput(players[i].player);
            if (tiebreakerTextMap.ContainsKey(players[i])) {
                playerPanels[i].tiebreakerText.text = tiebreakerTextMap[players[i]];
            }
            playerPanels[i].gameObject.SetActive(true);
            /*
            var playerInput = PlayerManager.Instance.PlayerInputs[i];
            if (!playerInput)
                continue;
            playerInput.currentActionMap.FindAction("Navigate").performed += playerPanels[i].Rate;
            */
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
