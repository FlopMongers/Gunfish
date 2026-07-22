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
