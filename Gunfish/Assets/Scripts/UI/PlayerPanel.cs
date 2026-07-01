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

    private Player player;
    private bool delegated;

    void Start() {
        thumbsUp?.SetActive(false);
        thumbsDown?.SetActive(false);
        delegated = false;
    }

    void RatePositive() {
        thumbsUp?.SetActive(true);
        thumbsDown?.SetActive(false);
        GameModeManager.Instance.matchManagerInstance.SetPlayerRating(player, 1);
    }

    void RateNegative() {
        thumbsUp?.SetActive(false);
        thumbsDown?.SetActive(true);
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

    public void SetInput(Player player)
    {
        if (this.player)
        {
            this.player.input.actions.FindActionMap("EndLevel").FindAction("Navigate").performed -= Rate;
        }
        this.player = player;
        this.player.input.actions.FindActionMap("EndLevel").FindAction("Navigate").performed += Rate;
        delegated = true;
    }

    public void OnEnable()
    {
        if (!delegated && player)
        {
            this.player.input.actions.FindActionMap("EndLevel").FindAction("Navigate").performed += Rate;
        }
        delegated = true;
    }
       
    public void OnDisable()
    {
        if (delegated && player)
        {
            this.player.input.actions.FindActionMap("EndLevel").FindAction("Navigate").performed -= Rate;
        }
        delegated = true;
    }
}
