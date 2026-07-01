# CLAUDE.md — Gunfish

## Project Overview

Gunfish is a 4-player local multiplayer 2D arcade game built in Unity 2021.3.45f2. Players control gun-equipped fish that battle across physics-based levels in three game modes: **DeathMatch**, **Race**, and **Bassball**.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Engine | Unity 2021.3.45f2 (see `ProjectSettings/ProjectVersion.txt`) |
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

```
Player (input handler)
  └── Gunfish (character root)
        ├── GunfishRigidbody    — physics movement
        ├── GunfishRenderer     — sprite assembly
        ├── GunfishSegment[]    — body segments
        └── Gun (base class)
              ├── AutomaticGun / Minigun / GrenadeLauncher
              ├── SniperLaser / LaserGun
              ├── Stungun / Sword
              └── … (~16 weapon subclasses total in Player/Gunfish/Gun/)
```

- `Gunfish` is a segmented 2D physics creature; movement is physics-driven via `GunfishRigidbody`
- `Player` implements `IDeviceController`, `IGunfishController`, and `IUIController`

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
│   │   ├── Editor/             # Custom Unity Editor windows
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
