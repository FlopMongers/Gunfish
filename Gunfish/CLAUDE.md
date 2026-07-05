# CLAUDE.md — Gunfish

## Project Overview

Gunfish is a 4-player local multiplayer 2D arcade game built in Unity 6000.5.0f1. Players control gun-equipped fish that battle across physics-based levels in three game modes: **DeathMatch**, **Race**, and **Bassball**.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Engine | Unity 6000.5.0f1 (see `ProjectSettings/ProjectVersion.txt`) |
| Language | C# |
| IDE | VS Code with C# DevKit (solution: `Gunfish.sln`) |
| Input | Unity New Input System 1.7.0 — `PlayerInput` + action assets |
| Animation | DOTween (Demigiant) — prefer over coroutine-based lerps |
| Camera | Cinemachine 2.10.1 |
| Text | TextMeshPro 3.0.8 — all in-game text |
| Post-FX | Post Processing 3.4.0 |
| Persistence | SQLite-net (via `gilzoide/unity-sqlite-net`) |

**NOT used:** `Input.GetKey` / legacy Input Manager — always use New Input System action assets. Never use Unity's built-in `Text` component — always TextMeshPro.

---

## Development Environment

- **Open project:** Drag the `Gunfish/` folder onto Unity Hub, or File → Open Project
- **Run:** Press ▶ Play in the Unity Editor — no CLI entry point exists
- **Build:** File → Build Settings → select platform → Build
- **Testing:** No automated test suite — test by playing in the Editor

---

## Architecture

### Core Manager Pattern

Global systems are singletons that persist across scenes, inheriting from `Utils/PersistentSingleton<T>` (or `Singleton<T>` for scene-scoped). Key managers:

| Manager | Responsibility |
|---|---|
| `GameManager` | Top-level orchestrator; owns game state and initiates match flow |
| `PlayerManager` | Tracks connected players and their team/color assignments |
| `GameModeManager` | Holds the active `GameModeType` enum; delegates to the active match manager |
| `LevelManager` | Handles async scene loading and level selection |
| `MusicManager` | Controls BGM/SFX |
| `StatsManager` | Local stats/persistence via SQLite |
| `GameUIManager` | HUD, menus, and transitions |

### Match Flow

Each game mode has a dedicated `MatchManager` subclass (`Managers/MatchManagers/`):

```
MatchManager (abstract)
  └── GunfishGameMatchManager
        ├── DeathMatchManager
        ├── RaceMatchManager
        └── BassballMatchManager
```

Match managers own win conditions, score tracking, and end-of-match transitions.

### Player & Fish Architecture

`Player` and `Gunfish` are **sibling components on the same persistent GameObject** (e.g. `Player0`, alongside `PlayerInput`), not parent/child. This object persists for the whole session via `DontDestroyOnLoad`. `Player` owns identity and input plumbing (device, player number, team, action-map switching, which `GunfishData` was picked); `Gunfish` owns the creature's runtime state (health, effects, movement/firing dispatch) and orchestrates spawning/despawning the physical body.

The physical body (segments, gun, visuals) is a **separate, disposable GameObject hierarchy** — not parented under Player/Gunfish — that `Gunfish.Spawn()`/`Despawn()` builds and tears down on every spawn and respawn, tracked only by reference (`Gunfish.segments`, `Gunfish.gun`):

```
Player0 (persistent, DontDestroyOnLoad)
  ├── PlayerInput
  ├── Player             — identity, input routing, device binding
  └── Gunfish             — creature state, spawn/despawn orchestration

<FishName>Body (disposable — instantiated fresh from a baked prefab every spawn/respawn; see below)
  ├── GunfishSegment[]    — physics chain (Rigidbody2D / CircleCollider2D / FixedJoint2D / DistanceJoint2D)
  ├── GunfishRenderer     — MonoBehaviour on the root segment; owns and self-drives the body's LineRenderer
  ├── Gun (base class)
  │     ├── AutomaticGun / Minigun / GrenadeLauncher
  │     ├── SniperLaser / LaserGun
  │     └── Stungun / Sword / … (~16 weapon subclasses total in Player/Gunfish/Gun/)
  └── Destroyer / CollisionDamageReceiver / CompositeCollisionDetector / GroundDetector  — root segment only
```

- `GunfishRigidbody` is a plain C# wrapper (not a component) that applies forces/torques to the segment chain.
- `GunfishRenderer` is a `MonoBehaviour` living on the root segment — it drives its own `LineRenderer` via its own `Update()`, independent of whether a `Player`/`Gunfish` is currently wired to it.
- `Player` implements `IDeviceController`, `IGunfishController`, and `IUIController`

### Fish Body Generation (Editor-Time Baked Prefabs)

Fish bodies are **baked into prefabs at editor time**, not generated procedurally at runtime. `Gunfish.Spawn(GunfishData data, Vector3 position)` just does `Instantiate(data.fishPrefab, position, Quaternion.identity)` and rewires a handful of per-instance references (per-player physics layer, `GunfishSegment.gunfish`, `Gun.gunfish`, etc.) — no `GameObject`/`Rigidbody2D`/`Joint2D` construction happens at spawn or respawn time anymore. `GunfishGenerator`/`GunfishRenderer` still contain the actual body-building logic, but they're only ever invoked by the baking tool below, never at runtime.

**`GunfishData` field split:**
- Read every spawn, same as always: `maxHealth`, `flopForce`, `groundTorque`, water params, `angularDrag`, `flopCooldown`, `spriteMat`, `gun` (for `maxAmmo` etc.)
- **Bake-time-only** — read only by the baker, no longer by `Spawn()`: `segmentCount`, `width` (AnimationCurve), `mass`, `fixedJointDamping`, `fixedJointFrequency`, `gunOffset`, `gunSegmentIndex`. Editing these has **no effect in Play mode until you re-bake**.
- `fishPrefab` (`GameObject`) — the baked prefab; `Spawn()` throws if this is unset.
- `bakedSnapshotJson` (`[HideInInspector] string`) — a fingerprint of the bake-relevant fields as of the last bake, used to detect when an asset has drifted from its baked prefab.

**Baking tool** (`Assets/Scripts/Editor/GunfishPrefabBaker.cs`, Editor-only static class):
- `BakeFish(GunfishData data)` — builds one complete fish (body + gun + gunSprite + barrels + the 4 root-only components) on a scratch object, saves it to `Assets/Resources/Prefabs/Player/Fish/<FishName>.prefab`, and writes `fishPrefab`/`bakedSnapshotJson` back onto the asset.
- **Tools → Gunfish → Bake All Roster Fish Prefabs** — bakes every fish in `GunfishList.asset` (the live roster only; `GunfishData` assets not in that list are skipped).

**Workflow — tuning an existing fish:** edit any field on the `GunfishData` asset → its Inspector (`GunfishDataEditor.cs`) shows a warning if it's never been baked, or has drifted since its last bake → click **Garbulate** to re-bake before testing in Play mode.

**Workflow — creating a new fish:** create a `GunfishData` asset as before → click **Garbulate** on it directly (works even before it's added to the roster) → add it to `GunfishList.asset` to make it selectable in the fish-select UI.

**Known limitation (not yet fixed):** the body's `LineRenderer` uses world-space positions baked near the origin (baking runs `GunfishGenerator.Generate` at `Vector3.zero`). Dragging a baked fish prefab to a different position in a scene for level-design reference will **not** visually reposition its rendered line — only the physical colliders/segments move to the new spot. The fix is switching `GunfishRenderer` to local space (`LineRenderer.useWorldSpace = false` + `transform.InverseTransformPoint(...)` in `Render()`), not yet applied.

`Gunfish.SwapFish()` / `FishPowerup` exist in code but are **dead/orphaned** — no scene or spawner references `FishPowerup.prefab`. Don't treat mid-match fish-swapping as a live, tested feature.

### Game Modes (ScriptableObjects)

Game modes, gunfish data, and scene lists are defined as **ScriptableObject assets** — edit them in the Inspector, never hardcode values in scripts.

| ScriptableObject | Purpose |
|---|---|
| `GameMode` | Per-mode settings |
| `GameModeList` | Ordered list of available modes |
| `GunfishDataList` | All playable fish configs |
| `SceneList` | Levels available per mode |

### Level Objects

Interactive hazards live in `Scripts/LevelObjects/`: Cannon, PinballFlipper, MovingPlatform, SpinnyWheel, FishHook, Crate, explosive barrels, sea mines, pelicans, water/acid zones, powerups, and more. Game logic boundaries (spawn areas, kill boxes, goals, checkpoints) are in `Scripts/GameLogic/`.

---

## Folder Structure

```
Gunfish/
├── Assets/
│   ├── Scripts/
│   │   ├── Audio/              # AudioPalette, FX_* sound components
│   │   ├── Editor/             # Custom Unity Editor windows; GunfishPrefabBaker.cs + GunfishDataEditor.cs handle fish body baking
│   │   ├── Effects/            # Visual effect ScriptableObjects
│   │   ├── EnvironmentObjects/ # Static environment pieces
│   │   ├── GameLogic/          # KillBox, SpawnArea, Checkpoint, Goal, BassballBall
│   │   ├── General/            # Shared utilities: Destroyer, Fader, GameEvents, IHittable
│   │   ├── LevelObjects/       # All interactive hazards and pickups
│   │   ├── Managers/           # Singleton managers + MatchManagers/
│   │   ├── Parallax/           # Background parallax scroll
│   │   ├── Player/             # Player, Gunfish fish + segments, Gun subclasses
│   │   ├── ScriptableObjects/  # SO type definitions
│   │   ├── UI/                 # All menu pages, HUD, stats, pause, fish select
│   │   └── Utils/              # PersistentSingleton, Singleton, BSpline, extensions
│   ├── Plugins/                # DOTween, SmartLighting2D — do not edit
│   ├── Resources/              # Runtime-loaded assets (ScriptableObjects, prefabs)
│   │   └── Prefabs/Player/Fish/ # Baked fish body prefabs, one per roster fish (see Fish Body Generation)
│   └── Scenes/                 # Unity scene files
├── Packages/
│   ├── manifest.json           # Package dependencies — edit via Package Manager UI
│   └── packages-lock.json
└── ProjectSettings/            # Unity project config — edit via Editor UI, not by hand
```

---

## Testing

| Layer | Method |
|---|---|
| Game logic | Press ▶ in Editor; use keyboard or controllers |
| Multiplayer | Editor + up to 4 gamepads |
| Builds | File → Build Settings → macOS standalone |

No automated test suite. The Unity Test Framework is available but unused.

---

## Issues

Use the template at `.github/ISSUE_TEMPLATE/issue.md` for all bugs and features. Title convention: `[Area] Short description` (e.g., `[Gun] Grenade launcher knockback too strong`). The area should match one of the subsystem names in the template checklist.

---

## Conventions

### Never hand-edit Unity-generated files

| File type | Correct approach |
|---|---|
| `.meta` files | Let Unity generate them on import |
| `Packages/manifest.json` | Use Window → Package Manager UI |
| `ProjectSettings/*.asset` | Use the corresponding Editor settings panel |
| Prefab / Scene files | Edit in the Unity Editor, not in a text editor |

### Other conventions

- **Singletons:** Use `PersistentSingleton<T>` for cross-scene managers, `Singleton<T>` for scene-scoped — never raw `static` fields on MonoBehaviours.
- **Animation:** DOTween for all tweens — not coroutine-based `Lerp` loops.
- **Input:** New Input System action assets only (`PlayerInput` component) — never `Input.GetKey`.
- **Text:** TextMeshPro everywhere — never Unity's built-in `Text` component.
- **Data:** Game mode / fish / scene configs are ScriptableObject assets — don't hardcode values in scripts.
- **`NOTE(Name):`** comments flag architectural debates; grep before refactoring those areas.
- **`TODO` / `FIXME`** are scattered across UI, FishHook, Destroyer, GameManager, GameUIManager — grep before touching those files.

### Color palette

| Name | Hex |
|---|---|
| Deep Sea Blue | `#02478e` |
| Aquamarine | `#7FFFD4` |
| Coral Orange | `#FF7F50` |
| Sun Yellow | `#FFDF00` |
| Neon Green | `#39FF14` |
| Bubblegum Pink | `#FF69B4` |
| Bright Red | `#FF0000` |
