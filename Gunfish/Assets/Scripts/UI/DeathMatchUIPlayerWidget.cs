using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Death Match UI Player Widget encapsulates the UI for one active player during deathmatch.
/// It displays their score and stock count, and shows which fish they are.
/// </summary>
public class DeathMatchUIPlayerWidget : MonoBehaviour {
    public Player player;
    [SerializeField]
    private TMP_Text stockText;
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private Image playerPortrait;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    /// <summary>
    /// At the start of the match, initialize the player widget with the provided player and set score to 0.
    /// If the player slot is empty, don't call this method and instead disable this GameObject.
    /// </summary>
    public void InitializeMatch(Player player) {
        SetPlayer(player);
        scoreText.text = "0";
    }

    /// <summary>
    /// At the start of the level, initialize the player widget with the provided player and stock value.
    /// If the player slot is empty, don't call this method and instead disable this GameObject.
    /// </summary>
    public void InitializeLevel(int stockValue, Player player) {
        SetPlayer(player);
        stockText.text = stockValue.ToString();
    }

    public void SetPlayer(Player player) {
        this.player = player;
        playerPortrait.overrideSprite = player.gunfishData.sprite;
    }

    public void OnStockChange(int newStockValue) {
        if (newStockValue == -1) {
            OnPlayerEliminated();
        }
        scoreText.text = newStockValue.ToString();
        // TODO trigger anim
    }

    public void OnScoreChange(int newScoreValue) {
        scoreText.text = newScoreValue.ToString();
        // TODO update score value, trigger anim
    }

    private void OnPlayerEliminated() {
        // TODO gray out widget color
    }
}
