using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public enum QuipType { 
    Attractor,
    PlayerDeath,
    FishSelection,
    Player1Wins,
    Player2Wins,
    Player3Wins,
    Player4Wins,
    Team1Wins,
    Team2Wins,
    Tie,
    NoOneWins,
    Pelicans,
    Three,
    Two,
    One,
    Ready,
    Flop,
    GUNFISH,
    Team3Wins,
    Team4Wins,
}


public class MarqueeManager : PersistentSingleton<MarqueeManager> {
    [Serializable]
    private class Quip {
        public string text;
        public AudioClip clip;
    }

    [Serializable]
    private class QuipTuple {
        public QuipType QuipType;
        public List<Quip> quips = new List<Quip>();
    }

    [Serializable]
    private struct MarqueeData {
        public float duration;
        public AnimationCurve tween;
        public TMP_Text text;
        
        [HideInInspector]
        public float t;

        public void Init() {
            t = 1f;
            text.SetText("");
            text.rectTransform.anchoredPosition = new Vector2(-Screen.width, 0f);
        }
    }
    
    [SerializeField]
    private List<Quip> quips = new();

    [SerializeField]
    private List<QuipTuple> quipList = new List<QuipTuple>();

    Dictionary<QuipType, List<Quip>> quipMap = new Dictionary<QuipType, List<Quip>>();

    [SerializeField]
    private MarqueeData titleSettings;
    [SerializeField]
    private MarqueeData quipSettings;


    private void Start() {
        titleSettings.Init();
        quipSettings.Init();
        foreach (QuipTuple tuple in quipList) {
            quipMap[tuple.QuipType] = tuple.quips;
        }
    }

    private void Update() {
        UpdateData(ref titleSettings);
        UpdateData(ref quipSettings);
    }

    private void UpdateData(ref MarqueeData data) {
        if (data.t < 1f) {
            var tween = data.tween.Evaluate(data.t);
            var value = Mathf.Lerp(Screen.width, -Screen.width, tween);
            data.text.rectTransform.anchoredPosition = new Vector2(value, 0f);

            data.t += Time.deltaTime / data.duration;
        }
    }

    public void PlayTitle(string title) {
        titleSettings.text?.SetText(title);
        titleSettings.t = 0;
    }

    public void PlayRandomQuip(QuipType quipType) {
        if (quipMap == null || quipMap.ContainsKey(quipType) == false || quipMap[quipType].Count == 0) {
            Debug.LogWarning("Could not enqueue quip. Make sure you have at least one in the MarqueeManager");
            return;
        }

        var index = UnityEngine.Random.Range(0, quipMap[quipType].Count);
        PlayQuip(quipMap[quipType][index]);
    }

    private void PlayQuip(Quip quip) {
        ArduinoManager.Instance.PlayClip(quip.clip);
        quipSettings.text.SetText(quip.text);
        quipSettings.t = 0;
    }

    public void PlayPlayerWinQuip(Player player) {
        print(player.PlayerNumber);
        switch (player.PlayerNumber) {
            case 0:
                PlayRandomQuip(QuipType.Player1Wins);
                break;
            case 1:
                PlayRandomQuip(QuipType.Player2Wins);
                break;
            case 2:
                PlayRandomQuip(QuipType.Player3Wins);
                break;
            case 3:
                PlayRandomQuip(QuipType.Player4Wins);
                break;
        }
    }

    public void PlayWinQuip(ScoredTeamReference winnerTeam) {
        if (winnerTeam.players.Count == 1) {
            PlayPlayerWinQuip(winnerTeam.players[0].player);
        }
        else {
            switch (winnerTeam.teamNumber) {
                case 0:
                    PlayRandomQuip(QuipType.Team1Wins); break;
                case 1:
                    PlayRandomQuip(QuipType.Team2Wins); break;
                case 2:
                    PlayRandomQuip(QuipType.Team3Wins); break;
                case 3:
                    PlayRandomQuip(QuipType.Team4Wins); break;
            }
        }
    }

    void OnValidate() {
        
        for (int i = 0; i < quips.Count; i++)
        {
            quips[i].text = quips[i].clip.name.Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        }
    }
}
