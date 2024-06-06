using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelectPanel : MonoBehaviour {
    public Image gameModeImage;
    public TMP_Text gameModeName;
    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public RectTransform confirmHint;
    public RectTransform cancelHint;

    private List<GameMode> gameModes;
    private int gameModeCount;
    private int gameModeIndex;

    private void Start() {
        gameModes = GameManager.Instance.GameModeList.gameModes;
        gameModeCount = gameModes.Count;
        gameModeIndex = 0;
    }

    public void NextGameMode() {
        gameModeIndex = (gameModeIndex + 1) % gameModeCount;
        UpdateGameMode();
    }

    public void PreviousGameMode() {
        gameModeIndex--;
        if (gameModeIndex < 0) {
            gameModeIndex = gameModeCount - 1;
        }
    }

    public void UpdateGameMode() {
        var gameMode = gameModes[gameModeIndex];
        gameModeName?.SetText(gameMode.name);
        gameModeImage.sprite = gameMode.image;
        GameManager.Instance.SetSelectedGameMode(gameMode.gameModeType);
    }
}
