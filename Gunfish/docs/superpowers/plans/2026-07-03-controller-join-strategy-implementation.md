# Controller Join Strategy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor `PlayerManager`'s join/leave/device-lost handling into a pluggable `IPlayerJoinStrategy`, so the arcade build keeps today's fixed-order "ritual" byte-for-byte, while the online build gets free join/leave at the fish-select lobby with same-device mid-match reconnect.

**Architecture:** `PlayerManager` stops implementing join logic itself. It holds one `IPlayerJoinStrategy` instance, chosen once in `Initialize()` via `#if GUNFISH_ARCADE`/`#else`, and delegates every entry point (`OnPlayerJoined`, `OnPlayerLeft`, device-lost/regained) to it. `ArcadeJoinStrategy` is a verbatim extraction of today's behavior. `OnlineJoinStrategy` pre-sizes `Players`/`PlayerInputs`/`PlayerFish` to 4 fixed "port" slots (index = stable slot number, `null` = empty) so a lobby-time leave leaves a visible gap rather than reshuffling other players' colors/panels — mid-match, a `Player` whose device is lost is never removed from its slot, which is what makes reconnecting the same device resume control with no extra bookkeeping.

**Tech Stack:** Unity 6000.5.0f1, C#, Unity Input System `PlayerInput`/`PlayerInputManager`.

## Global Constraints

- Max player count is a fixed global constant of **4** for both builds (from `PlayerInputManager.maxPlayerCount` on the `PlayerManager` GameObject inside `Assets/Resources/Prefabs/GameManager.prefab`) — do not add UI or logic to change this.
- Online build: fish-select always shows all 4 controller slots. A lobby-time leave (or an unresolved mid-match disconnect carried into the next lobby) leaves a **gap** in that slot until the same or a different device fills it — slots never compact/reshuffle.
- Arcade build (`GUNFISH_ARCADE` compile define) must behave **byte-for-byte identically** to today — this is a live revenue machine, zero regression tolerance.
- No automated test suite exists in this project (confirmed in `CLAUDE.md`) — every task's verification is a `dotnet build` compile check plus, where noted, a manual in-Editor check.
- Never hand-edit `ProjectSettings/*.asset` or `.meta` files — the one Project Settings change this plan requires (adding the `GUNFISH_ARCADE` scripting define symbol) must be done through the Player Settings Editor UI, not by editing `ProjectSettings/ProjectSettings.asset` directly.
- `PlayerManager`'s existing public surface (`Players`, `PlayerInputs`, `PlayerFish`, `SetPlayerFish`, `SetInputMode`, `playerColors`) keeps its exact field names and types so `GameManager`, `FishSelectMenuPage`, `GameModeManager`, `QuickLaunchManager`, `SplashMenuPage`, `GameModeSelectMenuPage`, `DeathMatchManager`, `RaceMatchManager`, and `MatchManager` don't need structural changes beyond the null-guards this plan adds explicitly.
- `PlayerManager.playerThreshold` is dead public API today (nothing outside `PlayerManager.cs` reads it, confirmed by repo-wide grep) — it is intentionally **not** preserved; its value moves to a private field inside `ArcadeJoinStrategy`.

## Design decisions made while planning (read before objecting to a task)

- **Fixed slots with holes, not a compacting list.** The spec's own wording ("assign the first free slot (0-3)") implies stable ports, but `PlayerManager.PlayerInputs` is read as a live "joined count" via `.Count` in five other files (`GameModeSelectMenuPage`, `SplashMenuPage`, `QuickLaunchManager`, `DeathMatchManager`, `StatsUI`). Confirmed with the project owner: the online build must always show 4 slots and simply leave a gap when a player leaves, rather than compacting indices. `GameModeSelectMenuPage`/`SplashMenuPage` already null-guard their `PlayerInputs` iteration (`if (!playerInput) continue;`), so they need **no changes**. `QuickLaunchManager` does raw indexing without a null guard and does need fixing (Task 4). `DeathMatchManager`/`StatsUI`'s only `PlayerInputs` references are inside `/* ... */` block comments (dead code) — no changes needed there.
- **`GameModeManager` also reads raw `PlayerManager.Instance.Players` and must be null-guarded (Task 5).** `GameManager.InitializeGame()` passes `PlayerManager.Instance.Players` straight into `GameModeManager.InitializeGameMode(gameMode, players, ...)`, which does `players.Where(player => player.Active)` — an NPE on any empty slot. `GameModeManager.TeardownGameMode()` has the identical unconditional-`SetPlayerFish`-over-every-index bug found in `QuickLaunchManager`. Everything downstream of that point (`MatchManager`, `RaceMatchManager`, `DeathMatchManager`, `BassballMatchManager`, `PelicanSpawner`) only ever touches the already-filtered `GameParameters.activePlayers`/`GameModeManager.activePlayers`, never the raw list, so fixing these two spots is sufficient — confirmed by a repo-wide grep for every `.Players`/`activePlayers` reference.
- **`GameManager.Instance.InitializePostRitualManagers()` timing differs per strategy.** Today it only fires once the arcade ritual's fixed threshold is met. Online has no threshold, so `OnlineJoinStrategy.Initialize()` fires it immediately (boots `LevelManager`/`MusicManager`/`MainMenu`/etc. before any controller joins, so a menu is visible for players to join into) — `ArcadeJoinStrategy` keeps firing it exactly where it fires today (inside `OnPlayerJoined` once the threshold is hit).
- **`IPlayerJoinStrategy` gains two members beyond the spec's four** (`Initialize(PlayerManager owner)` and `OnGUI()`), because the ritual's `OnGUI` prompt and both strategies' list setup have to live somewhere, and strategies are plain C# objects (not `MonoBehaviour`s) instantiated by `PlayerManager`.

---

### Task 1: Strategy pattern core — `IPlayerJoinStrategy`, `ArcadeJoinStrategy`, `OnlineJoinStrategy`, refactored `PlayerManager`

**Files:**
- Create: `Assets/Scripts/Managers/IPlayerJoinStrategy.cs`
- Create: `Assets/Scripts/Managers/ArcadeJoinStrategy.cs`
- Create: `Assets/Scripts/Managers/OnlineJoinStrategy.cs`
- Modify: `Assets/Scripts/Managers/PlayerManager.cs` (full rewrite of the class body — see below)

**Interfaces:**
- Consumes: `PersistentSingleton<T>` (`Assets/Scripts/Utils/PersistentSingleton.cs`), `GameManager.Instance.debug` / `.debugPlayerCount` / `.InitializePostRitualManagers()` (`Assets/Scripts/Managers/GameManager.cs`), `Player.Initialize(int playerNumber)` / `Player.FreezeControls` (`Assets/Scripts/Player/Player.cs`), `DebugRegistrar.Track` (`Assets/Scripts/Managers/DebugRegistrar.cs`).
- Produces: `IPlayerJoinStrategy` interface (`Initialize(PlayerManager)`, `OnPlayerJoined(PlayerInput)`, `OnPlayerLeft(PlayerInput)`, `OnDeviceLost(Player)`, `OnDeviceRegained(Player)`, `OnGUI()`) — consumed by Task 2's `Player.cs` changes via `PlayerManager.OnDeviceLost(Player)`/`OnDeviceRegained(Player)`, and by Task 3's `FishSelectMenuPage` via `PlayerManager.OnSlotJoined`/`OnSlotLeft` events (`event Action<int>`).

- [ ] **Step 1: Create the `IPlayerJoinStrategy` interface**

`Assets/Scripts/Managers/IPlayerJoinStrategy.cs`:
```csharp
using UnityEngine.InputSystem;

public interface IPlayerJoinStrategy {
    void Initialize(PlayerManager owner);
    void OnPlayerJoined(PlayerInput input);
    void OnPlayerLeft(PlayerInput input);
    void OnDeviceLost(Player player);
    void OnDeviceRegained(Player player);
    void OnGUI();
}
```

- [ ] **Step 2: Create `ArcadeJoinStrategy` as a verbatim extraction of today's ritual**

`Assets/Scripts/Managers/ArcadeJoinStrategy.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArcadeJoinStrategy : IPlayerJoinStrategy {
    private PlayerManager owner;
    private bool showDebugMessage;
    private int playerThreshold;

    public void Initialize(PlayerManager owner) {
        this.owner = owner;
        showDebugMessage = true;

        if (GameManager.Instance.debug) {
            playerThreshold = GameManager.Instance.debugPlayerCount;
            DebugRegistrar.Track("PlayerManager.RequiredPlayerCount", () =>
                $"OVERRIDDEN -> {GameManager.Instance.debugPlayerCount} " +
                $"(prod would require {owner.GetComponent<PlayerInputManager>().maxPlayerCount})");
        } else {
            playerThreshold = owner.GetComponent<PlayerInputManager>().maxPlayerCount;
        }

        owner.PlayerInputs = new List<PlayerInput>();
        owner.Players = new List<Player>();
        owner.PlayerFish = new List<GunfishData>();
    }

    public void OnPlayerJoined(PlayerInput input) {
        owner.PlayerInputs.Add(input);

        if (owner.PlayerInputs.Count == playerThreshold) {
            showDebugMessage = false;
            InitializePlayers();
            GameManager.Instance.InitializePostRitualManagers();
        }
    }

    public void OnPlayerLeft(PlayerInput input) {
        Debug.Log($"Player {input.name} has been disconnected.");
        owner.PlayerInputs.Remove(input);
    }

    public void OnDeviceLost(Player player) {
        player.FreezeControls = true;
    }

    public void OnDeviceRegained(Player player) {
        player.FreezeControls = false;
    }

    private void InitializePlayers() {
        owner.SetInputMode(PlayerManager.InputMode.UI);
        for (int playerIndex = 0; playerIndex < owner.PlayerInputs.Count; playerIndex++) {
            var playerInput = owner.PlayerInputs[playerIndex];
            var player = playerInput.GetComponent<Player>();
            player.Initialize(playerIndex);
            owner.Players.Add(player);
            owner.PlayerFish.Add(null);
        }
    }

    public void OnGUI() {
        if (!showDebugMessage) return;
        GUIStyle style = new GUIStyle(GUI.skin.textArea) {
            fontSize = 30,
            wordWrap = true
        };

        GUILayout.TextField(
            "Welcome to Gunfish! If you're seeing this message it means this game is still initializing. Please press the GUN button for each controller in the following order: RED, GREEN, BLUE, YELLOW.",
            style
        );
    }
}
```

This is identical in behavior to today's `PlayerManager` (same ritual text, same threshold computation, same `DebugRegistrar` entry, same `InitializePlayers` body), plus two net-new methods (`OnDeviceLost`/`OnDeviceRegained`) that didn't exist as callable behavior before — the spec calls for this explicitly, and `Player.FreezeControls` is already checked in `OnMove`/`OnFire`/`OnRespawn`, so this is a pure addition, not a behavior change to existing paths.

- [ ] **Step 3: Create `OnlineJoinStrategy` with fixed 4-slot semantics**

`Assets/Scripts/Managers/OnlineJoinStrategy.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class OnlineJoinStrategy : IPlayerJoinStrategy {
    private PlayerManager owner;

    public void Initialize(PlayerManager owner) {
        this.owner = owner;

        var slotCount = owner.GetComponent<PlayerInputManager>().maxPlayerCount;
        owner.PlayerInputs = new List<PlayerInput>(new PlayerInput[slotCount]);
        owner.Players = new List<Player>(new Player[slotCount]);
        owner.PlayerFish = new List<GunfishData>(new GunfishData[slotCount]);

        GameManager.Instance.InitializePostRitualManagers();
    }

    public void OnPlayerJoined(PlayerInput input) {
        var slot = owner.PlayerInputs.IndexOf(null);
        if (slot == -1) return;

        var player = input.GetComponent<Player>();
        player.Initialize(slot);

        owner.PlayerInputs[slot] = input;
        owner.Players[slot] = player;
        owner.PlayerFish[slot] = null;

        owner.NotifySlotJoined(slot);
    }

    public void OnPlayerLeft(PlayerInput input) {
        var slot = owner.PlayerInputs.IndexOf(input);
        if (slot == -1) return;

        owner.PlayerInputs[slot] = null;
        owner.Players[slot] = null;
        owner.PlayerFish[slot] = null;

        owner.NotifySlotLeft(slot);
    }

    public void OnDeviceLost(Player player) {
        player.FreezeControls = true;
    }

    public void OnDeviceRegained(Player player) {
        player.FreezeControls = false;
    }

    public void OnGUI() { }
}
```

`new List<T>(new T[slotCount])` gives a `List<T>` of length `slotCount` pre-filled with `null` (valid for the reference types `PlayerInput`/`Player`/`GunfishData` used here). `IndexOf(null)` finds the first empty slot; Unity's own `PlayerInputManager.maxPlayerCount` cap means `OnPlayerJoined` is never even invoked once all 4 slots are occupied (occupied includes mid-match-frozen players, since their `PlayerInput` is never destroyed), so the `slot == -1` guard is unreachable in practice but cheap insurance against a `-1` index corrupting the lists.

- [ ] **Step 4: Rewrite `PlayerManager.cs` to delegate to the active strategy**

Replace the full contents of `Assets/Scripts/Managers/PlayerManager.cs` with:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : PersistentSingleton<PlayerManager> {
    public List<Color> playerColors;

    public List<Player> Players;
    public List<GunfishData> PlayerFish;
    public List<PlayerInput> PlayerInputs;

    public event Action<int> OnSlotJoined;
    public event Action<int> OnSlotLeft;

    private IPlayerJoinStrategy strategy;

    public override void Initialize() {
        base.Initialize();

#if GUNFISH_ARCADE
        strategy = new ArcadeJoinStrategy();
#else
        strategy = new OnlineJoinStrategy();
#endif
        strategy.Initialize(this);

        SetInputMode(InputMode.UI);
    }

    public void OnPlayerJoined(PlayerInput input) => strategy.OnPlayerJoined(input);

    public void OnPlayerLeft(PlayerInput input) => strategy.OnPlayerLeft(input);

    public void OnDeviceLost(Player player) => strategy.OnDeviceLost(player);

    public void OnDeviceRegained(Player player) => strategy.OnDeviceRegained(player);

    internal void NotifySlotJoined(int slot) => OnSlotJoined?.Invoke(slot);

    internal void NotifySlotLeft(int slot) => OnSlotLeft?.Invoke(slot);

    public void SetPlayerFish(int playerIndex, GunfishData data) {
        if (playerIndex < 0 || playerIndex >= PlayerFish.Count) {
            return;
        }
        PlayerFish[playerIndex] = data;
        Players[playerIndex].gunfishData = data;
        Players[playerIndex].Active = data != null;
    }

    public void SetInputMode(InputMode inputMode) {
        foreach (var playerInput in PlayerInputs) {
            playerInput?.SwitchCurrentActionMap(inputMode.ToString());
        }
    }

    public void OnGUI() => strategy.OnGUI();

    // Must be either Player or UI
    public enum InputMode {
        Player,
        UI,
        EndLevel,
        Null,
    }
}
```

Note `OnPlayerJoined(PlayerInput)`/`OnPlayerLeft(PlayerInput)` keep their exact existing names and signatures — these are invoked by Unity via the persistent `UnityEvent` calls wired in `Assets/Resources/Prefabs/GameManager.prefab`'s `PlayerInputManager` component (`m_PlayerJoinedEvent`/`m_PlayerLeftEvent`), so no prefab changes are needed.

- [ ] **Step 5: Compile-check the default (online) configuration**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)` (warnings unrelated to these files are pre-existing and fine).

- [ ] **Step 6: Compile-check the arcade (`GUNFISH_ARCADE`) configuration**

Run:
```bash
existing=$(grep -o '<DefineConstants>[^<]*</DefineConstants>' Assembly-CSharp.csproj | head -1 | sed -e 's/<[^>]*>//g')
dotnet build Assembly-CSharp.csproj -p:DefineConstants="${existing};GUNFISH_ARCADE"
```
Expected: `0 Error(s)`. This proves the `#if GUNFISH_ARCADE` branch (which references `ArcadeJoinStrategy`) compiles even though the project isn't built with that define by default.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/Managers/IPlayerJoinStrategy.cs Assets/Scripts/Managers/ArcadeJoinStrategy.cs Assets/Scripts/Managers/OnlineJoinStrategy.cs Assets/Scripts/Managers/PlayerManager.cs
git commit -m "refactor: extract PlayerManager join logic into IPlayerJoinStrategy"
```

---

### Task 2: `Player.cs` — forward device-lost/regained to `PlayerManager`

**Files:**
- Modify: `Assets/Scripts/Player/Player.cs:62-72`

**Interfaces:**
- Consumes: `PlayerManager.Instance.OnDeviceLost(Player)` / `.OnDeviceRegained(Player)` (produced by Task 1).
- Produces: nothing new consumed by later tasks — this closes the loop Unity's own `PlayerInput` SendMessage convention started (`OnDeviceLost(PlayerInput)`/`OnDeviceRegained(PlayerInput)` are invoked automatically by Unity, same mechanism as `OnMove`/`OnFire`).

- [ ] **Step 1: Wire the two stubs to forward to `PlayerManager`**

In `Assets/Scripts/Player/Player.cs`, replace:
```csharp
    public void OnDeviceLost(PlayerInput input) {

    }

    public void OnDeviceRegained(PlayerInput input) {

    }
```
with:
```csharp
    public void OnDeviceLost(PlayerInput input) {
        PlayerManager.Instance.OnDeviceLost(this);
    }

    public void OnDeviceRegained(PlayerInput input) {
        PlayerManager.Instance.OnDeviceRegained(this);
    }
```
(`OnControlsChanged` stays an empty stub — neither strategy uses it, per spec.)

- [ ] **Step 2: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Player/Player.cs
git commit -m "feat: forward Player device-lost/regained to PlayerManager's join strategy"
```

---

### Task 3: `FishSelectMenuPage` — dynamic per-slot wiring for online join/leave

**Files:**
- Modify: `Assets/Scripts/UI/FishSelectMenuPage.cs`

**Interfaces:**
- Consumes: `PlayerManager.Instance.OnSlotJoined` / `.OnSlotLeft` (`event Action<int>`, produced by Task 1), `PlayerManager.Instance.PlayerInputs` (now fixed-size with `null` holes on the online build, unchanged compacting list on arcade).
- Produces: nothing consumed by later tasks.

Today, `OnPageStart` wires each panel's input callbacks exactly once, looping `0..PlayerManager.Instance.PlayerInputs.Count`. On the online build that list is always length 4 (with `null` holes), so this loop must skip empty slots at page-start time, and must react when a slot is filled or emptied *while the fish-select page is already open* — otherwise a controller that joins mid-lobby is never wired to its panel, silently breaking the whole point of this feature.

- [ ] **Step 1: Add a field to track which `PlayerInput` each slot was wired against**

In `Assets/Scripts/UI/FishSelectMenuPage.cs`, replace:
```csharp
    private List<PlayerAction> playerActions;
```
with:
```csharp
    private List<PlayerAction> playerActions;
    private List<PlayerInput> wiredPlayerInputs;
```

- [ ] **Step 2: Replace the per-slot setup loop in `OnPageStart` with slot-count pre-sizing + a call to a new `WireSlot` helper, and subscribe to slot-change events**

Replace:
```csharp
        playerActions = new List<PlayerAction>();
        gunfishIndices = new List<int>();
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            int playerIndex = i;
            var playerInput = PlayerManager.Instance.PlayerInputs[playerIndex];
            var fishSelectPanel = fishSelectPanels[playerIndex];
            gunfishIndices.Add(0);

            PlayerAction playerAction = new PlayerAction(
                (InputAction.CallbackContext context) => OnNavigate(context, playerIndex),
                (InputAction.CallbackContext context) => OnSubmit(context, playerIndex),
                (InputAction.CallbackContext context) => OnCancel(context, playerIndex)
            );
            playerActions.Add(playerAction);
            playerInput.currentActionMap.FindAction("Navigate").performed += playerAction.navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed += playerAction.submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed += playerAction.cancelPerformed;

            var color = PlayerManager.Instance.playerColors[playerIndex];

            fishSelectPanel.Initialize();
            fishSelectPanel.SetColor(color);
            fishSelectPanel.SetState(FishSelectPanel.State.Inactive);
        }

        Fade();
```
with:
```csharp
        playerActions = new List<PlayerAction>(new PlayerAction[fishSelectPanels.Count]);
        gunfishIndices = new List<int>(new int[fishSelectPanels.Count]);
        wiredPlayerInputs = new List<PlayerInput>(new PlayerInput[fishSelectPanels.Count]);

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count && i < fishSelectPanels.Count; i++) {
            WireSlot(i);
        }

        PlayerManager.Instance.OnSlotJoined += WireSlot;
        PlayerManager.Instance.OnSlotLeft += UnwireSlot;

        Fade();
```

- [ ] **Step 3: Replace `OnPageStop`'s inline unwire loop with unsubscription + a call to a new `UnwireSlot` helper**

Replace:
```csharp
    public override void OnPageStop(MenuPageContext context) {
        ArduinoManager.Instance.playAttractors = false;
        DebugRegistrar.Untrack("FishSelectMenuPage.ReadyPlayerGate");

        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.Instance.PlayerInputs[i];
            playerInput.currentActionMap.FindAction("Navigate").performed -= playerActions[i].navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed -= playerActions[i].submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed -= playerActions[i].cancelPerformed;
        }
        base.OnPageStop(context);
    }
```
with:
```csharp
    public override void OnPageStop(MenuPageContext context) {
        ArduinoManager.Instance.playAttractors = false;
        DebugRegistrar.Untrack("FishSelectMenuPage.ReadyPlayerGate");

        PlayerManager.Instance.OnSlotJoined -= WireSlot;
        PlayerManager.Instance.OnSlotLeft -= UnwireSlot;

        for (int i = 0; i < fishSelectPanels.Count; i++) {
            UnwireSlot(i);
        }
        base.OnPageStop(context);
    }

    private void WireSlot(int playerIndex) {
        var playerInput = PlayerManager.Instance.PlayerInputs[playerIndex];
        if (playerInput == null) return;

        wiredPlayerInputs[playerIndex] = playerInput;
        gunfishIndices[playerIndex] = 0;

        PlayerAction playerAction = new PlayerAction(
            (InputAction.CallbackContext context) => OnNavigate(context, playerIndex),
            (InputAction.CallbackContext context) => OnSubmit(context, playerIndex),
            (InputAction.CallbackContext context) => OnCancel(context, playerIndex)
        );
        playerActions[playerIndex] = playerAction;
        playerInput.currentActionMap.FindAction("Navigate").performed += playerAction.navigatePerformed;
        playerInput.currentActionMap.FindAction("Submit").performed += playerAction.submitPerformed;
        playerInput.currentActionMap.FindAction("Cancel").performed += playerAction.cancelPerformed;

        var color = PlayerManager.Instance.playerColors[playerIndex];
        var fishSelectPanel = fishSelectPanels[playerIndex];
        fishSelectPanel.Initialize();
        fishSelectPanel.SetColor(color);
        fishSelectPanel.SetState(FishSelectPanel.State.Inactive);
    }

    private void UnwireSlot(int playerIndex) {
        var playerInput = wiredPlayerInputs[playerIndex];
        if (playerInput != null) {
            var playerAction = playerActions[playerIndex];
            playerInput.currentActionMap.FindAction("Navigate").performed -= playerAction.navigatePerformed;
            playerInput.currentActionMap.FindAction("Submit").performed -= playerAction.submitPerformed;
            playerInput.currentActionMap.FindAction("Cancel").performed -= playerAction.cancelPerformed;
        }
        wiredPlayerInputs[playerIndex] = null;
        playerActions[playerIndex] = default;

        fishSelectPanels[playerIndex].SetState(FishSelectPanel.State.Inactive);
    }
```

`UnwireSlot` reads the `PlayerInput` reference it cached in `wiredPlayerInputs` (not `PlayerManager.Instance.PlayerInputs[playerIndex]`, which `OnlineJoinStrategy.OnPlayerLeft` has already nulled out by the time the `OnSlotLeft` event fires) — this is what lets it unsubscribe safely regardless of event ordering. On arcade, `OnSlotJoined`/`OnSlotLeft` never fire (only `OnlineJoinStrategy` calls `NotifySlotJoined`/`NotifySlotLeft`), so this is a purely additive, zero-risk change there — `OnPageStart`'s initial loop and `OnPageStop`'s final loop reproduce today's exact wire/unwire behavior for arcade's compacting list.

- [ ] **Step 4: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/UI/FishSelectMenuPage.cs
git commit -m "feat: wire fish-select panels dynamically as controllers join/leave online"
```

---

### Task 4: `QuickLaunchManager` — null-safety for the fixed-slot online model

**Files:**
- Modify: `Assets/Scripts/Managers/QuickLaunchManager.cs:112-114` (`IsValidControllerIndex`)
- Modify: `Assets/Scripts/Managers/QuickLaunchManager.cs:236-260` (`Submit`, two separate loops)
- Modify: `Assets/Scripts/Managers/QuickLaunchManager.cs:330-347` (`RenderControllersStep`)

**Interfaces:**
- Consumes: `PlayerManager.Instance.PlayerInputs` (now `null`-holed on the online build).
- Produces: nothing consumed by later tasks.

This is the one file in the codebase that indexes `PlayerManager.Instance.PlayerInputs` raw, without a null guard, and calls `PlayerManager.Instance.SetPlayerFish(i, ...)` for every index regardless of occupancy — both are NPE risks once `PlayerInputs` can contain `null` holes on the online build. This tool is `GameManager.Instance.debug`-gated (F4 dev overlay), never shown to players, but it must not crash when a developer opens it with fewer than 4 controllers connected.

- [ ] **Step 1: Guard `IsValidControllerIndex` against empty slots**

Replace:
```csharp
    private bool IsValidControllerIndex(int index) {
        return PlayerManager.InstanceExists && index >= 0 && index < PlayerManager.Instance.PlayerInputs.Count;
    }
```
with:
```csharp
    private bool IsValidControllerIndex(int index) {
        return PlayerManager.InstanceExists && index >= 0 && index < PlayerManager.Instance.PlayerInputs.Count
            && PlayerManager.Instance.PlayerInputs[index] != null;
    }
```

- [ ] **Step 2: Skip empty slots in `Submit()`'s pre-teardown despawn loop and its fish-assignment loop**

Replace:
```csharp
            foreach (var player in PlayerManager.Instance.Players) {
                var gunfish = player.Gunfish;
                if (gunfish != null && gunfish.segments != null && gunfish.segments.Count > 0) {
                    player.DespawnGunfish();
                }
            }
```
with:
```csharp
            foreach (var player in PlayerManager.Instance.Players) {
                if (player == null) continue;
                var gunfish = player.Gunfish;
                if (gunfish != null && gunfish.segments != null && gunfish.segments.Count > 0) {
                    player.DespawnGunfish();
                }
            }
```

Then, a few lines later, replace:
```csharp
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            var fish = activeControllers.Contains(i) && fishByController.TryGetValue(i, out var chosen) ? chosen : null;
            PlayerManager.Instance.SetPlayerFish(i, fish);
        }
```
with:
```csharp
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            if (PlayerManager.Instance.PlayerInputs[i] == null) continue;
            var fish = activeControllers.Contains(i) && fishByController.TryGetValue(i, out var chosen) ? chosen : null;
            PlayerManager.Instance.SetPlayerFish(i, fish);
        }
```
(Without these, `player.Gunfish` and `PlayerManager.SetPlayerFish`'s `Players[playerIndex].gunfishData = data` would both NPE on any empty slot, since neither checks occupancy today.)

- [ ] **Step 3: Guard the device-name lookup in `RenderControllersStep`**

Replace:
```csharp
    private void RenderControllersStep(StringBuilder sb) {
        sb.AppendLine("Step 1/4 - Controllers  (Up/Down move, Enter toggle, Right advance)");
        if (!PlayerManager.InstanceExists || PlayerManager.Instance.PlayerInputs.Count == 0) {
            sb.AppendLine("(no players connected)");
            return;
        }
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            string cursor = i == cursorIndex ? ">" : " ";
            string check = activeControllers.Contains(i) ? "[x]" : "[ ]";
            string device = PlayerManager.Instance.PlayerInputs[i].devices.Count > 0
                ? PlayerManager.Instance.PlayerInputs[i].devices[0].displayName
                : "Unknown Device";
            sb.AppendLine($"{cursor} {check} Player {i + 1} ({device})");
        }
        if (activeControllers.Count == 0) {
            sb.AppendLine("Select at least 1 controller to advance.");
        }
    }
```
with:
```csharp
    private void RenderControllersStep(StringBuilder sb) {
        sb.AppendLine("Step 1/4 - Controllers  (Up/Down move, Enter toggle, Right advance)");
        if (!PlayerManager.InstanceExists || PlayerManager.Instance.PlayerInputs.Count == 0) {
            sb.AppendLine("(no players connected)");
            return;
        }
        for (int i = 0; i < PlayerManager.Instance.PlayerInputs.Count; i++) {
            var playerInput = PlayerManager.Instance.PlayerInputs[i];
            if (playerInput == null) {
                sb.AppendLine($"    Player {i + 1} (empty)");
                continue;
            }
            string cursor = i == cursorIndex ? ">" : " ";
            string check = activeControllers.Contains(i) ? "[x]" : "[ ]";
            string device = playerInput.devices.Count > 0
                ? playerInput.devices[0].displayName
                : "Unknown Device";
            sb.AppendLine($"{cursor} {check} Player {i + 1} ({device})");
        }
        if (activeControllers.Count == 0) {
            sb.AppendLine("Select at least 1 controller to advance.");
        }
    }
```

- [ ] **Step 4: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add Assets/Scripts/Managers/QuickLaunchManager.cs
git commit -m "fix: null-guard QuickLaunchManager against empty controller slots"
```

---

### Task 5: `GameModeManager` — null-guard the raw `Players` list at match start/teardown

**Files:**
- Modify: `Assets/Scripts/Managers/GameModeManager.cs:18-21` (`InitializeGameMode`)
- Modify: `Assets/Scripts/Managers/GameModeManager.cs:46-58` (`TeardownGameMode`)

**Interfaces:**
- Consumes: `PlayerManager.Instance.Players` (now `null`-holed on the online build), `PlayerManager.Instance.SetPlayerFish(int, GunfishData)`.
- Produces: `GameModeManager.activePlayers` / `GameParameters.activePlayers` — already-filtered lists that every match manager (`MatchManager`, `RaceMatchManager`, `DeathMatchManager`, `BassballMatchManager`) and `PelicanSpawner` consume; none of them touch the raw `Players` list, so no changes are needed in those files.

This is the highest-severity gap found while planning: `GameManager.InitializeGame()` passes `PlayerManager.Instance.Players` directly into `InitializeGameMode`, which runs `players.Where(player => player.Active)` — this NPEs on the very first empty slot for **any** online match with fewer than 4 confirmed players, which will be the common case once free join/leave ships. `TeardownGameMode` has the same unconditional-indexing bug already fixed in `QuickLaunchManager` (Task 4).

- [ ] **Step 1: Null-guard the `Where` filter in `InitializeGameMode`**

Replace:
```csharp
        activePlayers = players.Where(player => player.Active).ToList();
```
with:
```csharp
        activePlayers = players.Where(player => player != null && player.Active).ToList();
```

- [ ] **Step 2: Skip empty slots in `TeardownGameMode`'s fish-clearing loop**

Replace:
```csharp
        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++) {
            PlayerManager.Instance.SetPlayerFish(i, null);
        }
```
with:
```csharp
        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++) {
            if (PlayerManager.Instance.Players[i] == null) continue;
            PlayerManager.Instance.SetPlayerFish(i, null);
        }
```

- [ ] **Step 3: Compile-check**

Run: `dotnet build Assembly-CSharp.csproj`
Expected: `0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Managers/GameModeManager.cs
git commit -m "fix: null-guard GameModeManager against empty controller slots"
```

---

### Task 6: `GUNFISH_ARCADE` build config + full manual regression pass

**Files:**
- No source changes — Unity Editor UI step (Player Settings) + manual verification only.

**Interfaces:**
- Consumes: everything from Tasks 1-5.
- Produces: nothing (final task).

`GUNFISH_ARCADE` does not exist anywhere in the project yet (confirmed by repo-wide grep — only `ProjectSettings/ProjectSettings.asset`'s per-platform `scriptingDefineSymbols` list exists, currently containing only `UNITY_POST_PROCESSING_STACK_V2`). Per this project's conventions, `ProjectSettings/*.asset` must be edited through its Editor panel, not by hand.

- [ ] **Step 1: Add the `GUNFISH_ARCADE` scripting define for the arcade build**

In the Unity Editor: **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**, for whichever platform tab represents the arcade cabinet build target (Standalone), add `GUNFISH_ARCADE` to the list and click **Apply**. Leave it absent for the platform tab(s) used for online/default builds. (There is currently no dedicated Unity Build Profile split between "arcade" and "online" — this define is a manual per-target toggle until/unless a dedicated Build Profile is created; that's out of scope here.)

- [ ] **Step 2: Manual verification — online build (default, no `GUNFISH_ARCADE`), in-Editor**

With the `GUNFISH_ARCADE` define **not** set for the Editor's active platform:
1. Press Play. Confirm the game boots straight to the splash/menu flow with no ritual prompt (since `OnlineJoinStrategy.OnGUI()` is a no-op) and no wait for a fixed controller count.
2. Join 1-4 players (keyboard + any connected gamepads, or the Input Debugger's simulated devices) in varying order at fish-select. Confirm each shows up in its own slot/color/panel as it joins, independent of arrival order.
3. Have a joined-but-not-yet-confirmed player back out (Cancel to `Inactive`, then leave the lobby if your input setup allows it / or just watch the slot on unpair). Confirm their slot shows as empty and does **not** shift any other already-confirmed player's panel/color.
4. Confirm match start still requires satisfying `GameMode.requiredPlayerCount` (or the debug override) exactly as before, **with fewer than 4 players connected** (e.g. 2 confirmed, 2 slots left empty) — this exercises the `GameModeManager` null-guards from Task 5; confirm the match starts without an exception and ends/tears down cleanly back to the lobby.
5. Mid-match: disable/unplug one device. Confirm that player's fish freezes in place (stops moving/firing) but is not despawned and still collides/scores.
6. Reconnect the **same** device mid-match. Confirm control resumes immediately with no re-join step.
7. With another player mid-match `Frozen`, connect a **new**, previously-unseen device. Confirm it does **not** take over the frozen player's slot (it either fills an empty slot if one exists, or is ignored if the roster shows 4 occupied slots — recall occupied includes frozen).

- [ ] **Step 3: Manual verification — arcade build (`GUNFISH_ARCADE` set), in-Editor**

With the `GUNFISH_ARCADE` define set for the Editor's active platform (temporarily, for this check):
1. Press Play. Confirm the exact same ritual `OnGUI` prompt text appears ("Welcome to Gunfish!... RED, GREEN, BLUE, YELLOW") and blocks everything else until the threshold (4 in prod, or `debugPlayerCount` with `GameManager.Instance.debug` on) is met.
2. Join controllers in RED/GREEN/BLUE/YELLOW order. Confirm `InitializePlayers()` fires exactly once at threshold and `GameManager.Instance.InitializePostRitualManagers()` runs immediately after (menus/managers initialize).
3. Confirm fish-select behaves exactly as before with the fixed roster.
4. Remove the temporary `GUNFISH_ARCADE` define once this check is done, unless you intend to keep building for the cabinet next.

- [ ] **Step 4: Commit** (only if Step 1 required a checked-in Player Settings change for a real arcade build profile — otherwise there is nothing to commit for this task)

If `ProjectSettings/ProjectSettings.asset` changed as a result of Step 1 and this repository ships that define as part of its committed arcade configuration:
```bash
git add ProjectSettings/ProjectSettings.asset
git commit -m "chore: add GUNFISH_ARCADE scripting define for the arcade build target"
```
