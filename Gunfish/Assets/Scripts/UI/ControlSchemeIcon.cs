using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PlayerAction {
    Select,
    Back,
    Menu,
    Move,
    Fire,
}

public class ControlSchemeIcon : MonoBehaviour {
    // Player: tracks one specific Player's device, via Init(player) (fish HUD).
    // LastActiveDevice: tracks whichever joined player's device most recently produced
    // ANY input, via the global ActiveControlSchemeTracker singleton -- for shared menu
    // screens (Splash, GameModeSelect, Pause/Settings) where every joined player's
    // Submit/Cancel/Navigate is wired identically and there is no single "the player."
    public enum IconSource {
        Player,
        LastActiveDevice,
    }

    [SerializeField] private Image targetImage;
    [SerializeField] private PlayerAction action = PlayerAction.Fire;
    [SerializeField] private InputGlyphDatabase glyphDatabase;
    [SerializeField] private IconSource source = IconSource.Player;

    private Player player;

    private void Awake() {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    private void OnEnable() {
        if (source == IconSource.LastActiveDevice) {
            ActiveControlSchemeTracker.Instance.SchemeChanged += UpdateIconFromScheme;
            UpdateIconFromScheme(ActiveControlSchemeTracker.Instance.CurrentScheme);
        }
    }

    private void OnDisable() {
        if (source == IconSource.LastActiveDevice && ActiveControlSchemeTracker.InstanceExists) {
            ActiveControlSchemeTracker.Instance.SchemeChanged -= UpdateIconFromScheme;
        }
    }

    public void Init(Player player) {
        Unhook();
        this.player = player;
        player.ControlSchemeChanged += UpdateIcon;
        UpdateIcon(player.input);
    }

    private void UpdateIcon(PlayerInput input) {
        targetImage.sprite = glyphDatabase.GetSprite(action, input.currentControlScheme);
    }

    private void UpdateIconFromScheme(string controlScheme) {
        targetImage.sprite = glyphDatabase.GetSprite(action, controlScheme);
    }

    private void OnDestroy() {
        Unhook();
    }

    private void Unhook() {
        if (player != null) player.ControlSchemeChanged -= UpdateIcon;
    }
}
