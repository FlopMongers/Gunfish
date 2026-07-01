# Gunfish — Codebase Overview

## Project Structure

Gunfish is a Unity 2021 LTS project. All game code lives under `Gunfish/Assets/Scripts/`, organized into these major subsystems:

```
Scripts/
├── Managers/           # Core game management (singletons)
│   └── MatchManagers/  # Game mode-specific match logic
├── Player/             # Player controller and fish/gun systems
│   └── Gunfish/
│       ├── Fish/       # Fish body, data, rendering, generation
│       └── Gun/        # Weapon base, gun types, bullets
├── GameLogic/          # Mode-specific mechanics (goals, checkpoints, bassball)
├── LevelObjects/       # Interactive level elements
│   ├── BaseScripts/    # Shootable, FishDetector, GroundMaterial
│   ├── CollisionDetector/ # Composite collision system
│   ├── Water/          # Water surface, buoyancy, acid variant
│   ├── Powerups/       # Invincibility, sharkmode, health
│   ├── Explosives/     # Barrels, sea mines, explosions
│   ├── LevelHazards/   # Toaster, zap, urchin, crab
│   ├── Pelicans/       # Pelican enemy + spawner
│   └── Spawner/        # Object spawning
├── UI/                 # Menus, HUD, marquee announcer, pause
├── Audio/              # FX_Spawner, sound effects
├── Effects/            # Gameplay effects (invincibility, sharkmode, zap, flame)
├── ScriptableObjects/  # Data definitions (GameMode, lists)
├── General/            # Events, interfaces, utilities
├── Utils/              # Singleton bases, extensions, B-splines
├── Parallax/           # Background parallax scrolling
├── EnvironmentObjects/ # Static environment pieces
└── Editor/             # Editor tools (scene selector, custom inspectors)
```

---

## Architecture Patterns

### Singleton Pattern (Two Variants)

The codebase uses two singleton base classes:

- **`Singleton<T>`** — Non-persistent, destroyed on scene load. Used by: GameCamera, MarqueeManager, MainMenu, SharkmodeManager, ArduinoManager.
- **`PersistentSingleton<T>`** — Survives scene loads (DontDestroyOnLoad). Used by: GameManager, PlayerManager, LevelManager, GameModeManager, StatsManager, MusicManager, FX_Spawner, PauseManager.

### Event/Delegate System

`GameEvents.cs` defines custom delegate types used across the codebase for decoupled communication:
- `GameEvent` — No-arg events
- `PlayerGameEvent`, `FishEvent`, `FishCollisionEvent` — Entity events
- `FishHitEvent`, `HitEvent`, `CheckpointEvent`, `OnGoalEvent` — Gameplay events
- `FloatGameEvent`, `CountGameEvent` — Value events

Systems subscribe to these delegates (level load, death, UI transitions) rather than directly coupling to each other.

### Component-Based Design

Level objects use Unity's component pattern heavily:
- **CompositeCollisionDetector** with child **SubCollisionDetector** components for multi-part hit detection
- **GroundDetector** for raycast-based ground contact
- **CollisionDamageDealer/Receiver** for physics-based damage exchange
- **OomphCalculator** for impact force calculations

### Generic Match System

`MatchManager<PlayerRef, TeamRef>` is a generic base class parameterized on player and team reference types:
- `DeathMatchManager` uses simple player/team references with stock counts
- `RaceMatchManager` tracks checkpoints per player
- `BassballMatchManager` uses `ScoredTeamReference` with goal counts

### ScriptableObject Data Model

Game data is defined via ScriptableObjects stored in `Assets/Resources/ScriptableObjects/`:
- **GunfishData** — Fish properties: dimensions, segment count, width curve, physics (mass, damping, frequency), movement (flop force, torque), water physics, health, gun reference, sprite, material
- **GunData** — Weapon properties: barrel config, kickback, range, damage, knockback, ammo, reload time, bullet prefab
- **GameMode** — Mode type, display image, level set (SceneList), rounds, player count constraints, MatchManager prefab
- **GunfishDataList** / **GameModeList** / **SceneList** — Collection SOs that aggregate items
- **TrackSet** — Music collections (Menu, Gameplay, Sharkmode)
- **Effect_SO** — Effect templates (e.g., SandFlop)

---

## Core Systems

### GameManager (Orchestrator)

The root PersistentSingleton. Holds references to the GunfishDataList and GameModeList. Manages the initialization chain:

1. **PlayerManager** initializes, waits for player input connections
2. On player count reached → **GameManager.InitializePostRitualManagers()**
3. Chain: LevelManager → MusicManager → ArduinoManager → FX_Spawner → MarqueeManager → PauseManager → GameModeManager → MainMenu
4. **InitializeGame()** spawns the MatchManager and starts the game mode

### Player System

**Player.cs** is the controller for a single human player. It holds:
- `PlayerInput` (Unity New Input System) — routes actions (Move, Fire, Respawn, Navigate, Pause)
- Reference to the spawned `Gunfish` and `Gun`
- Player color, layer, team number
- Input mode switching (Player / UI / EndLevel / Null action maps)

### Fish System

**Gunfish.cs** is the main fish class (implements `IHittable`):
- Segmented body — a list of GameObjects with Rigidbody2D physics connected by hinge joints
- **GunfishGenerator** — Procedurally generates fish body: creates segment GameObjects, circle colliders, fixed joints, positions gun
- **GunfishRenderer** — Uses Unity LineRenderer to draw the fish body from segment positions
- **GunfishRigidbody** — Physics wrapper that applies forces across all segments
- Health system with damage handling and death events
- Effect system: map of active effects (invincibility, sharkmode, paralysis, flame, etc.)
- Underwater detection and water-based physics (different movement in water zones)
- Respawn mechanism via held button press

### Gun/Weapon System

**Gun.cs** is the base weapon class:
- Ammo/reload cycle
- Fire cooldown and rate of fire
- Kickback application to the owning fish
- Multi-barrel support (barrel positions defined in GunData)

Weapon subtypes:
- **AutomaticGun** — Sustained fire while button held
- **Minigun** — Rapid fire with rev-up
- **LaserGun** — Charge-based beam with expanding radius
- **Stungun** — Fires paralysis projectiles (Stunbullet → Zap_Effect)
- **GrenadeLauncher** — Fires explosive grenades
- **Sword** — Melee weapon with charge/dash multipliers, uses CompositeCollisionDetector
- **SniperLaser** — Long-range with laser pointer

Projectiles: **Bullet** (ballistic), **Grenade** (explosive with timer/impact), **Stunbullet** (paralysis), **SwordDamageDealer** (melee collision).

### Damage Pipeline

1. Bullet/collision triggers contact
2. **MatchManager.ResolveHit()** checks team relationships
3. **GunfishSegment.Hit()** called with HitObject data
4. **Gunfish.Hit()** applies damage, checks for invincibility effect
5. **StatsManager** logs the damage event
6. Death check → triggers respawn or end-level condition

Hit data structures:
- **HitObject** — damage, HitType (Ballistic/Explosive/Impact), position, source
- **FishHitObject** extends HitObject — adds segment index and direction

### Match Lifecycle

1. GameModeManager selects levels randomly from the mode's SceneList
2. Creates MatchManager instance for the chosen game mode
3. LevelManager loads first level + skybox scene
4. MatchManager receives load callback → spawns players at spawn points with delay
5. Countdown timer (3-2-1-GO via LoadingCountdownUI)
6. Gameplay timer starts (default 90 seconds)
7. Level ends → Stats displayed (StatsUI) → Wait for continue input → Next level
8. After all levels → Final scores → Return to main menu

---

## UI System

### Menu System (Page-Based State Machine)

**MainMenu.cs** (Singleton) manages a stack of menu pages:
- **SplashMenuPage** — Intro/branding, idle attractors for cabinet
- **GameModeSelectMenuPage** — Mode selection with panels per mode
- **FishSelectMenuPage** — Per-player fish/weapon selection with panels

Pages use DOTween for slide transitions. Input routes through UI action map.

### In-Game UI

- **MatchUI** — Per-player widgets showing stocks, health, scores. Color-coded.
- **HealthUI** — Health bar rendered near each fish
- **MarqueeManager** — Scrolling text announcer with audio quips (kill quips, selection quips, countdown, win/goal announcements). Quips are categorized by type with random selection.
- **OffscreenTracker** — Arrow indicators pointing toward off-screen fish
- **LoadingCountdownUI** — Pre-match countdown overlay
- **PauseManager** — Pause menu with volume controls
- **StatsUI** — Post-level statistics display

---

## Audio System

### Music (MusicManager)

PersistentSingleton with three track sets (Menu, Gameplay, Sharkmode). Uses dual AudioSources for crossfading with AnimationCurve-based timing.

### Sound Effects (FX_Spawner)

PersistentSingleton that pools and spawns particle/sound effects. Uses an FXType enum with ~20 categories (Bang, Fish_Death, Fish_Hit, Flop, Splash, Spawn, etc.). Rate-limits effects per type to prevent audio spam.

---

## Effects System

**Effect** is a serializable base class with subtypes:
- **FlopModify_Effect** — Multiplicative flop force modifier (e.g., sand movement)
- **TimedEffect** — Auto-removes after duration
  - **Invincibility_Effect** — Nullifies damage
  - **Sharkmode_Effect** — Doubles water physics, damages other fish on contact, triggers special music
  - **Zap_Effect** — Paralysis with random movement injection, cycles through delay/zap states
  - **Flame_Effect** — Damage aura with particle effects
- **NoMove_Effect** — Locks movement (counter-based stacking, used by Zap)

Effects are stored in a dictionary on each Gunfish instance and updated each frame.

---

## Water System

- **WaterZone** — Manages an array of WaterSurfaceNodes forming the water surface. Handles splash propagation, buoyancy forces, and fish submersion detection.
- **WaterSurfaceNode** — Individual point with spring physics for wave simulation
- **WaterSurfaceGenerator** — Procedurally generates the water mesh
- **AcidWaterZone/AcidWaterSurfaceGenerator** — Damage-over-time variant
- **SplashEffect** — Visual splash particles scaled by impact force

Fish have separate movement physics when underwater (different velocity limits, force multipliers, torque behavior).

---

## Stats/Telemetry (MAGFest 2025 Addition)

**StatsManager** uses SQLite for persistent storage. Tracks:
- **MatchResult** — Game mode, timestamps, level count, player count
- **LevelResult** — Level name, timestamps
- **PlayerMatchResult** — Player ID, team, fish selection, score, rating
- **PlayerDamage** — Source, damage amount, fatal flag, XY position
- **PlayerSpawn** — Spawn location and time
- **PowerUpPickup** — Type, time, location

Telemetry is collected automatically during gameplay via MatchManager callbacks.

---

## Arduino/Cabinet Integration

**ArduinoManager** (Singleton) communicates via serial (COM3/4/5, 9600 baud) to drive cabinet LEDs. It sends a loudness byte (0–255) sampled from an AudioSource. Supports an attractor mode with configurable timing for idle cabinet display. Gracefully falls back when no Arduino is connected.

---

## Key File Paths

| System | Path |
|--------|------|
| GameManager | `Assets/Scripts/Managers/GameManager.cs` |
| PlayerManager | `Assets/Scripts/Managers/PlayerManager.cs` |
| MatchManager | `Assets/Scripts/Managers/MatchManagers/MatchManager.cs` |
| DeathMatchManager | `Assets/Scripts/Managers/MatchManagers/DeathMatchManager.cs` |
| Player | `Assets/Scripts/Player/Player.cs` |
| Gunfish | `Assets/Scripts/Player/Gunfish/Fish/Gunfish.cs` |
| GunfishData (SO) | `Assets/Scripts/Player/Gunfish/Fish/GunfishData.cs` |
| GunfishGenerator | `Assets/Scripts/Player/Gunfish/Fish/GunfishGenerator.cs` |
| Gun | `Assets/Scripts/Player/Gunfish/Gun/Gun.cs` |
| GunData (SO) | `Assets/Scripts/Player/Gunfish/Gun/GunData.cs` |
| Effect | `Assets/Scripts/Effects/Effect.cs` |
| WaterZone | `Assets/Scripts/LevelObjects/Water/WaterZone.cs` |
| FX_Spawner | `Assets/Scripts/Audio/FX_Spawner.cs` |
| MainMenu | `Assets/Scripts/UI/MainMenu.cs` |
| MarqueeManager | `Assets/Scripts/UI/MarqueeManager.cs` |
| GameEvents | `Assets/Scripts/General/GameEvents.cs` |
| Singleton | `Assets/Scripts/Utils/Singleton.cs` |
| PersistentSingleton | `Assets/Scripts/Utils/PersistentSingleton.cs` |
| StatsManager | `Assets/Scripts/Managers/StatsManager.cs` |
| ArduinoManager | `Assets/Scripts/Managers/ArduinoManager.cs` |
