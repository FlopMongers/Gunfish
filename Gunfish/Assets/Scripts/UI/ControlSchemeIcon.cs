using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ControlSchemeIcon : MonoBehaviour {
    [SerializeField] private Image targetImage;
    [SerializeField] private string actionName = "Fire";
    [SerializeField] private InputGlyphDatabase glyphDatabase;

    private Player player;
    private InputAction action;

    private void Awake() {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    public void Init(Player player) {
        Unhook();
        this.player = player;
        action = player.input.actions.FindAction(actionName);
        player.ControlSchemeChanged += UpdateIcon;
        UpdateIcon(player.input);
    }

    private void UpdateIcon(PlayerInput input) {
        targetImage.sprite = glyphDatabase.GetSprite(action, input.currentControlScheme);
    }

    private void OnDestroy() {
        Unhook();
    }

    private void Unhook() {
        if (player != null) player.ControlSchemeChanged -= UpdateIcon;
    }
}
