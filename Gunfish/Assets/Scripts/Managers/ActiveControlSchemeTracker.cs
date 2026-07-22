using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// Tracks which control scheme (Keyboard&amp;Mouse / Gamepad / cabinet) most recently
/// produced ANY input, across every currently-joined player slot. Used by
/// ControlSchemeIcon's IconSource.LastActiveDevice mode for shared/global menu screens
/// (Splash, GameModeSelect, Pause/Settings) where multiple players can Submit/Cancel/
/// Navigate simultaneously and there is no single "the player" to key off of.
///
/// Mirrors the WireSlot/UnwireSlot + PlayerManager.OnSlotJoined/OnSlotLeft pattern
/// already used by SplashMenuPage, but wires once for this object's whole lifetime
/// (via Initialize(), invoked by GameManager alongside every other manager) rather
/// than per-menu-page-show/hide.
/// </summary>
public class ActiveControlSchemeTracker : PersistentSingleton<ActiveControlSchemeTracker> {
    public string CurrentScheme { get; private set; } = "Keyboard&Mouse";
    public event Action<string> SchemeChanged;

    private List<PlayerInput> wiredPlayerInputs = new List<PlayerInput>();
    private List<Action<InputAction.CallbackContext>> wiredHandlers = new List<Action<InputAction.CallbackContext>>();

    public override void Initialize() {
        base.Initialize();

        int slotCount = PlayerManager.Instance.PlayerInputs.Count;
        wiredPlayerInputs = new List<PlayerInput>(new PlayerInput[slotCount]);
        wiredHandlers = new List<Action<InputAction.CallbackContext>>(new Action<InputAction.CallbackContext>[slotCount]);

        for (int i = 0; i < slotCount; i++) {
            WireSlot(i);
        }

        PlayerManager.Instance.OnSlotJoined += WireSlot;
        PlayerManager.Instance.OnSlotLeft += UnwireSlot;
    }

    protected override void OnDestroy() {
        if (PlayerManager.InstanceExists) {
            PlayerManager.Instance.OnSlotJoined -= WireSlot;
            PlayerManager.Instance.OnSlotLeft -= UnwireSlot;
        }
        for (int i = 0; i < wiredPlayerInputs.Count; i++) {
            UnwireSlot(i);
        }
        base.OnDestroy();
    }

    private void WireSlot(int playerIndex) {
        var playerInput = PlayerManager.Instance.PlayerInputs[playerIndex];
        if (playerInput == null) return;

        // Capture playerInput per-slot so the handler knows which device fired --
        // unlike SplashMenuPage.OnAnyKey, which doesn't need to know which slot
        // triggered. Store the exact delegate instance so UnwireSlot can remove it.
        void Handler(InputAction.CallbackContext context) => OnAnyInput(playerInput);

        wiredPlayerInputs[playerIndex] = playerInput;
        wiredHandlers[playerIndex] = Handler;
        playerInput.currentActionMap.FindAction("Any").performed += Handler;
    }

    private void UnwireSlot(int playerIndex) {
        var playerInput = wiredPlayerInputs[playerIndex];
        var handler = wiredHandlers[playerIndex];
        if (playerInput != null && handler != null) {
            playerInput.currentActionMap.FindAction("Any").performed -= handler;
        }
        wiredPlayerInputs[playerIndex] = null;
        wiredHandlers[playerIndex] = null;
    }

    private void OnAnyInput(PlayerInput playerInput) {
        var scheme = playerInput.currentControlScheme;
        if (string.IsNullOrEmpty(scheme) || scheme == CurrentScheme) return;

        CurrentScheme = scheme;
        SchemeChanged?.Invoke(CurrentScheme);
    }
}
