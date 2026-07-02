# Controller Join Strategy — Design

## Background

Today, `PlayerManager` gates the entire game behind "the ritual": on boot, players
press their GUN button in left-to-right order until a fixed count (4 in prod, 1-4 in
debug) is reached, then `InitializePlayers()` runs once and the roster is locked for
the session. This exists because the arcade cabinet's 4 controllers previously
couldn't be deterministically mapped to a stable left-to-right order — Unity's
runtime `deviceId` changes across power cycles / USB re-enumeration.

Two problems with keeping this as the only path:
1. It's a clunky UX even on the cabinet.
2. It fundamentally breaks for the upcoming online release, where controllers should
   be able to join/leave freely (Smash-Bros-style, at the existing fish-select lobby)
   and reconnect after a mid-match disconnect.

Max player count stays fixed at **4** for both builds (current UI has hard technical
limits around this; no near-term desire to change it).

## Goals

- Online build: controllers join/leave freely at the fish-select lobby (which already
  has a per-slot ready-up flow in `FishSelectMenuPage` / `FishSelectPanel`). A
  mid-match physical disconnect freezes that player's fish in place; reconnecting the
  *same physical device* resumes control automatically. A different device cannot
  hijack another player's slot.
- Arcade build: preserve **exactly** today's ritual behavior in this pass — no
  regressions. Auto-detection of physical port order is out of scope for this spec
  (see TODO below) but the architecture must not block adding it later.

## Non-goals

- Implementing arcade physical-port auto-detection (tracked as a TODO/spike below).
- Supporting more than 4 players.
- Allowing a different physical controller to resume a disconnected player's slot
  (only the original device can reconnect into its slot).
- Mid-match explicit "leave" beyond what `PlayerInputManager` already does (leaving is
  a lobby-time action; a physical disconnect mid-match is a `Frozen` state, not a
  leave).

## Architecture

`PlayerManager` stops implementing join/leave/device-lost logic directly. It owns an
`IPlayerJoinStrategy`, chosen once during `Initialize()`:

```csharp
public interface IPlayerJoinStrategy {
    void OnPlayerJoined(PlayerInput input);
    void OnPlayerLeft(PlayerInput input);
    void OnDeviceLost(Player player);
    void OnDeviceRegained(Player player);
}
```

- `#if GUNFISH_ARCADE` → `ArcadeJoinStrategy`
- `#else` → `OnlineJoinStrategy`

`GUNFISH_ARCADE` is a compile-time scripting define symbol set per build target in
Player Settings — consistent with how `GameManager.debug` /
`DevConfigOverride` already branch behavior, but this one is a build-time constant,
not a runtime toggle, since the two builds ship to fundamentally different hardware.

`PlayerManager` keeps its current public surface unchanged (`Players`,
`PlayerInputs`, `SetPlayerFish`, `SetInputMode`, `playerColors`, etc.) so
`GameManager`, `FishSelectMenuPage`, and the match managers don't need to know which
strategy is active.

This single-class-with-pluggable-strategy shape (rather than two `PlayerManager`
subclasses swapped per build) was chosen deliberately: it matches the direction of
the recent "de-prefab all submanagers to avoid inconsistencies" change — introducing
two build-variant manager subclasses would reintroduce the same class of
variant-drift risk that commit removed.

`Player.OnDeviceLost` / `OnDeviceRegained` / `OnControlsChanged` — currently empty
method stubs on `Player : IDeviceController` — are already invoked automatically by
Unity's `PlayerInput` component via its SendMessage notification convention (same
mechanism as `OnMove`/`OnFire`). They become the trigger points: implement them to
forward to `PlayerManager.Instance`'s active strategy.

## Components

### `ArcadeJoinStrategy`

Moved as-is from today's `PlayerManager`: `OnGUI` ritual prompt, buffer joins into
`PlayerInputs`, wait for `playerThreshold`, call `InitializePlayers()` once met.
`OnDeviceLost`/`OnDeviceRegained` set `player.FreezeControls`. No slot-reassignment
logic — behavior is byte-for-byte identical to today.

### `OnlineJoinStrategy`

- `OnPlayerJoined(PlayerInput)`: assign the first free slot (0-3) immediately and
  call `Player.Initialize(slot)` right away — no waiting for a fixed count. This is
  what lets `FishSelectMenuPage` (already iterating
  `PlayerManager.Instance.PlayerInputs`) show players as they join.
- `OnPlayerLeft(PlayerInput)`: explicit unpair (fires from `PlayerInputManager` on
  `PlayerInput` destruction — a lobby-time action, not a physical unplug). Frees the
  slot.
- `OnDeviceLost(Player)`: mid-match physical disconnect. Set
  `player.FreezeControls = true`. Player stays in `Players` and keeps
  scoring/colliding. The slot is **not** freed — Unity's `PlayerInput` pairing keeps
  the slot bound to that exact device instance while lost, which is what makes
  "reconnect as the same player" work with no extra bookkeeping.
- `OnDeviceRegained(Player)`: set `player.FreezeControls = false`.
- A different, previously-unseen device pressing a button while another player's slot
  is `Frozen` does not take over that slot — it can only claim an `Empty` slot, or is
  ignored if the roster is full.

### `Player.cs` changes

Implement the three currently-empty `IDeviceController` methods to forward to the
active strategy via `PlayerManager.Instance`. `FreezeControls` already exists and is
already checked in `OnMove`/`OnFire` — no new flag needed.

## Data Flow / State (Online)

Per-slot state, extending the existing `FishSelectPanel.State` one level up:

```
Empty --(OnPlayerJoined)--> Joined/Selecting --(confirm in fish-select)--> Confirmed
Confirmed --(match starts)--> InMatch
InMatch --(OnDeviceLost)--> InMatch-Frozen --(OnDeviceRegained, same device)--> InMatch
InMatch-Frozen --(match ends, still lost)--> Empty for next lobby
Joined/Selecting --(OnPlayerLeft / cancel)--> Empty
```

`FishSelectMenuPage.AllPlayersReady()` keeps working unchanged — it already just
counts `Confirmed` panels against `allowedPlayerCounts`, independent of arrival
order/timing.

## Error Handling & Fallback

- **Online**: no fixed player-count gate — matches already support 1-4 via
  `GameMode.requiredPlayerCount` / `allowedPlayerCounts`.
- **Arcade**: unchanged in this pass. Zero new risk to the cabinet.

## TODO / Future Spike: Arcade Physical-Port Auto-Detection

Not implemented in this pass. Captured here so the context isn't lost:

- **Hardware topology**: all 4 arcade controllers are wired into a single 4-port USB
  hub, which feeds **one** upstream USB port on the NUC. This means any physical-port
  identifier must be the hub's *downstream* port index for each controller, not a
  NUC-side port — there is only one NUC-side port to distinguish by.
- Unity's Input System does not expose USB topology / downstream hub port directly
  (`InputDevice.description` does not reliably include it, and generic arcade
  encoders typically share the same VID/PID with no per-unit serial, so serial-based
  matching likely won't work either).
- Likely requires native interop (Windows `SetupAPI` / device instance path parsing)
  to correlate a Unity `InputDevice` to its hub-relative port number. This is
  unverified and needs a spike **on the actual cabinet hardware** before any
  implementation commitment.
- When/if a resolver is built, it slots into `ArcadeJoinStrategy` as an optional first
  attempt: if it produces a unique, confident 4-way mapping, skip the ritual; if not
  (or not implemented), silently fall back to the ritual exactly as it behaves today.
  This fallback behavior was a deliberate decision — the cabinet is a live revenue
  machine and must never get stuck waiting on auto-detect.
- Do not add a manual override toggle unless a real on-site troubleshooting need
  arises; not planned for now.

## Testing

No automated test suite in this project — manual verification in-editor:

- Join 1-4 players in varying order at fish-select; confirm slot/color assignment and
  match start.
- Mid-match: disable a device; confirm the fish freezes in place (not despawned),
  still scores/collides.
- Reconnect the same device mid-match; confirm control resumes with no re-join step.
- Confirm a different new controller while another player is `Frozen` only fills an
  `Empty` slot, never the frozen one.
- Arcade build (`GUNFISH_ARCADE` define): regression-check the ritual behaves
  identically to pre-change behavior.
