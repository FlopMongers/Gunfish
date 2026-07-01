# Quick Launch Debug Tool — Design

## Context

Testing anything in Gunfish (a gunfish, a gamemode, a map) today requires going through the full production menu flow (main menu → gamemode select → fish select → level) every single time, even mid-iteration. `GameManager.debug` already relaxes the player-count gate, `DevConfigOverride` (Editor-only) can swap which `GameModeList`/`GunfishDataList` is active, and `DebugRegistrar` (F3) surfaces what's currently overridden — but there's no way to jump directly to a specific gamemode/fish/level combination from wherever you currently are, including mid-match.

This spec adds a **Quick Launch** tool: an in-game, keyboard-driven wizard (hotkey **F4**), active only when `GameManager.debug == true`, that walks through four required steps and then performs a full clean restart into the chosen configuration.

## Goals

- Jump to any gamemode + level (scoped to that gamemode) + per-player fish, from anywhere: main menu, mid-match, anywhere.
- Always let the tester change which connected controllers are active for the next run.
- Remember the last submitted picks so re-launching (or tweaking one step) is fast.
- Never touch production code paths when `GameManager.debug == false` (same safety guarantee as `DevConfigOverride`/`DebugRegistrar`).

## Non-goals

- Detecting brand-new controllers joining mid-session. The engine currently only runs `PlayerManager.InitializePlayers()` once, at the boot join-threshold (`PlayerManager.OnPlayerJoined`). Quick Launch only toggles which of the *already-known* connected controllers are active; it does not add controller hot-join support.
- Live hot-swapping fish/level without tearing down the running match. Every submit is a full clean restart (teardown → reconfigure → reinitialize).
- Project-wide level browsing. Level choices are scoped to the selected gamemode's `SceneList` (`gameMode.levels.sceneNames`), matching how levels are modeled everywhere else in the codebase.
- Cross-session persistence of the last picks (e.g. via `EditorPrefs`). Picks are remembered in memory for the current play session only.

## Architecture

### `QuickLaunchManager` (new)

`Assets/Scripts/Managers/QuickLaunchManager.cs` — `PersistentSingleton<QuickLaunchManager>`, self-bootstrapped via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`, mirroring `DebugRegistrar`'s bootstrap so it's available before any scene's `Awake()` runs, with no manual scene/prefab wiring.

- `Update()`: no-ops entirely unless `GameManager.InstanceExists && GameManager.Instance.debug`. When active, polls `Keyboard.current` for `f4Key.wasPressedThisFrame` to toggle the wizard open/closed.
- All UI is constructed at runtime in code (`new GameObject` + `AddComponent<Canvas>()` / `AddComponent<TextMeshProUGUI>()`), the same way `DebugRegistrar.EnsureOverlayCreated()` builds its overlay. No new prefab or scene asset is authored — prefab/scene authoring is an Editor-UI action outside what this tool changes via file edits, and a runtime-built UI sidesteps needing an `EventSystem`/`InputSystemUIInputModule` that could collide with the 4 players' `PlayerInput` action maps.
- Navigation is direct `Keyboard.current` polling (arrow keys/WASD to move the highlighted option, Enter/Space to confirm a step or toggle a controller, Backspace to go back a step, Escape to close without submitting), the same low-level Input System pattern already used by `DebugRegistrar` and precedented by `PauseManager`/`LifePreserver`.

### Step flow

A `Step { Controllers, GameMode, Fish, Level }` state machine. Each step must have a valid selection before advancing to the next; Backspace returns to the previous step to edit it. All 4 steps must be satisfied before Submit is reachable.

1. **Controllers** — lists every entry in `PlayerManager.Instance.PlayerInputs`; Enter/Space toggles each controller's inclusion in the next run.
2. **Game mode** — options come from `GameManager.Instance.GameModeList.gameModes`.
3. **Fish per active controller** — one selector per controller marked active in step 1, options from `GameManager.Instance.GunfishDataList.gunfishes`.
4. **Level** — options are `gameMode.levels.sceneNames` for the gamemode chosen in step 2 (scoped, not project-wide).

**Selection memory:** the last *submitted* combination (active controller set, gamemode, per-controller fish, level) is kept in an in-memory field on `QuickLaunchManager` for the current play session. Reopening the wizard (F4) pre-fills all 4 steps with it, so an unchanged relaunch is just "open → confirm through 4 steps → submit," and changing one step only requires editing that step.

### Submit sequence (full clean restart)

Executed only once all 4 steps are valid and the tester confirms on the Level step:

1. If a match is currently running (`GameModeManager.Instance.matchManagerInstance != null`) → call `GameManager.Instance.ResetGame()` (existing teardown path: unsubscribes `LevelManager` events, resets every player's fish/active state, logs match results).
2. For every connected player index, call `PlayerManager.Instance.SetPlayerFish(i, <chosen fish> or null)` — `null` for controllers not marked active in step 1. This single existing call handles both fish assignment and the active/inactive toggle (`SetPlayerFish` already sets `Players[i].Active = data != null`), so no new active-flag state is introduced.
3. `GameManager.Instance.SetSelectedGameMode(<chosen gamemode>)`.
4. `GameManager.Instance.InitializeGame(forcedLevels)` — see below for the new optional parameter — passing the single chosen level path.
5. Close the wizard overlay.

This reuses the exact scene-load path (`LevelManager.LoadLevel`, `SceneManager.LoadSceneAsync(_, LoadSceneMode.Single)`) already used by the production flow, so it behaves identically whether invoked from the main menu or mid-match.

### `forcedLevels` — minimal plumbing change

Two existing methods gain an optional parameter, defaulting to `null` so every existing call site is unaffected:

- `GameManager.InitializeGame(List<string> forcedLevels = null)` — passes `forcedLevels` through to `GameModeManager.InitializeGameMode`.
- `GameModeManager.InitializeGameMode(GameMode gameMode, List<Player> players, List<string> forcedLevels = null)` — uses `forcedLevels ?? SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch)` instead of always randomizing.

Quick Launch always passes a single-element list (the one chosen level), so `GameParameters.scenes.Count == 1` and the match plays exactly that level before returning through the normal end-of-match flow (`EndLastLevel` → stats → main menu) — no forced multi-round rotation.

## Error handling

- Empty/misconfigured `GameModeList`, `GunfishDataList`, or a gamemode with zero levels: the relevant step shows an inline message ("No game modes configured" / "This gamemode has no levels") instead of crashing, and blocks advancing past that step.
- No controllers connected yet (e.g. opening Quick Launch at the very first frame of `MainMenu` before boot join threshold is hit): the Controllers step shows "no players connected" and blocks advancing.
- Any exception thrown while resolving a display value (e.g. a null asset reference) is caught and shown inline, matching the existing try/catch pattern in `DebugRegistrar`'s tracked-value rendering.

## Testing (manual — no automated suite in this project)

1. Open F4 at `MainMenu` before any controller has joined — Controllers step shows "no players connected," can't advance.
2. After boot, open F4, walk all 4 steps, submit — lands in the chosen level with the chosen fish per active player, plays exactly one round (no auto-continuation into a second randomly-picked level).
3. Mid-match, open F4, pick a different combination, submit — running match tears down cleanly (no duplicate `matchManagerInstance`, no leaked event subscriptions) and the new one starts fresh.
4. Toggle a controller inactive in step 1, submit — that player is excluded from spawning/scoring, same as unchecking them in the normal Fish Select flow.
5. Reopen F4 after a submit — all 4 steps are pre-filled with the last picks.
6. With `GameManager.debug == false`, confirm F4 does nothing at all (production safety parity with the F3 `DebugRegistrar` overlay).
