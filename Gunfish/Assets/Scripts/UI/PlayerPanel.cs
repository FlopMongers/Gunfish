using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour {
    [Range(0, 3)]
    public int playerNumber;
    public GameObject panel;
    public TextMeshProUGUI playerName;
    public Image playerImg;
    public TextMeshProUGUI playerScore;
    public CanvasGroup highlightCanvasGroup;
    public TextMeshProUGUI tiebreakerText;
    public Image panelColor;
    public GameObject thumbsUp;
    public GameObject thumbsDown;

    void Start() {
        thumbsUp?.SetActive(false);
        thumbsDown?.SetActive(false);
    }

    void RatePositive() {
        thumbsUp?.SetActive(true);
        thumbsDown?.SetActive(false);
        var player = PlayerManager.Instance.Players[playerNumber];
        GameModeManager.Instance.matchManagerInstance.SetPlayerRating(player, 1);
    }

    void RateNegative() {
        thumbsUp?.SetActive(false);
        thumbsDown?.SetActive(true);
        var player = PlayerManager.Instance?.Players[playerNumber];
        GameModeManager.Instance?.matchManagerInstance?.SetPlayerRating(player, -1);
    }

    public void Rate(InputAction.CallbackContext context) {
        var direction = context.ReadValue<Vector2>();

        // Horizontal
        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x)) {
            FX_Spawner.Instance.SpawnFX(FXType.MenuBloop, Camera.main.transform.position, Quaternion.identity);
            if (direction.y > 0) {
                RatePositive();
            } else {
                RateNegative();
            }
        }
    }
}
