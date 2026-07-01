# Milestone 11: MAGFest 2024 Release

**January 1--30, 2024 | 101 commits**

This was the largest milestone in the project's history. MAGFest 2024 represented the first public showing of Gunfish on a custom arcade cabinet, and the team committed 101 changes in thirty days to get the game from a development build to a convention-ready product. The work spanned new characters, new level objects, a UI overhaul, audio infrastructure, cabinet-specific features, and a comprehensive balance pass across the entire roster.

## New Characters and Weapons

### Pufferfish

The Pufferfish was paired with the grenade launcher. Its defining mechanic is contact damage on puff -- when the Pufferfish inflates, other fish that collide with it take damage. This gives the Pufferfish a defensive option that no other character has: a passive damage zone around its body. The grenade launcher complements this with area denial, lobbing explosive projectiles that force opponents to reposition.

### Needlefish

The Needlefish was paired with the needle sniper, a long-range piercing weapon. Sniper shots pass through targets, allowing a single shot to hit multiple fish if they are lined up. The Needlefish's long, narrow body profile and high-damage single-shot weapon created a sharpshooting archetype that contrasted with the close-range Swordfish and spread-fire Flounder.

### Laser Gun and Eel

First implementation passes were made on the laser gun and the eel character. These were functional but not yet polished -- playable in internal testing but not included in the final MAGFest roster. They remained in the codebase for continued development after the convention.

## New Level Objects

### Pelicans

The `PelicanSpawner` introduces flying pelican enemies into deathmatch arenas after a timer expires (configured at 120 seconds). Pelicans target active fish and fly toward them, creating pressure on players who are stalling or camping. The spawn rate increases over time -- `spawnTimerRange` decreases by `spawnTimerRangeDecreaseRate` each frame, and pelican speed increases by `pelicanSpeedRangeIncreaseRate`, both approaching configured limits. This mechanic forces matches toward resolution and punishes passive play.

### Powerups

Three powerup types were implemented. Invincibility grants temporary damage immunity. Health pickups restore HP. Sharkmode (carried forward from its Milestone 10 prototype) doubles underwater force and velocity, enables contact damage, and triggers dedicated music through the `SharkmodeManager`. Powerups spawn in arenas as collectible items, adding a resource-control dimension to combat.

### Explosive Barrels

Destructible barrel objects were added that detonate on sufficient damage and chain-explode when near other barrels. Each barrel uses a `HitCounter` component that tracks which player last dealt damage to it, enabling proper kill attribution when a barrel explosion eliminates a fish. The chain explosion mechanic creates emergent moments where a single shot can trigger a cascade of explosions across an arena.

### Environmental Hazards and Platforms

Moving platforms follow B-spline paths through arenas, creating dynamic geometry that players must track and ride. Acid water zones (`AcidWaterZone` / `AcidWaterSurfaceGenerator`) function as `WaterZone` variants that deal damage over time to any fish submerged in them, turning water from a movement tool into a hazard. Conveyor belts apply directional forces to fish passing over them. Factory spawners generate objects on configurable timers. Fish hooks provide a line-based catching mechanic. A giant rotating wheel obstacle was added as a large-scale dynamic hazard. Sea mine behavior received updates for improved collision and damage behavior.

## Announcer and Quip System

The `MarqueeManager` was rewritten with a categorized quip system. The `QuipType` enum defines categories including `PlayerDeath`, `FishSelection`, per-player and per-team win announcements (`Player1Wins` through `Player4Wins`, `Team1Wins` through `Team4Wins`), tie and no-winner conditions, countdown sequence (`Three`, `Two`, `One`, `Ready`, `Flop`), `Goal` for sports modes, `Pelicans` for pelican spawn alerts, and `Attractor` for idle-mode announcements. Each category maps to a list of quip audio clips, and `PlayRandomQuip` selects randomly from the appropriate list. This replaced the previous system where announcer lines were triggered ad-hoc and made it straightforward to add new voice lines for any game event.

## UI and Visual Polish

### Player Color Outlines

A custom shader was implemented to render colored outlines on fish sprites. The shader exposes parameters for alpha, pulse rate, and border color, allowing each player's fish to display a distinct colored border that pulses during certain states. This solved a visibility problem in four-player matches where similarly colored fish were difficult to distinguish, especially during chaotic close-quarters combat.

### Health UI Rework

Health bar display received improvements addressing issues #42 and #43. The updated health bars are more readable at a glance and handle edge cases like damage-over-time and rapid multi-hit scenarios more gracefully.

### Deathmatch UI

Visibility fixes were applied to the deathmatch HUD, and player indicators were improved to maintain readability across different arena backgrounds and lighting conditions.

## Audio Infrastructure

Separate audio mixer channels were established for music, sound effects, and announcer voice. This allowed independent volume control for each category and enabled features like ducking SFX during announcer lines. The mixer architecture also supported the cabinet's physical volume controls.

## Game Logic

Tiebreaker logic was implemented to resolve matched scores at the end of a round or match, preventing ambiguous outcomes. Spawn invincibility grants a brief window of damage immunity after a fish respawns, preventing spawn-camping. Underwater shooting was enabled so that bullets function correctly inside water zones, expanding combat options in water-heavy arenas.

## Cabinet-Specific Features

### Controller Ordering Ritual

The arcade cabinet uses a physical controller setup ceremony. `GameManager.InitializePostRitualManagers` is called after the controller ordering completes, ensuring that all manager singletons initialize only after player-to-controller mapping is established. The sequence starts with `PlayerManager`, then proceeds through `LevelManager`, `MusicManager`, `ArduinoManager`, `FX_Spawner`, `MarqueeManager`, `PauseManager`, `GameModeManager`, and `MainMenu`.

### Attractors

When the cabinet is idle and no players are active, attractor-mode fish animations play on the splash screen. These are physics-driven fish flopping across the screen, serving as an attract mode to draw in passersby at the convention. The `MarqueeManager` plays `QuipType.Attractor` quips during this state.

### Other Cabinet Features

The cursor is hidden during gameplay via `Cursor.visible = false` in `GameManager.Awake()`. A self-destruct button on the cabinet hardware was wired up as a supported input. Build instructions were added to the project README to document the cabinet build process.

## Balance Pass

A comprehensive balance pass was executed across all fish characters and weapons. This included tuning mass, flop force, torque values, max health, and gun parameters for every fish in the roster. Level-specific tuning was applied to Great Bay, Crags, Barrel, Cargo Hold, and Pipes -- adjusting hazard damage values, spawn positions, water zone parameters, and camera bounds.

## Summary

Milestone 11 transformed Gunfish from a functional multiplayer prototype into a convention-ready arcade game. The addition of Pufferfish and Needlefish brought the playable roster to a size that supported varied matchups. Level objects like pelicans, explosive barrels, and acid water added environmental complexity. The announcer quip system, player color outlines, and health UI improvements made the game readable to spectators watching over players' shoulders at a convention booth. Cabinet-specific features like the attract mode, controller ordering ritual, and hidden cursor addressed the unique requirements of running unattended on custom hardware in a public space. The 101 commits in thirty days reflect the intensity of the push, but the result was a game that ran at MAGFest 2024 on a custom arcade cabinet for its first public audience.