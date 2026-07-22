---
max_iterations: 4
---

<!--
max_iterations rationale: 4 (higher than the default 3). This plan touches five C# files
across three concerns (a new global singleton with per-slot delegate lifecycle, a
MonoBehaviour dual-mode refactor, and a scheme-resolution branch rewrite), plus a
required manual Editor step (adding a GameObject to a prefab) that the automated
implementer/tester loop cannot itself perform and must instead verify was done and
document clearly if not. Budgeting one extra iteration accounts for: (1) a first pass
implementing all code, (2) a pass fixing any compile issues from the per-slot delegate
closures or the new GetSprite branch ordering, (3) a pass verifying/adjusting the
manual-step documentation is unambiguous, and (4) headroom for one genuine retry if the
tester finds a behavioral issue in the cabinet-gating logic (the highest-risk, most
novel branch in this change).
-->

## Goal

Give `ControlSchemeIcon` a second operating mode — `IconSource.LastActiveDevice` —
that is fully self-sufficient (no external `Init()` call required) and resolves its
glyph from a new global `ActiveControlSchemeTracker` singleton tracking "whichever
joined player's device most recently produced any input," defaulting to a
`Keyboard&Mouse` fallback until real input is observed; this lets shared/global menu
screens (Splash, GameModeSelect, Pause/Settings) that wire every joined player's
`Submit`/`Cancel`/`Navigate` identically — where there is no single "the player" to
call `Init(player)` on — show a correct, live-updating control-scheme icon, while the
existing `IconSource.Player` mode (used by `Gunfish.cs`'s per-player fish HUD via
`Init(Player player)`) remains byte-for-byte behaviorally identical. In the same change,
rework `InputGlyphDatabase.GetSprite` to gate glyph-set selection on
`PlatformConfig.IsCabinet` rather than on the literal control-scheme string: off
cabinet, only `Keyboard&Mouse`/`Gamepad` are ever resolved (never `cabinetGlyphs`); on
cabinet, `Keyboard&Mouse` is treated as an Editor/debug-only fallback and *every other*
reported scheme name (`Gamepad`, `Joystick`, or anything else the cabinet's hardware
might enumerate as) is treated as "the cabinet controller," because the cabinet's
physical joystick/buttons may not reliably report as `"Joystick"` — while preserving the
existing "cabinet has no physical Menu button, so `(cabinet, Menu)` shows the keyboard
glyph" override, now nested inside the cabinet branch. Both `IconSource` modes funnel
through this same reworked `GetSprite`, so the cabinet-gating fix applies uniformly to
the fish HUD and to shared menu icons alike.

## Scope

- **Create** `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs` — new
  `PersistentSingleton<ActiveControlSchemeTracker>` that mirrors
  `SplashMenuPage`'s `WireSlot`/`UnwireSlot` + `PlayerManager.OnSlotJoined`/`OnSlotLeft`
  pattern, but wires once for the app's lifetime (via `Initialize()`, called from
  `GameManager`) rather than per-page-show, and tracks per-slot delegates so the
  `"Any"` action handler knows which `PlayerInput` fired. Exposes
  `CurrentScheme` (defaults `"Keyboard&Mouse"`) and `event Action<string> SchemeChanged`.
- **Modify** `Assets/Scripts/UI/ControlSchemeIcon.cs` — add `public enum IconSource {
  Player, LastActiveDevice }` and `[SerializeField] private IconSource source =
  IconSource.Player`; add `OnEnable`/`OnDisable` that subscribe/unsubscribe to
  `ActiveControlSchemeTracker.Instance.SchemeChanged` only when `source ==
  LastActiveDevice`, pulling `CurrentScheme` immediately on enable. `Init(Player player)`
  and the `Player`-mode code path are untouched.
- **Modify** `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs` —
  replace `GetSprite`'s scheme-string switch with a `PlatformConfig.IsCabinet` gate;
  nest the existing cabinet-Menu override inside the cabinet branch; document the
  `Gamepad`-dead-on-cabinet / `Joystick`-dead-off-cabinet consequence in a code comment.
- **Modify** `Assets/Scripts/Managers/GameManager.cs` — add
  `ActiveControlSchemeTracker.Instance.Initialize();` to
  `InitializePostRitualManagersCR()`, alongside the existing manager-initialize calls.
- **Manual Editor step (documented, not hand-edited)** — add a new empty child
  GameObject named `ActiveControlSchemeTracker` (with the new component) as a sibling of
  `PlayerManager`/`GameManager`/`ArduinoManager`/etc. inside
  `Assets/Resources/Prefabs/GameManager.prefab`, which is the prefab instantiated
  directly in `Assets/Scenes/MainMenu.unity` and is the established bootstrap location
  for every `PersistentSingleton`-derived manager in this project.
- **Create** `docs/superpowers/specs/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md`
  and `docs/superpowers/plans/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating.md`
  — new dated doc pair (rationale below) covering both parts of this change.
- **Not touched** (explicitly out of scope, confirmed by reading): `Gunfish.cs` (its
  `controlSchemeIcon.Init(player)` call site at line 387 needs no code change — it keeps
  compiling and behaving identically because `IconSource` defaults to `Player`),
  `Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab`,
  `Assets/Resources/Prefabs/UI/FishHealthUI.prefab`, `GameModeSelectMenuPage.cs`,
  `SplashMenuPage.cs`, `FishSelectMenuPage.cs`, `InputGlyphSet.cs`,
  `InputGlyphSetSeeder.cs`, `PlayerManager.cs`, `Player.cs`, `PlatformConfig.cs`,
  `DevConfigOverride.cs`.

## Design Decisions (rationale)

**`OnEnable`/`OnDisable`, confirmed against real code, not `Awake`/`OnDestroy`.**
Verified directly in `Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab`: the
`GameModeSelectPage` GameObject (fileID `388296117535746215`) and `FishSelectPage`
GameObject (fileID `388296115997575688`) both serialize `m_IsActive: 0` — they start
inactive, only `SplashPage` starts active. `MenuPage.OnPageStart`/`OnPageStop`
(`Assets/Scripts/UI/MenuPage.cs`) call `gameObject.SetActive(true)`/`SetActive(false)`,
and `MainMenu.CoSetState` (`Assets/Scripts/UI/MainMenu.cs`) drives this via
`currentPage.OnPageStart(context)` / `page.OnPageStop(context)` on every menu-state
transition. Because these page GameObjects start inactive, Unity defers `Awake` on
every component under them (including any `ControlSchemeIcon` child) until the page's
*first* activation — `Awake`/`OnDestroy` would therefore only ever fire once per
lifetime, missing every later re-show/re-hide. `OnEnable`/`OnDisable` fire on every
activation/deactivation (after the one-time `Awake`), which is exactly the granularity
needed to (a) avoid a stale/blank icon the first time a page is shown and (b)
cleanly unsubscribe every time the page is hidden rather than leaking a subscription
across repeated Splash→GameModeSelect→Splash cycles.

**Per-slot delegate tracking, not a single shared handler.**
`SplashMenuPage.OnAnyKey` (`Assets/Scripts/UI/SplashMenuPage.cs:54`) doesn't care which
`PlayerInput` fired — it just triggers a menu transition — so it binds the exact same
method reference to every slot and unsubscribes that same reference in `UnwireSlot`.
The tracker's handler *does* need to know which device fired (to read its
`currentControlScheme`), so a single shared method won't do. `FishSelectMenuPage.cs`
already solves this exact problem for its own per-player delegates by storing a
`PlayerAction` struct of three closures per slot in a `List<PlayerAction>` indexed by
`playerIndex`, so `UnwireSlot` can unsubscribe the *same* closure instance it
subscribed. The tracker mirrors that: `List<PlayerInput> wiredPlayerInputs` and
`List<Action<InputAction.CallbackContext>> wiredHandlers`, both indexed by slot, where
`WireSlot(i)` builds a closure capturing `playerInput` and stores it before
subscribing, and `UnwireSlot(i)` looks up and unsubscribes that stored instance. This
avoids the classic bug of subscribing a closure and unsubscribing a *different*
closure instance (which silently fails to unsubscribe and leaks).

**Wired once for the app's lifetime, not per-page-show.**
`SplashMenuPage`/`GameModeSelectMenuPage`/`FishSelectMenuPage` all wire/unwire their
slot subscriptions in `OnPageStart`/`OnPageStop` because *they* are the things being
shown and hidden. `ActiveControlSchemeTracker` is a `PersistentSingleton` with the same
lifetime as `GameManager`/`PlayerManager` — it should never unwire just because a menu
page happens to hide, since other pages (and the fish HUD, indirectly, if ever wired to
`LastActiveDevice`) may want the same tracked value at the same time. It wires once in
`Initialize()` and only unwires in `OnDestroy()` (which in practice never fires during
normal play, since `PersistentSingleton` never gets destroyed — this exists for
symmetry/domain-reload safety, matching how `Singleton<T>.OnDestroy()` clears
`Instance`).

**Bootstrapping matches the established `PersistentSingleton` convention exactly, not
an ad hoc one.** Traced the concrete mechanics: `Assets/Resources/Prefabs/GameManager.prefab`
(GUID `c367644c428800f4e8afcb2a496d487a`) is a single large prefab instantiated directly
in `Assets/Scenes/MainMenu.unity`, containing `GameManager`, `PlayerManager`,
`ArduinoManager`, `MusicManager`, `StatsManager`, `AudioManager`, `GameModeManager`,
`FX_Spawner`, `LevelManager`, and `MarqueeManager` as sibling child GameObjects —
confirmed by grepping each script's GUID against the prefab's `m_Script` references and
cross-referencing `m_Name:` lines. Every one of these managers follows the same
two-phase pattern: `PersistentSingleton<T>.Awake()` sets `Instance` + `DontDestroyOnLoad`
immediately on scene load (arbitrary sibling order), but real setup work happens in an
overridden `Initialize()`, explicitly invoked in a specific order by
`GameManager.InitializePostRitualManagersCR()` (`Assets/Scripts/Managers/GameManager.cs:108-123`).
Traced *why* that coroutine's timing is safe for our tracker: `ArcadeJoinStrategy.OnPlayerJoined`
only calls `GameManager.Instance.InitializePostRitualManagers()` once
`PlayerInputs.Count == playerThreshold` (all cabinet players already joined and
non-null), while `OnlineJoinStrategy.Initialize()` calls it immediately with a
pre-sized-but-all-null `PlayerInputs` list (PC players join later via
`OnSlotJoined`). Either way, by the time `ActiveControlSchemeTracker.Initialize()` runs,
`PlayerManager.Instance.PlayerInputs` is a valid (possibly all-null) list of the right
size, and subsequent `OnSlotJoined`/`OnSlotLeft` events cover every later change — this
is exactly the same shape `SplashMenuPage.OnPageStart` already relies on. Therefore: add
`ActiveControlSchemeTracker` as a **new sibling GameObject inside
`GameManager.prefab`** (Editor manual step — never hand-edit the prefab YAML, per
project convention: "Prefab / Scene files: Edit in the Unity Editor, not in a text
editor") and add one call to `InitializePostRitualManagersCR()`, matching every other
manager's wiring exactly.

**`GetSprite` branch structure: cabinet-gate first, Menu-override nested inside it, then
scheme-to-set resolution.** The cleanest composition is *not* "cabinet-gate × Menu-override
× scheme-set" as three independent orthogonal axes — it's a two-level decision where the
Menu-override is a sub-case that only exists *because* we're resolving for the cabinet:

```
IsCabinet?
├── true (cabinet hardware)
│   ├── scheme == "Keyboard&Mouse"?  →  keyboardGlyphs  (debug-only fallback; keyboard
│   │                                    genuinely has an Escape key, so Menu needs no
│   │                                    override here)
│   └── else (any other scheme — Gamepad, Joystick, anything)
│       ├── action == Menu?  →  keyboardGlyphs[Menu]  (explicit override: no physical
│       │                        Menu button on the cabinet's front panel)
│       └── else             →  cabinetGlyphs
└── false (not cabinet hardware)
    ├── scheme == "Keyboard&Mouse"?  →  keyboardGlyphs
    ├── scheme == "Gamepad"?         →  gamepadGlyphs
    └── else                        →  null → fallbackGlyph  (cabinetGlyphs never
                                        reachable off cabinet)
```

This preserves the pre-existing comment's reasoning ("the cabinet-Menu override is
checked before anything can short-circuit it") while relocating it to only apply when
we've already determined we're resolving cabinet hardware — it can no longer
accidentally fire for a real PC gamepad that happens to report some unexpected scheme
string, which the old flat `controlScheme == "Joystick" && action == Menu` check was
technically exposed to (harmless in practice since PC schemes are never literally
`"Joystick"`, but the new structure makes the invariant explicit rather than
coincidental).

**Explicit dead-code consequence, stated as intentional.** Under `IsCabinet == true`,
`gamepadGlyphs` is never referenced by any path through `GetSprite` — the cabinet branch
only ever selects `keyboardGlyphs` or `cabinetGlyphs`. The `gamepadGlyphs` field remains
on `InputGlyphDatabase` (still authored/seeded, since the same database asset is shared
across cabinet and non-cabinet builds) but is functionally dead weight in any build with
`ARCADE_CABINET` defined. Symmetrically, under `IsCabinet == false`, the literal string
`"Joystick"` is never checked anywhere and `cabinetGlyphs` is never selected — it too
remains authored on the shared asset but is dead in non-cabinet builds. This is the
correct intentional outcome of gating by build target instead of by scheme-string
identity (the entire point being that cabinet hardware may not reliably report as
`"Joystick"` in the first place, making a string-identity check unsound on real cabinet
hardware regardless of dead-code concerns).

**New dated doc pair instead of editing the existing 2026-07-22 docs.** The existing
`docs/superpowers/specs/2026-07-22-input-glyph-semantic-actions-design.md` and its
companion plan document a *completed*, internally-consistent unit of work (the fixed
`PlayerAction` enum + static sprite table), including a "Known limitation" note that is
still accurate and still pending (the `MainMenuCanvas.prefab` stale-serialized-int
issue). Splicing this change's very different concerns (a new global singleton +
lifecycle design, and a build-target gating rule) into that file would mix a
closed/historical design with new/forward-looking design in a way that makes neither
easy to read standalone, and risks accidentally editing text that documents *already
shipped* behavior (e.g. the Kenney file-mapping table, which does not change here). A
new dated pair keeps each doc scoped to one coherent unit of work, matching the
project's existing convention of one dated doc pair per design decision (evidenced by
the existing pair itself superseding an even earlier `2026-07-20` doc rather than
editing it in place, per that file's own "Background" section).

## Implementation Steps

### 1. Create `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs`

```csharp
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
```

Notes for the implementer:
- `PlayerInputs` indices in `WireSlot`/`UnwireSlot` are always within the range
  established at `Initialize()` time (the same invariant `SplashMenuPage` relies on) —
  no bounds-checking beyond what's shown is needed.
- `Any` action is confirmed present in both the `Player` and `UI` action maps of
  `Assets/Resources/Input/Gunfish.inputactions` (menus run players in the `UI` map via
  `PlayerManager.SetInputMode(PlayerManager.InputMode.UI)`), so
  `playerInput.currentActionMap.FindAction("Any")` resolves correctly regardless of
  which map is currently active on that `PlayerInput`.

### 2. Modify `Assets/Scripts/UI/ControlSchemeIcon.cs`

Replace the entire file:

```csharp
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
```

Why this preserves `Player`-mode compatibility: Unity serializes MonoBehaviour fields
by name, not declaration position, so adding the new `source` field does not disturb
any already-serialized `ControlSchemeIcon` component's existing field values (including
the two pre-existing components on `MainMenuCanvas.prefab`, which are NOT touched by
this plan). Since `IconSource.Player` is the first-declared enum value (`= 0`, the
implicit default), every pre-existing serialized instance — which has no `source` entry
at all yet — deserializes with `source == IconSource.Player`, i.e. unchanged behavior,
with zero prefab edits required for backward compatibility. `Init(Player player)`'s
signature, body, and the `OnDestroy`/`Unhook` cleanup path are byte-for-byte unchanged.

### 3. Modify `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs`

Replace the `GetSprite` method (keep the class's field declarations unchanged):

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Input Glyph Database", menuName = "Scriptable Objects/Input Glyph Database")]
public class InputGlyphDatabase : ScriptableObject {
    public InputGlyphSet keyboardGlyphs;
    public InputGlyphSet gamepadGlyphs;
    public InputGlyphSet cabinetGlyphs;
    public Sprite fallbackGlyph;

    public Sprite GetSprite(PlayerAction action, string controlScheme) {
        InputGlyphSet set;

        if (PlatformConfig.IsCabinet) {
            // Cabinet builds only ever have two real input sources: the cabinet's own
            // controller, and -- as an Editor/debug-only fallback (e.g. testing with the
            // simulated ArcadeCabinet build target while using a real keyboard) -- a
            // real keyboard. We deliberately do NOT gate on the literal string
            // "Joystick": the cabinet's physical joystick/buttons may enumerate to the
            // Input System as a generic gamepad-like device and report "Gamepad"
            // instead of "Joystick", so on cabinet hardware "not Keyboard&Mouse" means
            // "the cabinet," full stop -- matching by scheme-name string alone is not
            // trustworthy here.
            if (controlScheme == "Keyboard&Mouse") {
                set = keyboardGlyphs;
            } else {
                // Explicit override: the cabinet's physical front panel has no "Menu"
                // button. Per design, Menu on cabinet always shows the KEYBOARD glyph
                // (Esc) -- never the generic fallback sprite. cabinetGlyphs
                // deliberately has no Menu entry seeded (see
                // InputGlyphSetSeeder.CabinetEntries) so this branch is the only path
                // that can ever produce a cabinet-scheme Menu sprite.
                if (action == PlayerAction.Menu) {
                    if (keyboardGlyphs != null && keyboardGlyphs.TryGetSprite(PlayerAction.Menu, out var keyboardMenuSprite))
                        return keyboardMenuSprite;
                    return fallbackGlyph;
                }
                set = cabinetGlyphs;
            }
        } else {
            // Off cabinet, only a real keyboard or a real gamepad are meaningful
            // control schemes. cabinetGlyphs is never relevant on non-cabinet hardware,
            // so an unrecognized scheme (including a literal "Joystick", which is dead
            // off cabinet -- see design notes) falls through to the generic fallback.
            set = controlScheme switch {
                "Keyboard&Mouse" => keyboardGlyphs,
                "Gamepad" => gamepadGlyphs,
                _ => null,
            };
        }

        if (set == null) return fallbackGlyph;
        return set.TryGetSprite(action, out var sprite) ? sprite : fallbackGlyph;
    }
}
```

Consequence, stated explicitly (also documented in the new design doc): under
`IsCabinet == true`, `gamepadGlyphs` is never read by `GetSprite` (dead in cabinet
builds); under `IsCabinet == false`, the literal string `"Joystick"` is never checked
and `cabinetGlyphs` is never read (dead in non-cabinet builds). Both fields remain on
the shared `InputGlyphDatabase` asset since the same asset is used across both build
targets — this is intentional, not an oversight.

### 4. Modify `Assets/Scripts/Managers/GameManager.cs`

In `InitializePostRitualManagersCR()` (currently lines 112–123), add the tracker
initialize call. Placement: first in the list, since it depends only on
`PlayerManager` (already initialized earlier, in `GameManager.Initialize()`) and
nothing else in this list depends on it.

```csharp
    private IEnumerator InitializePostRitualManagersCR() {
        // Yielding for one frame is required due to a bug in Unity.
        yield return new WaitForEndOfFrame();
        ActiveControlSchemeTracker.Instance.Initialize();
        LevelManager.Instance.Initialize();
        MusicManager.Instance.Initialize();
        ArduinoManager.Instance.Initialize();
        FX_Spawner.Instance.Initialize();
        MarqueeManager.Instance.Initialize();
        PauseManager.Instance.Initialize();
        GameModeManager.Instance.Initialize();
        MainMenu.Instance.Initialize();
    }
```

### 5. Manual Editor step — add the tracker GameObject to `GameManager.prefab`

This is the one unavoidable prefab edit. Do it **in the Unity Editor**, never by
hand-editing the prefab's YAML (per project convention):

1. Open `Assets/Resources/Prefabs/GameManager.prefab` in Prefab Mode (double-click it
   in the Project window).
2. Right-click the prefab's root in the Hierarchy → Create Empty. Rename the new child
   GameObject to `ActiveControlSchemeTracker`.
3. Add the `ActiveControlSchemeTracker` component to it via Add Component.
4. Save the prefab (Ctrl+S while in Prefab Mode, or the Save button in the Prefab Mode
   toolbar).
5. `git status` should show only `Assets/Resources/Prefabs/GameManager.prefab` modified
   (plus no `.meta` churn beyond the new component's own additions) — this is expected
   and required for step 4 of Part A to function at runtime; do not skip it.

If this step is skipped, `ActiveControlSchemeTracker.Instance` will be `null` at the
`GameManager.InitializePostRitualManagersCR()` call site, throwing a
`NullReferenceException` at startup (or, if referenced first via a menu page's
`OnEnable`, via `Singleton<T>.Instance` being `null`). This is a loud, immediate,
unmissable failure in Play mode — not a silent one — so it is safe to defer this step
to a manual verification pass without risking an undetected regression.

### 6. Documentation — new dated doc pair

Create `docs/superpowers/specs/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md`:

```markdown
# Control Scheme Icon — Shared Menus & Cabinet Gating — Design

## Background

`ControlSchemeIcon` (see `2026-07-22-input-glyph-semantic-actions-design.md`) resolves
its glyph via `Init(Player player)`, subscribing to that one player's
`ControlSchemeChanged` event. This works for `Gunfish.cs`'s per-player fish HUD, where
there genuinely is one `Player` per icon. It does not work for shared/global menu
screens -- Splash, GameModeSelect, Pause/Settings -- where every joined player's
`Submit`/`Cancel`/`Navigate` is wired identically (see `GameModeSelectMenuPage.WireSlot`,
looping `PlayerManager.Instance.PlayerInputs`) and there is no single "the player" to
call `Init` on. Two `ControlSchemeIcon` components already hand-placed on
`MainMenuCanvas.prefab` (`ConfirmButtonIndicator`/`CancelButtonIndicator` on the
in-progress `GameModeSelectPage`) were found with their sprite never populating, for
exactly this reason.

Separately: cabinet builds only ever support the cabinet's own control scheme (with
keyboard as a debug-only backup, never a real "Gamepad" scheme), and the cabinet's
physical joystick/buttons may not reliably enumerate to the Input System as
`"Joystick"` -- they may report as a generic `"Gamepad"`-like scheme instead. The
previous `GetSprite` gated on the literal string `"Joystick"`, which is unsound on real
cabinet hardware.

## Goals

- A `ControlSchemeIcon.IconSource` mode flag: `Player` (existing, default, used by the
  fish HUD) or `LastActiveDevice` (new) -- "whichever joined player's device most
  recently produced any input," with a keyboard fallback before any input is observed.
- `LastActiveDevice` mode is fully self-sufficient: no external caller invokes
  anything; it subscribes/unsubscribes itself via `OnEnable`/`OnDisable`.
- A new global `ActiveControlSchemeTracker` singleton, wired once for the app's
  lifetime, that answers "what's the last-active control scheme across every joined
  player," reusing the `WireSlot`/`UnwireSlot` + `OnSlotJoined`/`OnSlotLeft` pattern
  already established by `SplashMenuPage`.
- `InputGlyphDatabase.GetSprite` gates glyph-set selection on
  `PlatformConfig.IsCabinet` (build target) rather than control-scheme-name string
  identity, since scheme names are not trustworthy on cabinet hardware.
- Preserve the existing cabinet-Menu override (no physical Menu button on cabinet) as
  a nested sub-case of the cabinet branch.

## Non-goals

- Changing `Gunfish.cs`'s existing `Init(player)` call site or the fish HUD's behavior
  in any way.
- Changing `MainMenuCanvas.prefab`, `FishHealthUI.prefab`, or any menu page script's
  existing slot-wiring logic (`SplashMenuPage`, `GameModeSelectMenuPage`,
  `FishSelectMenuPage` are all read-only references for this design, not edited).
- Rebinding UI / persistence -- unchanged non-goal from the prior design doc.
- Changing `InputGlyphSet`/`InputGlyphSetSeeder`'s shape -- the fixed 5-action table is
  unaffected; only which `InputGlyphSet` gets selected changes.

## Architecture

```
ControlSchemeIcon (IconSource.Player)                ControlSchemeIcon (IconSource.LastActiveDevice)
        │ Init(player) subscribes to                          │ OnEnable subscribes to
        ▼                                                       ▼
Player.ControlSchemeChanged                     ActiveControlSchemeTracker.SchemeChanged
        │                                                       │
        └──────────────────┬────────────────────────────────────┘
                            ▼
              InputGlyphDatabase.GetSprite(PlayerAction action, string controlScheme)
                            │  gates on PlatformConfig.IsCabinet, then resolves via
                            ▼
              InputGlyphSet (Keyboard / Gamepad / Cabinet) . TryGetSprite(...)
```

`ActiveControlSchemeTracker` mirrors `SplashMenuPage`'s per-slot wiring
(`PlayerManager.Instance.PlayerInputs`, `OnSlotJoined`/`OnSlotLeft`,
`playerInput.currentActionMap.FindAction("Any").performed`), but wires once in
`Initialize()` (invoked by `GameManager.InitializePostRitualManagersCR()`, alongside
every other `PersistentSingleton` manager) rather than per-page-show, since it is
itself a `PersistentSingleton` with app lifetime, not a page. Per-slot delegate
closures are tracked (`List<PlayerInput>` + `List<Action<InputAction.CallbackContext>>`,
both indexed by slot) so the handler can identify which `PlayerInput` fired --
`SplashMenuPage.OnAnyKey` doesn't need this since it doesn't care which slot fired.

`GetSprite`'s new branch structure gates on `PlatformConfig.IsCabinet` first:

```csharp
if (PlatformConfig.IsCabinet) {
    if (controlScheme == "Keyboard&Mouse") {
        set = keyboardGlyphs;                    // debug-only fallback
    } else {
        if (action == PlayerAction.Menu) {
            return keyboardGlyphs's Menu sprite;  // no physical Menu button
        }
        set = cabinetGlyphs;                      // "not keyboard" == "the cabinet"
    }
} else {
    set = controlScheme switch {
        "Keyboard&Mouse" => keyboardGlyphs,
        "Gamepad" => gamepadGlyphs,
        _ => null,                                // cabinetGlyphs never reachable
    };
}
```

Consequence (intentional, not an oversight): `gamepadGlyphs` is dead code under
`IsCabinet == true` (no path selects it); `cabinetGlyphs` and the literal string
`"Joystick"` are dead under `IsCabinet == false`. Gating on build target rather than
scheme-string identity is the whole point -- the cabinet's hardware may not reliably
report as `"Joystick"` in the first place.

## Testing

No automated test suite in this project -- manual verification in-editor:

- With `DevConfigOverride.Enabled` off (default, `Tools > Gunfish > Dev Config`),
  `PlatformConfig.IsCabinet` is `false` on a normal Editor Play session (unless an
  `ARCADE_CABINET` scripting define is set) -- confirm keyboard/gamepad glyphs resolve
  as before.
- Set `DevConfigOverride.Enabled = true` and `SimulatedBuildTargetOverride =
  ArcadeCabinet` via the Dev Config window -- confirm `PlatformConfig.IsCabinet` reads
  `true`, and `GetSprite(action, "Keyboard&Mouse")` still resolves `keyboardGlyphs`
  (debug fallback) while `GetSprite(action, "Gamepad")` and `GetSprite(action,
  "Joystick")` both resolve `cabinetGlyphs` (except `Menu`, which resolves the keyboard
  Menu glyph in both cases).
- Place a `ControlSchemeIcon` with `source = LastActiveDevice` on a shared menu page;
  confirm it shows the keyboard glyph before any input, then updates live as different
  joined players' devices produce input.
- Confirm `Gunfish.cs`'s `controlSchemeIcon.Init(player)` call site still compiles and
  the fish HUD icon updates exactly as before (`source` defaults to `Player`).
```

Create `docs/superpowers/plans/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating.md`:

```markdown
# Control Scheme Icon — Shared Menus & Cabinet Gating — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `ControlSchemeIcon.IconSource.LastActiveDevice`, backed by a new
`ActiveControlSchemeTracker` singleton, for shared/global menu screens with no single
per-icon `Player`; and rework `InputGlyphDatabase.GetSprite` to gate glyph-set
selection on `PlatformConfig.IsCabinet` instead of control-scheme-string identity, since
cabinet hardware may not reliably report as `"Joystick"`.

**Architecture:** See the companion design doc,
`2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md`.

## Global Constraints

- No automated test suite (confirmed in `CLAUDE.md`) -- every task's verification is a
  `dotnet build` compile check plus a manual in-Editor check.
- Never hand-edit prefab or scene files. `MainMenuCanvas.prefab` and
  `FishHealthUI.prefab` are out of scope. `GameManager.prefab` requires one manual,
  Editor-driven addition (Task 3) -- never edit its YAML directly.
- `IconSource.Player` mode and `Gunfish.cs`'s `Init(player)` call site must remain
  behaviorally identical (modulo the cabinet-gating fix in `GetSprite`, which applies
  uniformly to both modes).

## File Structure

- **Create** `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs` (Task 1).
- **Modify** `Assets/Scripts/UI/ControlSchemeIcon.cs` -- add `IconSource` mode (Task 2).
- **Modify** `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs` -- cabinet-gated
  `GetSprite` (Task 4).
- **Modify** `Assets/Scripts/Managers/GameManager.cs` -- initialize the tracker
  (Task 1).
- **Manual Editor step** -- add `ActiveControlSchemeTracker` GameObject to
  `Assets/Resources/Prefabs/GameManager.prefab` (Task 3).

---

### Task 1: `ActiveControlSchemeTracker.cs` + `GameManager.cs` wiring

**Files:**
- Create: `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs`
- Modify: `Assets/Scripts/Managers/GameManager.cs`

- [ ] **Step 1:** Create `ActiveControlSchemeTracker.cs` with the exact contents from
  the design doc / pipeline-plan Implementation Steps section 1.
- [ ] **Step 2:** In `GameManager.InitializePostRitualManagersCR()`, add
  `ActiveControlSchemeTracker.Instance.Initialize();` as the first line after the
  `yield return new WaitForEndOfFrame();`.
- [ ] **Step 3: Compile-check.** Run `dotnet build Assembly-CSharp.csproj` from
  `Gunfish/`. Expected: `0 Error(s)`. (Expected at this point: the build still succeeds
  even though nothing yet places the GameObject in the scene -- that failure mode is a
  *runtime* null-reference, not a compile error, and is covered by Task 3's manual
  verification.)
- [ ] **Step 4: Commit.**
  ```bash
  git add Assets/Scripts/Managers/ActiveControlSchemeTracker.cs Assets/Scripts/Managers/GameManager.cs
  git commit -m "feat: add ActiveControlSchemeTracker singleton for shared-menu control scheme icons"
  ```

---

### Task 2: `ControlSchemeIcon.cs` — dual-mode `IconSource`

**Files:**
- Modify: `Assets/Scripts/UI/ControlSchemeIcon.cs`

- [ ] **Step 1:** Replace the file with the exact contents from the pipeline-plan
  Implementation Steps section 2.
- [ ] **Step 2: Compile-check.** Run `dotnet build Assembly-CSharp.csproj`. Expected:
  `0 Error(s)`.
- [ ] **Step 3: Manual check -- `Player` mode unaffected.** Open
  `Assets/Resources/Prefabs/UI/FishHealthUI.prefab` (read-only inspection, do not save
  any change) and confirm its `ControlSchemeIcon` component still shows `Icon Source:
  Player` after this change (new field defaults correctly for pre-existing serialized
  data). Play the game with at least one joined player; confirm the fish HUD's control
  scheme icon still updates correctly on control-scheme change, exactly as before.
- [ ] **Step 4: Commit.**
  ```bash
  git add Assets/Scripts/UI/ControlSchemeIcon.cs
  git commit -m "feat: add ControlSchemeIcon.IconSource.LastActiveDevice for shared menu screens"
  ```

---

### Task 3: Manual Editor step — bootstrap the tracker in `GameManager.prefab`

**Files:**
- Manual Editor edit: `Assets/Resources/Prefabs/GameManager.prefab`

- [ ] **Step 1:** In the Unity Editor, open `GameManager.prefab` in Prefab Mode, add a
  new empty child GameObject named `ActiveControlSchemeTracker`, add the
  `ActiveControlSchemeTracker` component to it, and save the prefab. (See pipeline-plan
  Implementation Steps section 5 for the exact procedure.)
- [ ] **Step 2: Manual check -- singleton resolves at runtime.** Enter Play mode from
  `Assets/Scenes/MainMenu.unity` (or whichever scene bootstraps `GameManager.prefab`)
  and confirm no `NullReferenceException` is thrown referencing
  `ActiveControlSchemeTracker`. Add a temporary `Debug.Log(ActiveControlSchemeTracker.Instance.CurrentScheme)`
  if useful, confirm it logs `"Keyboard&Mouse"` before any input; press a key and
  confirm a `SchemeChanged` observer (or a temporary log in `OnAnyInput`) fires with the
  new scheme.
- [ ] **Step 3: Commit.**
  ```bash
  git add "Assets/Resources/Prefabs/GameManager.prefab"
  git commit -m "chore: bootstrap ActiveControlSchemeTracker alongside the other persistent managers"
  ```

---

### Task 4: `InputGlyphDatabase.cs` — cabinet-gated `GetSprite`

**Files:**
- Modify: `Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs`

- [ ] **Step 1:** Replace `GetSprite` with the exact contents from the pipeline-plan
  Implementation Steps section 3.
- [ ] **Step 2: Compile-check.** Run `dotnet build Assembly-CSharp.csproj`. Expected:
  `0 Error(s)`.
- [ ] **Step 3: Manual check -- non-cabinet (default).** With
  `DevConfigOverride.Enabled` off (or set to a non-`ArcadeCabinet` simulated target),
  confirm `PlatformConfig.IsCabinet == false`, and that a keyboard player's icons show
  `keyboardGlyphs` sprites, a gamepad player's icons show `gamepadGlyphs` sprites, and
  no icon ever shows a `cabinetGlyphs` sprite.
- [ ] **Step 4: Manual check -- simulated cabinet.** Open `Tools > Gunfish > Dev
  Config`, enable dev overrides, and set the simulated build target to
  `ArcadeCabinet`. Confirm `PlatformConfig.IsCabinet == true` (e.g. via a temporary log
  or the Inspector debug view). With a keyboard connected in the Editor, confirm
  `Keyboard&Mouse`-scheme icons still show `keyboardGlyphs` (debug fallback) and that
  simulating any other reported scheme name resolves to `cabinetGlyphs`, except `Menu`,
  which resolves to the keyboard `Menu` glyph in both the `"Gamepad"`- and
  `"Joystick"`-reported cases. Revert the simulated build target back to `WindowsPC`
  (or disable dev overrides) when done.
- [ ] **Step 5: Manual check -- `Init(player)` call site.** Confirm
  `Assets/Scripts/Player/Gunfish/Fish/Gunfish.cs:387`
  (`controlSchemeIcon.Init(player);`) still compiles with no changes required, and that
  the fish HUD icon behaves identically off-cabinet to before this change.
- [ ] **Step 6: Commit.**
  ```bash
  git add Assets/Scripts/ScriptableObjects/InputGlyphDatabase.cs
  git commit -m "refactor: gate InputGlyphDatabase.GetSprite by PlatformConfig.IsCabinet instead of scheme-string identity"
  ```

---

### Task 5: Documentation

**Files:**
- Create: `docs/superpowers/specs/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md`
- Create: `docs/superpowers/plans/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating.md`

- [ ] **Step 1:** Create both files with the exact contents from the pipeline-plan
  Implementation Steps section 6.
- [ ] **Step 2: Commit.**
  ```bash
  git add docs/superpowers/specs/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating-design.md docs/superpowers/plans/2026-07-22-control-scheme-icon-shared-menus-and-cabinet-gating.md
  git commit -m "docs: add design + plan for shared-menu control scheme icons and cabinet gating"
  ```

## Known limitations (carried forward, not introduced here)

- `MainMenuCanvas.prefab`'s two pre-existing `ControlSchemeIcon` components still have
  the stale `action` int values noted in the prior design doc's "Known limitation" --
  unaffected and unresolved by this plan. The user has stated they will manually flip
  these two components' `source` field to `LastActiveDevice` themselves once their
  in-progress prefab edits are done; this plan does not touch that prefab.
```

## Verification Criteria

- **Compile check (required, run after every task):**
  `dotnet build Assembly-CSharp.csproj` from `Gunfish/` (i.e.
  `C:\Users\ryana\Projects\game-dev\Gunfish\Gunfish\`). Expected output: contains
  `0 Error(s)` (warnings are acceptable if pre-existing).
- **`Gunfish.cs` backward compatibility:** `Assets/Scripts/Player/Gunfish/Fish/Gunfish.cs:387`
  (`if (controlSchemeIcon != null) controlSchemeIcon.Init(player);`) must compile with
  zero changes and, at runtime, produce identical fish-HUD icon behavior to before this
  change (verify by playing with a keyboard and, if available, a gamepad, and confirming
  each player's icon shows the correct scheme's glyph and updates on scheme change).
- **`IconSource` default-safety:** Inspect any pre-existing serialized
  `ControlSchemeIcon` component (e.g. on `FishHealthUI.prefab`, read-only) after adding
  the `source` field and confirm its Inspector shows `Icon Source: Player` (not blank,
  not `LastActiveDevice`) — proving the new field's default did not silently alter
  already-serialized data.
- **`ActiveControlSchemeTracker` runtime resolution:** After completing the manual
  Editor step (adding the GameObject to `GameManager.prefab`), entering Play mode from
  the game's real bootstrap scene must not throw any `NullReferenceException`
  referencing `ActiveControlSchemeTracker`. `ActiveControlSchemeTracker.Instance.CurrentScheme`
  must read `"Keyboard&Mouse"` before any player produces input, and must update (with
  `SchemeChanged` firing) after any joined player's device produces input via the `Any`
  action in either the `Player` or `UI` action map.
- **`LastActiveDevice` icon lifecycle:** Placing a `ControlSchemeIcon` with
  `source = LastActiveDevice` under a `MenuPage`-derived page that starts inactive
  (e.g. temporarily under `GameModeSelectPage`) must show a non-blank icon
  (keyboard fallback) the very first time that page becomes active — not a blank
  `Image` — proving `OnEnable` (not a missed `Awake`) is doing the subscribe-and-pull.
  Hiding and re-showing that page must not throw, double-subscribe (verify no duplicate
  sprite-update log lines if a temporary log is added), or leak references.
- **Cabinet gate reachability (must be checked in-Editor, not assumed from code
  reading):** Open `Tools > Gunfish > Dev Config`, enable dev overrides, set
  `Simulated Build Target` to `Arcade Cabinet`, and confirm (via a temporary
  `Debug.Log(PlatformConfig.IsCabinet)` or equivalent) that it now reads `true` in the
  Editor without needing an actual cabinet build. With that override active, confirm
  `InputGlyphDatabase.GetSprite(action, "Keyboard&Mouse")` still returns a
  `keyboardGlyphs` sprite (debug fallback preserved), and
  `InputGlyphDatabase.GetSprite(action, "Gamepad")` / `GetSprite(action, "Joystick")`
  both return the same `cabinetGlyphs` sprite for a given non-`Menu` action (proving the
  "not keyboard means cabinet, regardless of exact scheme string" gate works), except
  for `action == Menu`, where both must return the keyboard `Menu` sprite. Revert the
  simulated build target afterward so it doesn't affect unrelated testing.
- **Non-cabinet gate correctness:** With dev overrides disabled (or simulated target
  set to `WindowsPC`), confirm `PlatformConfig.IsCabinet == false` and that
  `GetSprite(action, "Joystick")` now returns `fallbackGlyph` (proving `cabinetGlyphs`
  is unreachable off cabinet, the stated intentional dead-code consequence), while
  `GetSprite(action, "Keyboard&Mouse")` and `GetSprite(action, "Gamepad")` are
  unaffected.
- **No forbidden files touched:** `git diff --stat` against the pre-change tree must
  show no changes to `Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab`,
  `Assets/Resources/Prefabs/UI/FishHealthUI.prefab`,
  `Assets/Scripts/UI/GameModeSelectMenuPage.cs`, `Assets/Scripts/UI/SplashMenuPage.cs`,
  `Assets/Scripts/UI/FishSelectMenuPage.cs`, `Assets/Scripts/ScriptableObjects/InputGlyphSet.cs`,
  or `Assets/Scripts/Editor/InputGlyphSetSeeder.cs`.

## Tester Feedback (Iteration 1)

**Legend:** [AUTOMATED] = actually run/verified against real repo state this pass.
[STATIC] = Editor/Play-mode step that cannot be run headlessly; verified instead by
reading the code as written and reasoning about its behavior. [MANUAL-PENDING] = the
one deliberately-deferred human Editor step — not counted as a failure.

1. **Compile check — [AUTOMATED] PASS.** Ran
   `dotnet build Assembly-CSharp.csproj` from `Gunfish/`. Output: `Build succeeded.
   0 Warning(s) 0 Error(s)`. Both `Assembly-CSharp-firstpass` and `Assembly-CSharp`
   built cleanly.

2. **`Gunfish.cs` backward compatibility — [AUTOMATED] PASS.** Read
   `Assets/Scripts/Player/Gunfish/Fish/Gunfish.cs:382-391`. Line 387 is unchanged:
   `if (controlSchemeIcon != null) controlSchemeIcon.Init(player);`. It compiled
   successfully against the new `ControlSchemeIcon.cs` (confirmed by the build above),
   and `Init(Player player)`'s signature/body/`Unhook()` cleanup path in the new file
   are byte-for-byte identical to the plan's specified replacement — no divergence
   found.

3. **`IconSource` default-safety — [AUTOMATED] PASS.** `ControlSchemeIcon.cs:19-22`
   declares `public enum IconSource { Player, LastActiveDevice }` — `Player` is first,
   so it is the implicit `0`/default. Spot-checked serialized YAML: `FishHealthUI.prefab`
   has no `ControlSchemeIcon` component at all yet (grep for both the literal string
   `ControlSchemeIcon` and the script's GUID `f3a67a52667fcc2479d4eb7b271a49d8` found
   nothing) — consistent with the plan's scope note that this prefab's wiring may not
   exist yet. Found a better real-world spot check instead: the two pre-existing
   `ControlSchemeIcon` components on `MainMenuCanvas.prefab` (`action: 1` /
   `action: 0` instances, GUID `f3a67a52667fcc2479d4eb7b271a49d8`) serialize only
   `targetImage`, `action`, `glyphDatabase` — no `source:` line present in either. This
   confirms pre-existing serialized instances will deserialize with `source ==
   IconSource.Player` (Unity fills unserialized fields with the C# default), i.e.
   unchanged behavior with zero prefab edits required.

4. **`ActiveControlSchemeTracker` code correctness — [AUTOMATED] PASS.** Read the full
   new file, `Assets/Scripts/Managers/ActiveControlSchemeTracker.cs`. Confirmed:
   `CurrentScheme` defaults to `"Keyboard&Mouse"` (line 18); `event Action<string>
   SchemeChanged` exists (line 19); `Initialize()` wires every current slot (loop over
   `PlayerManager.Instance.PlayerInputs.Count`) plus subscribes `OnSlotJoined`/
   `OnSlotLeft` to `WireSlot`/`UnwireSlot`; `WireSlot`/`UnwireSlot` use per-slot delegate
   tracking via parallel `List<PlayerInput> wiredPlayerInputs` /
   `List<Action<InputAction.CallbackContext>> wiredHandlers` indexed by slot — a local
   `Handler` closure is built and stored in the list *before* subscribing, and
   `UnwireSlot` looks up that exact stored instance to unsubscribe (not a fresh lambda),
   correctly avoiding the classic un-unsubscribable-closure bug. `OnAnyInput` only calls
   `SchemeChanged?.Invoke` when `scheme != CurrentScheme` (line 76 guards on both
   null/empty and equality before mutating `CurrentScheme` and firing). Also verified
   against `Assets/Scripts/Utils/Singleton.cs`: `Initialize()` throws `UnityException`
   if `Instance` doesn't exist yet, confirming the plan's claimed loud-failure behavior
   if the manual step (below) is skipped.

5. **`LastActiveDevice` icon lifecycle — [STATIC] PASS (reasoning only, not run in
   Play mode).** Read the new `ControlSchemeIcon.cs`. `OnEnable` (not `Awake`) drives
   the subscription — line 35-40 subscribes `ActiveControlSchemeTracker.Instance
   .SchemeChanged += UpdateIconFromScheme` and immediately calls
   `UpdateIconFromScheme(ActiveControlSchemeTracker.Instance.CurrentScheme)` in the same
   block, so a freshly-activated page's icon is populated (keyboard-fallback sprite,
   not blank) the instant it's shown, before any input occurs. `OnDisable` (line 42-46)
   guards with `ActiveControlSchemeTracker.InstanceExists` before unsubscribing,
   correctly avoiding a `NullReferenceException` during domain-reload/shutdown teardown
   ordering where the tracker singleton may already be gone. Both are gated on
   `source == IconSource.LastActiveDevice`, leaving `Player`-mode icons (which have no
   `OnEnable`/`OnDisable` behavior at all) untouched. This criterion requires actually
   entering Play mode and toggling page visibility to fully confirm no double-subscribe
   in practice; the code as written has no path that would double-subscribe (each
   `OnEnable` is preceded by exactly one `OnDisable` per Unity's activation lifecycle,
   and the handler reference `UpdateIconFromScheme` is a stable method group each time).

6. **Cabinet gate structure — [STATIC] PASS (reasoning + spot check against
   `PlatformConfig.cs`, not run in Play mode / Dev Config window).** Read the new
   `InputGlyphDatabase.GetSprite`. Branch structure matches the plan exactly:
   `PlatformConfig.IsCabinet` gates first (line 13). Inside the cabinet branch,
   `"Keyboard&Mouse"` → `keyboardGlyphs` (line 23-24); every other scheme falls into the
   `else` (line 25), where `action == PlayerAction.Menu` always short-circuits to the
   keyboard Menu sprite (or `fallbackGlyph` if unavailable, lines 32-35) *before*
   `set = cabinetGlyphs` (line 37) — so `Gamepad`, `Joystick`, or any other cabinet
   scheme string resolves to `cabinetGlyphs` except for the Menu-action override,
   exactly as specified. In the non-cabinet branch (line 44-48), only
   `"Keyboard&Mouse"`/`"Gamepad"` resolve to real sets via the `switch`; everything else
   including the literal string `"Joystick"` falls through `_ => null` to
   `fallbackGlyph` at line 51. Read `Assets/Scripts/Managers/PlatformConfig.cs`:
   `IsCabinet` checks `DevConfigOverride.TryGetSimulatedBuildTarget` first (Editor-only,
   via `#if UNITY_EDITOR`), returning `true` iff the simulated target is
   `ArcadeCabinet`; falls back to the `ARCADE_CABINET` scripting-define check otherwise.
   This matches exactly what the plan assumed and describes in its "Testing" section
   (Dev Config window override for Editor testing without a real cabinet build).

7. **`GameManager.cs` wiring — [AUTOMATED] PASS.** Confirmed
   `ActiveControlSchemeTracker.Instance.Initialize();` was added as the first statement
   in `InitializePostRitualManagersCR()` (`GameManager.cs:112-124`), after the
   `WaitForEndOfFrame` yield and before every other manager's `Initialize()` call.
   Traced the ordering claim concretely: `GameManager.Initialize()` (line 88-93) calls
   `PlayerManager.Instance.Initialize()` synchronously, and is itself invoked from
   `GameManager.Start()` (line 84-86) — i.e. it runs during normal Unity
   Awake/Start bootstrap of the `GameManager.prefab` instance in the scene.
   `InitializePostRitualManagersCR()` is only ever kicked off later, via
   `GameManager.Instance.InitializePostRitualManagers()`, called from
   `ArcadeJoinStrategy.cs:34` (after all cabinet players have joined) or
   `OnlineJoinStrategy.cs:15` (immediately, but after PC-mode player-list setup) — both
   call sites necessarily run after the scene's `GameManager`/`PlayerManager` Awake/Start
   has already completed. So `PlayerManager.Instance` is guaranteed valid by the time
   the tracker's `Initialize()` reads `PlayerManager.Instance.PlayerInputs.Count`,
   confirming the plan's stated reasoning holds.

8. **No forbidden files touched by this implementer run — [AUTOMATED] PASS.**
   `git status --porcelain` shows: `.claude/pipeline-plan.md` (this file, expected);
   `FishSelectPanel.prefab` and `Assets/Scenes/MainMenu.unity` (user's own live
   concurrent Editor edits, per given context, not this pipeline); `CabinetGlyphs.asset`
   / `GamepadGlyphs.asset` / `KeyboardGlyphs.asset` / `InputGlyphSetSeeder.cs` /
   `InputGlyphSet.cs` / two `2026-07-20` docs (leftover uncommitted work from an earlier
   pipeline run this session, per given context, not this implementation step); this
   plan's own new/modified files (`ActiveControlSchemeTracker.cs` untracked,
   `GameManager.cs`, `InputGlyphDatabase.cs`, `ControlSchemeIcon.cs` modified, plus the
   two new `2026-07-22-control-scheme-icon-...` docs and a stray untracked
   `2026-07-22-input-glyph-semantic-actions.md` plan doc). Specifically verified:
   - `Assets/Resources/Prefabs/GameManager.prefab` — **not present in `git status`
     output at all**, i.e. zero diff. Confirmed directly via
     `git diff --stat -- .../GameManager.prefab` returning empty output. The manual
     step has not been done, as expected/documented.
   - `Assets/Scripts/UI/GameModeSelectMenuPage.cs`, `SplashMenuPage.cs`,
     `FishSelectMenuPage.cs` — none appear in `git status` at all (not even
     pre-existing-dirty), so trivially untouched by this or any prior run.
   - `Assets/Scripts/ScriptableObjects/InputGlyphSet.cs` and
     `Assets/Scripts/Editor/InputGlyphSetSeeder.cs` — both are dirty (pre-existing, per
     given context), but grepped both files' full contents for `IconSource` and
     `ActiveControlSchemeTracker` and found zero matches in either — proving this
     implementer did not touch them for Part A/Part B of this plan.
   - `MainMenuCanvas.prefab` — dirty, but spot-checked the diff: changes are
     `m_AnchoredPosition` tweaks on existing RectTransforms and a brand-new
     `ConfirmArea` GameObject/RectTransform being added — UI layout work unrelated to
     `ControlSchemeIcon`/`IconSource` (the two pre-existing `ControlSchemeIcon`
     components' YAML, checked above for criterion 3, still has no `source:` line,
     confirming this run didn't add one). Consistent with "user's own live concurrent
     Editor edits."

**Manual Editor step (Task 3 — deferred, not a failure):**
`Assets/Resources/Prefabs/GameManager.prefab` does not yet contain an
`ActiveControlSchemeTracker` GameObject/component — confirmed via empty `git diff`
output on that file. This is the plan's explicitly-documented human-only Editor step
(never hand-edit prefab YAML) and was correctly *not* attempted by the implementer.
The code is verified ready for it: `ActiveControlSchemeTracker` compiles, exposes the
right public API (`CurrentScheme`, `SchemeChanged`, `Initialize()`), and
`GameManager.InitializePostRitualManagersCR()` already calls
`ActiveControlSchemeTracker.Instance.Initialize()` at the correct point in the
sequence. Until the manual step is performed, entering Play mode will throw a loud,
immediate `UnityException`/`NullReferenceException` at that call site — this is the
documented, expected, non-silent failure mode, not a regression.

**Summary:** All 7 automatable/static-reasoning criteria (1-7) PASS, plus the
"no forbidden files touched" criterion (8) PASS. Zero code/logic defects found. The
one manual Editor step (adding `ActiveControlSchemeTracker` to `GameManager.prefab`)
remains pending as documented and does not count as a failure.
