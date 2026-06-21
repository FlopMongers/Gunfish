# Gunfish — Development Milestones

Commits from the `main` branch grouped into 13 milestones. Each milestone ends in a different month and represents a coherent phase of development. (Milestones 12+13 and 14+15 from the original plan were merged.)

---

## Milestone 1: Project Foundation
**Period:** Jan 12 – Jan 22, 2023 (19 commits)
**Last commit date:** 2023-01-22

Initial repository setup. Hinge-joint fish physics prototype. Custom gunfish shader (inverts Y axis). Procedural fish generation via GunfishGenerator. ScriptableObject data model (GunfishData). Unity Input System import. Basic fish movement and GunfishRigidbody. Scene and level manager scaffolding.

**Blog file:** `blog-2023-01-foundation.md`

---

## Milestone 2: Game Loop Architecture
**Period:** Feb 4 – Feb 26, 2023 (8 commits)
**Last commit date:** 2023-02-26

Match Manager. Player class with device management. Player spawning system. Basic menu UI assets. Main menu. Multi-controller testing. GunfishController. InputManager defaults.

**Blog file:** `blog-2023-02-game-loop.md`

---

## Milestone 3: Multi-Controller & Git LFS
**Period:** Mar 5 – Apr 16, 2023 (14 commits)
**Last commit date:** 2023-04-16

Death match manager and auxiliary gameplay managers. BOING (elastic bouncing). Git LFS enabled (PR #11). GameModeDetails ScriptableObjects. Menu encapsulation. Addressables package (added/removed). Fish concept art (FlyFish). Gunfish spawning on debug start.

**Blog file:** `blog-2023-04-multicontroller-lfs.md`

---

## Milestone 4: Menu System
**Period:** May 2 – Jun 11, 2023 (6 commits)
**Last commit date:** 2023-06-11

GameModeSelectMenuPage submit logic. FishSelectMenuPage initial work. PlayerManager relocation. Menu changes and restructuring.

**Blog file:** `blog-2023-06-menu-system.md`

---

## Milestone 5: Core Gameplay Loop
**Period:** Jul 3 – Jul 27, 2023 (30 commits)
**Last commit date:** 2023-07-27

Shooting mechanics. Fish-on-fish violence hit detection. FishSelect UI. DeathMatchUI skeleton. Marquee text with action callbacks. Camera shake and freeze frame. Music Manager. Effect framework (sand, hit effects). Controller support. Underwater movement. Health bars (first pass). Beach ball physics. Pause functionality. Audio mixer. Python socket test for Arduino.

**Blog file:** `blog-2023-07-core-gameplay.md`

---

## Milestone 6: First Playable Demo
**Period:** Aug 3 – Aug 30, 2023 (55 commits)
**Last commit date:** 2023-08-30

Sea mines. Loading screen and countdown UI. Arduino integration (ArduinoManager). Ammo system. Barrel level. Shotgun and pistol weapons. Cannon prototype. Marquee announcer with quips. Health bars (functional). Level stats screen. Smart Lighting 2D (FunkyCode plugin). Camera follow. Spawn FX and kill box. Scene loading rework. Deathmatch UI fixes. Player loading screen. Crate and fishhook level objects. BIG REFACTOR. Gun rework (PR #28). First water experiments.

**Blog file:** `blog-2023-08-first-playable.md`

---

## Milestone 7: Water System
**Period:** Sep 4 – Sep 28, 2023 (16 commits)
**Last commit date:** 2023-09-28

Water shader (part 1). Water surface generator. Buoyancy first attempt. Water physics and swimming. Splash effects. Water ripples. Collision damage first pass. Sea urchin (initial + rework). DOTween import. Sprites folder reorganization. FunkyCode moved to 3rd party.

**Blog file:** `blog-2023-09-water-system.md`

---

## Milestone 8: Visual Polish & Repo Reorganization
**Period:** Oct 5 – Oct 16, 2023 (28 commits)
**Last commit date:** 2023-10-16

Skybox camera movement. Color grading post-processing. Parallax backgrounds. Massive folder reorganization (materials, shaders, sprites, fonts, animations, input, audio, UI all moved to Resources; 3rd party to Plugins). Arduino overhaul ("Kilimanjaro"). Broken pipe level. ScriptableObject renames. Level objects renamed. Pascal case enforcement. Code linting (dotnet format). Toolchain and Input System updates. New sprites (toast, toaster, kelp, hook, rock, urchin, bubble, igneous rock, sand block).

**Blog file:** `blog-2023-10-visual-polish.md`

---

## Milestone 9: Level Art & Gun Rework
**Period:** Nov 10 – Nov 30, 2023 (15 commits)
**Last commit date:** 2023-11-30

Level art passes on Barrel, Crags, and Cargo Hold. Fish-gun split system (PR #31 — fish and gun become separate entities). Upgrade all guns to new system with colliders. Gunfish diameter curves. UI player colors. Broken pipe kill box fix. FishHook line fix. GameManager initialization order. Prefabify managers. Firing range level.

**Blog file:** `blog-2023-11-level-art-gun-rework.md`

---

## Milestone 10: New Fish & Features
**Period:** Dec 6 – Dec 31, 2023 (36 commits)
**Last commit date:** 2023-12-31

Swordfish prototype (PR #45). Flounder shotty fish. Minigun fish. Automatic guns. Sharkmode prototype/powerup. Ground detection rework (PR #37). Fish select menu overhaul. Valley and Pipes levels. Team deathmatch stats. MainMenu background. MatchManager logic updates. Music manager rework. Editor scene swapper (Ctrl+T). Balance pass. Great Bay current. Barrel dynamic physics. FX pass. Offscreen tracker. Underwater rotation fix. Drunken Sailor logic project.

**Blog file:** `blog-2023-12-new-fish-features.md`

---

## Milestone 11: MAGFest 2024 Release
**Period:** Jan 1 – Jan 30, 2024 (101 commits)
**Last commit date:** 2024-01-30

The biggest milestone — preparing for the first public showing at MAGFest 2024.

Pelicans (spawn after timeout). Powerups (invincibility, health, sharkmode). Pufferfish and needlefish characters. Explosive barrels. Moving platforms. Acid water. Conveyor belts. Sea mines update. Factory spawners. Fish hooks. Giant spinny wheel. Cannon improvements. Announcer/quips overhaul (MarqueeManager rewrite). Deathmatch UI improvements. Player color outlines on fish (custom shader with alpha, pulsing, border). Audio mixers. Cabinet UI (controller ordering ritual, attractors, cursor hidden, self-destruct). Underwater shooting. Tiebreaker logic. Health UI rework (#42, #43). Spawn invincibility. Build instructions in README. Massive balance pass across all fish. Level art on Great Bay, Crags, Barrel, Cargo Hold, Pipes. Lasergun and eel first pass.

**Blog file:** `blog-2024-01-magfest-2024.md`

---

## Milestone 12: Post-MAGFest — New Game Modes & Polish
**Period:** Feb 1 – Mar 2, 2024 (20 commits)
**Last commit date:** 2024-03-02

Race mode (checkpoint-based). Bassball mode (goal-based team sport). Pause menu with volume sliders. Bug fixes (swordfish damage, status effects, FX spatial blend). Fish select UI rework with bubble transition. Legacy menu UI backup. Remove cabinet-only ritual. New menu system. Fish art for all characters (bassic, salmon, swordfish, flounder, flyfish, needlefish, pufferfish). Remove lowpass filter from master mixer. Menu icons, aspects, and animations. Powerup refactor. Player layer refactor. Add exposed lowpass filter. Clamp FX bursts to 8 (performance). Package updates.

**Blog file:** `blog-2024-03-new-game-modes.md`

---

## Milestone 13: MAGFest 2025 — Revival & Release
**Period:** Oct 21, 2024 – Jan 30, 2025 (78 commits)
**Last commit date:** 2025-01-30

Revival after 7-month hiatus. Compilation error fixes. POC castle level. Sword tweaks. Laser fish and bassball bug fixes. Replace menu shader with video texture.

Then the MAGFest 2025 push: Tiled terrains system (metal, wood, sand, rocks with borders). Sprite shapes (PR #65). Beach level. Acid factory level rework. Valley/Cargo Hold/Barrel/Firing Range level adjustments. Angler and electric eel fish. Stun gun rework. Bassball mode polish (goals, lighting, level tweaks). Cannon improvements (flame effect, pause at waypoints, DOPunchScale animation). Skybox replaced with video texture. Cabinet ritual re-added. Pipe race map POC. Camera fixes. Player join behavior. Menu sound effects. Gamemode images. Gamemode player number constraints + UI. Level stats UI. Stats/telemetry system (SQLite). Controller deadzone. Pelican fixes. Underwater damage/bullet fixes. Laser pointer on sniper. Win quips re-added. Announcer volume increase. Deathmatch-only mode for MAGFest. Mini gunfish tweaks.

**Blog file:** `blog-2025-01-magfest-2025.md`

---

## Summary Table

| # | Milestone | Period | Last Commit | Commits |
|---|-----------|--------|-------------|---------|
| 1 | Project Foundation | Jan 2023 | 2023-01-22 | 19 |
| 2 | Game Loop Architecture | Feb 2023 | 2023-02-26 | 8 |
| 3 | Multi-Controller & Git LFS | Mar–Apr 2023 | 2023-04-16 | 14 |
| 4 | Menu System | May–Jun 2023 | 2023-06-11 | 6 |
| 5 | Core Gameplay Loop | Jul 2023 | 2023-07-27 | 30 |
| 6 | First Playable Demo | Aug 2023 | 2023-08-30 | 55 |
| 7 | Water System | Sep 2023 | 2023-09-28 | 16 |
| 8 | Visual Polish & Repo Reorganization | Oct 2023 | 2023-10-16 | 28 |
| 9 | Level Art & Gun Rework | Nov 2023 | 2023-11-30 | 15 |
| 10 | New Fish & Features | Dec 2023 | 2023-12-31 | 36 |
| 11 | MAGFest 2024 Release | Jan 2024 | 2024-01-30 | 101 |
| 12 | Post-MAGFest — New Game Modes & Polish | Feb–Mar 2024 | 2024-03-02 | 20 |
| 13 | MAGFest 2025 — Revival & Release | Oct 2024–Jan 2025 | 2025-01-30 | 78 |
| | **Total** | | | **~700** |
