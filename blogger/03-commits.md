# Gunfish Commit History (Jan 2023 – Jan 2025)

Total: ~700 commits on the main branch.

---

## January 2023 (19 commits)

The project begins. Initial repository setup, gunfish generation script, hinge-joint fish physics prototype, custom shader for gunfish sprite rendering, ScriptableObject data model, Unity Input System import, basic fish movement, scene/level manager scaffolding, and GunfishRigidbody.

Key commits:
- 2023-01-12: PURGE — initial repo cleanup
- 2023-01-12: Gunfish test scene
- 2023-01-14: POC Hinge Joint Fish Floppiness
- 2023-01-15: Refined gunfish generation script; custom shader; move GunfishData to ScriptableObject; reorganization
- 2023-01-16: Input system import
- 2023-01-17: ScriptableObject and Input system init; gunfish scripts refactor
- 2023-01-18: Basic movement (needs tweaking); GunfishRigidbody
- 2023-01-22: Initial scaffold for scene and level managers; move Gunfish player scripts

---

## February 2023 (8 commits)

Game loop architecture: Match Manager, Player class, player spawning and device management, basic menu UI assets, main menu, GunfishController compile fix, InputManager defaults.

Key commits:
- 2023-02-04: Match Manager; Player class; game loop design image; basic menu UI assets
- 2023-02-05: Player spawning and device management; GunfishController compile fix; reset InputManager to defaults
- 2023-02-18: Main menu
- 2023-02-26: Multi Controller Testing; merge managers branch into multiple-controllers

---

## March 2023 (5 commits)

Death match manager and auxiliary gameplay managers. BOING (elastic bouncing physics). README update.

Key commits:
- 2023-03-05: BOING (elastic physics); merge branches; Update README
- 2023-03-12: Add death match manager and some other auxiliary managers for gameplay

---

## April 2023 (9 commits)

Git LFS enabled. Menu system encapsulation. GameModeDetails ScriptableObjects. Addressables package (added then removed). Gunfish spawning on debug start. UI control start. Fish concept art (FlyFish).

Key commits:
- 2023-04-02: Enable Git LFS; UI control start; add Fish Concepts and FlyFish
- 2023-04-08: Merge GitLFS PR (#11); merge WorkAssets PR (#12)
- 2023-04-09: Gunfish spawning on debug start
- 2023-04-16: GameModeDetails scriptable objects; Move Menu scripts to UI folder; encapsulated menu management; remove Addressables

---

## May 2023 (4 commits)

Menu system work: GameModeSelectMenuPage submit logic, FishSelectMenuPage initial work, menu changes, move PlayerManager.

Key commits:
- 2023-05-02: Merge PR #13 from ryan branch
- 2023-05-14: GameModeSelectMenuPage submit logic; move PlayerManager; FishSelectMenuPage initial work (Jun 11 commit)

---

## June 2023 (2 commits)

FishSelectMenuPage initial work. Menu changes.

Key commits:
- 2023-06-04: Menu changes commit
- 2023-06-11: FishSelectMenuPage initial work

---

## July 2023 (30 commits)

Core gameplay features come online: shooting mechanics, fish-on-fish violence fix, FishSelect UI, DeathMatchUI skeleton, marquee text with callbacks, camera shake, freeze frame, Music Manager, effect framework, controller support, underwater movement, health bars, beach ball, sand effects, Python socket test (for Arduino).

Key commits:
- 2023-07-03–04: Fishy progress; first pass at fish calibration
- 2023-07-07: Shooty shoot (shooting mechanics!); add README; interface MainMenu with InputSystem
- 2023-07-08: Kill da stinky fish; fish-on-fish violence fixed; move ScriptableObjects and Prefabs to Resources
- 2023-07-09–12: Reorganization; underwater movement; FishSelectMenu bugs; effects (whack smack BOP); GameModeSelectMenu
- 2023-07-13–14: Fish select; placeholder fishes; tweaking gunfish flop
- 2023-07-15: Controller support!; pause + audio mixer; FRESH
- 2023-07-16: Freeze frame + camera shake; health bars (first pass); barrel FX; Unity-to-Python socket test
- 2023-07-18: Music Manager!; Singleton formatting; beach ball + FX collision logic
- 2023-07-19: Sand + effect framework
- 2023-07-22: DeathMatchUI skeleton; basic UI textures; upgrade to 2021.3.28f1
- 2023-07-25: Marquee text with action callbacks
- 2023-07-27: Hook up deathmatch UI stock change event

---

## August 2023 (55 commits)

First playable demo push (for an event). Sea mines, Arduino integration, loading screen, countdown UI, barrel level, shotgun/pistol, cannon prototype, marquee announcer quips, health bars, level stats, lighting system (Smart Lighting 2D), camera follow, spawn FX, kill box, scene loading rework, deathmatch UI fixes, player loading screen.

Key commits:
- 2023-08-03–04: Arduino test audio; ArduinoManager; add gamemodes and fish to GameManager
- 2023-08-05–06: Arduino exception handling; death match manager redesign; scene loading fix; UI scaling fix; deathmatch UI init
- 2023-08-07: Fix deathmatch UI initialization and score update
- 2023-08-11: Reconfigure fish collision detection
- 2023-08-13: Sea mines; layer fixes; FX spawner to game manager
- 2023-08-14: Countdown UI; health bars; shootable 'damaged' state; marquee quips; KillBox; spawn FX; level stats; fish names; cannon work; match end detection fixes; stock text
- 2023-08-15: Hacking to get 1v1 demo working; stats scene
- 2023-08-16: Sea mine bump-trigger explosion; camera follow; freeze frame on kill; stage0 2D version
- 2023-08-17: Lighting package (Smart Lighting 2D); lighting on stages; scene loading/countdown rework; game loop returns to main menu
- 2023-08-18: Kill boundaries; respawn fixes; minor bugfixes; health/name rotation fix
- 2023-08-19: Debug ArduinoManager; Arduino simplification; default Marquee tweaks; PlayerManager simplification; game mode icons; salmon sprite; lighting precision
- 2023-08-20: Add barrel stage
- 2023-08-23: Shotgun and pistol; cannon prototype; BIG REFACTOR; gun rework PR (#28); logo; marquee clips; fishhook; crate; megaflop fix; camera tracking
- 2023-08-24: Crate shadows; freeze frame tweak; bugfix
- 2023-08-28–30: First pass at water (initial experiments)

---

## September 2023 (16 commits)

Water system: water shader, buoyancy, water surface generator, water physics, splashes, collision damage first pass, sea urchin, reorganize sprites folder, DOTween import, physics damage, urchin rework.

Key commits:
- 2023-09-04: Water shader pt 1; first pass at water surface generator; WAWA (water experiments); delete old nodes
- 2023-09-05–07: Buoyancy first attempt; WAWA parts 2 and 3; fish reeled in mechanic
- 2023-09-09: Well level; sand/limestone textures; B-spline tester
- 2023-09-10: Water movement; collision damage first pass
- 2023-09-11: Water ripples
- 2023-09-20: Remove CLS Compliance messages
- 2023-09-24: Move FunkyCode to 3rd party; reorganize sprites; sea urchin stuff; initial sea urchin functionality; remove CBT; update MainMenu
- 2023-09-27: Physics damage first pass
- 2023-09-28: Import DOTween; urchin rework

---

## October 2023 (28 commits)

Visual polish and repository reorganization. Skybox camera, color grading, parallax, sprites/folders reorganized (materials, physics materials, shaders, sprites, fonts, animations, input, audio, UI all moved to Resources). Arduino overhaul (Kilimanjaro). Broken pipe level. ScriptableObjects renamed. Level objects. New sprites (toast, toaster, kelp, hook, rock, urchin, bubble, igneous rock, sand block). Lighting upgrade to Crags. Code linting.

Key commits:
- 2023-10-05: Kilimanjaro (Arduino overhaul)
- 2023-10-08–10: Toast/toaster sprites; kelp; hook, rock, urchin sprites; bubble; lighting upgrade to Crags
- 2023-10-12: Lighting upgrade to Crags
- 2023-10-14: Massive folder reorganization — move 3rd party to Plugins, spline mesh, materials, shaders, sprites, UI, fonts, physics materials, animations, input, audio all to Resources; ScriptableObjects renaming; parallax work; remove unused scripts; broken pipe; fishhook sprite
- 2023-10-15: Update toolchain; dotnet format for linting; Input System update; color grading; skybox lighting; remove SplineMesh; move Audio to Resources; CSharpExtension
- 2023-10-16: Skybox camera movement; fish names; InputSystem rename; ScriptableObject renames; LevelStuff→LevelObjects; Pascal case FX; materials/physics materials renamed

---

## November 2023 (15 commits)

Level art passes (Crags, Cargo Hold, Barrel). Fish-gun split system (PR #31 — separate fish and gun into distinct entities). Firing range level. Prefab managers. Gunfish diameter curves. UI player colors. Kill box in broken pipe.

Key commits:
- 2023-11-10: Crags level art placement
- 2023-11-11: Gunfish diameter curves; UI player colors; broken pipe kill box; FishHook line fix
- 2023-11-12: GameManager initialization order
- 2023-11-19: Cargo Hold art pass
- 2023-11-25: Fish LineRenderer width fix; stiffer fishes; infiniswim fix; scrunglification; merge ryan branch PR #30
- 2023-11-26: Fix merge with shootable class changes
- 2023-11-27: Fish-gun split PR #31 merged; compile error fix
- 2023-11-28: Upgrade other guns to new system, add colliders
- 2023-11-29: Tighten collider for SandBlock
- 2023-11-30: Prefabify managers; remove Level_Well from DeathmatchScenes

---

## December 2023 (36 commits)

New fish (Swordfish, Flounder shotty, minigun fish) and features. Automatic guns. Sharkmode prototype/powerup. Ground detection rework (PR #37). Fish select menu overhaul. Valley and Pipes levels. Team deathmatch stats. MainMenu background. Fish-gun panel refactor. Match manager logic updates. Music manager rework. Editor scene swapper. Balance pass. Great Bay level current. Barrel dynamic physics. FX pass. Level stat UI update. Offscreen tracker.

Key commits:
- 2023-12-06: Panel refactor
- 2023-12-08: Add Flounder shotty fish, fingergun for Defaultfish
- 2023-12-09: Fish select menu overhaul
- 2023-12-11: MainMenu background
- 2023-12-13: Automatic guns; underwater detection tweak
- 2023-12-14: Fix water splooshing; issue #35
- 2023-12-15: ArduinoManager update; missing gun references
- 2023-12-17: Ground detection rework PR #37 merged; fade transitions; game start countdown refactor; MatchManager logic update; show stats; input actions update; tracked work assets
- 2023-12-18: Game loop bugfix; minigun fish; sharkmode powerup
- 2023-12-19: Sharkmode prototype (first pass); fish detector bug fix; gunfish folder reorg; scenes folder reorg; water prefab fix; Great Bay update
- 2023-12-21: Swordfish prototype PR #45 merged
- 2023-12-23: Fix underwater rotation; air/water torque; offscreen tracker; gamepad joystick for fish selection; FXBang SFX fix; IgneousRock prefab; remove hitsplat from title; broken paths fix; move Logic folder; Drunken Sailor logic project
- 2023-12-24: Barrel dynamic physics; water script fix
- 2023-12-25: Level stat UI update; editor scene swapper (Ctrl+T); The Bonny Ship the Diamond
- 2023-12-26: Pipe, lantern, bgrock sprites
- 2023-12-28: Balance pass; Valley level
- 2023-12-29: Default chickenwave on start; chain on barrel lantern; current in Great Bay; music manager (broke then fixed)
- 2023-12-31: Restore default camera size; FX pass; restore music manager; fix gamemode ScriptableObjects

---

## January 2024 (101 commits)

**MAGFest 2024 release crunch.** Pelicans (spawn after timeout), powerups (invincibility, health, sharkmode), pufferfish, needlefish, explosive barrels, moving platforms, acid, conveyor belts, sea mines, factory spawners, fish hooks, giant wheel, cannons, quips/announcer overhaul, MarqueeManager overhaul, deathmatch UI improvements, player colors/outlines on fish, fish shader (alpha, pulsing, border), audio mixers, cabinet UI (Arduino, controller ordering ritual, attractors), bullet improvements, underwater shooting, balance pass (many fish tweaks), level art (Great Bay, Crags, Barrel, Cargo Hold, Pipes), tiebreaker logic, health UI rework, spawn invincibility, build instructions in README.

Key commits:
- 2024-01-01: RELEASE THE PELICANS; update FX; shrink Pipes; lighting
- 2024-01-02: Sharkmode powerup sprites; toasty toast; spawner fixes
- 2024-01-03: Cannons
- 2024-01-04: Invincibility powerup; health pickup; crate pickups; FAT SALMON/FLOUNDER; player color outlines on fish; outline shader; blip sounds; level subset selection
- 2024-01-05: Fish border shader (pulsing, transparency); fader/destroying component; debug removals; player colors
- 2024-01-06: Audio mixers; fish alpha shader; rock material; barticles; turtle to Great Bay; water build support; powerups fade; poof effect; fog shader fix
- 2024-01-07: Music crossfade; sharkmode fix; gun fixes; fish hit tweaks; water drag; Great Bay overhaul; level assets fix; ground detection fix; broken pipe fix; fish hook fix; bigger borders
- 2024-01-08: Moving platform; explosive barrel; shotgun multi-hit fix; fish hook null ref
- 2024-01-09: Explodey barrel with sea mine updates
- 2024-01-10: Tiebreaker logic; matchmanager player maps rework; explosion updates; fish tweaks; improve music looping
- 2024-01-11: FISHHOOK level object; button; giant spinny ship wheel; HealthUI update (#42, #43); acid; wheel/urchin sprites; offscreen indicator; spawn logic
- 2024-01-12: MarqueeManager overhaul + level titles; explosion sound on cannon; swim PR #53 merged; shocky crab; pelicans; grenade-launching pufferfish; collision damage tweaks; prototype level reorg; acidwater/conveyor belt test; spawn fix; splash fix; ground shooting fix
- 2024-01-13: Fix barrel scene
- 2024-01-14: Announcer (new); death quip audio; underwater bullets/shooting; remove debug UI; fish button unselect; gunfish attractors
- 2024-01-15: Swordfish for real + art; pufferfish; explosive barrel sprite; pelican sprites; crab sprite; pelican spawn rate; moving boxer crab; bassic fish; player numbers; prevent attractors; pelican bug fix
- 2024-01-16: Needlefish assets; piercing gun boolean; dancing crab; FISHICIDE; HAAAAAAAAARK; gun flopping fix; factory spawner; QUIPS; water/level object reorg; wins→points; fish tweaks; debug bool inspector; sword damage
- 2024-01-17: Many balance/polish commits: fix explosions + pufferfish rebalance; controller order ritual; MarqueeManager audio; quip adjustments; mine nullRef fix; DeathmatchUI visibility/indicators; fish selector fix; powerup sprites; shark mode sound effect; self-destruct on cabinet; firing range tweaks; quip fixes; sharkmode stop fix; sharkmode music; controller order fix; invincibility on spawn; bullets fade; Great Bay tweaks; UI bloop; instructions; icons; needle sniper; minigun sounds; flounder balance
- 2024-01-18: Conveyor belt spawn points; cursor invisible; player color fix; Arduino update; player widget; control image; round 2 fix; InputManager fix; effect bug fix; sword tweak; device ordering; hit counter; explosive barrel kills; sniper fix; audio mix; hark clip; auto level transition; pelican text; 1 level per match; attractor timer; stop crates/logs killing; deadzone for aerial movement; remove flop while swimming
- 2024-01-19: Debug commit
- 2024-01-25–26: Bullet stuff; fish drag tweak; barrel bullet fix; grenade launcher cooldown; pufferfish contact damage; lasergunfish first pass; eel first pass; lasergun bugfixes; level object tweaks
- 2024-01-27: Level stats rework; matchmanager rework; acid factory (cannon, acid stuff); pelicans on pelican layer; acid water fix; accidental main menu push
- 2024-01-30: Level stats rework; minor matchmanager rework; bugfixes

---

## February 2024 (16 commits)

Post-MAGFest: New game modes. Race mode logic. Bassball mode code. Pause menu. Volume sliders. Bug fixes (swordfish damage, status effects, FX spatial blend). Fish select UI rework. Bubble transition test. Legacy menu UI backup. Remove cabinet ritual. New menu system. Fish art (bassic, salmon, swordfish, flounder, flyfish, needlefish, pufferfish). Remove lowpass filter. Menu icons/aspects/animations. Powerup refactor.

Key commits:
- 2024-02-01: Race mode logic done; ScriptableObject changes; main menu
- 2024-02-02: Shitty gameplay pause menu
- 2024-02-03: Pause menu volume sliders; FX spatial blend fix; lots of bug fixes; deathmatch ties; racemode fixes; status effect removal bug; default gamemode
- 2024-02-05: Volume slider fix; swordfish damage fix; show cursor on pause
- 2024-02-06: Legacy Menu UI backup; remove cabinet-only ritual; bassball code; script reorg
- 2024-02-07: Bassball is in!; racemode manager tweak; FishSelectMenu and Bubble transition; FishSelectUI; game mode select UI; merge main into develop
- 2024-02-10: Game mode select UI
- 2024-02-27: Remove lowpass filter from master mixer; fish art (bassic, salmon, swordfish)
- 2024-02-28: New menu system; fish art (flounder, flyfish, needlefish, pufferfish)
- 2024-02-29: Fix menu icons/aspects; menu animations; powerup refactor; player layer refactor; gunfish game gamemode stub; remove debug prints; add back game modes

---

## March 2024 (4 commits)

Add exposed lowpass filter, SetParent usage, clamp FX bursts to 8, update packages, init fish panels on start.

Key commits:
- 2024-03-02: Add exposed lowpass filter; use SetParent; clamp FX bursts to 8; update packages; init Fish Panels on Start

---

## April–November 2024 (0 commits on main)

No commits on the main branch during this period. The project was on hiatus.

---

## October 2024 (1 commit)

- 2024-10-21: POC castle level

---

## December 2024 (5 commits)

Revival and MAGFest 2025 prep. Compilation errors fixed. Sword tweaks. Laser fish bug fix. Bassball mode map assignment fix. Menu shader replaced with video texture. Video clips to replace shaders.

Key commits:
- 2024-12-27: Compilation errors fixed; slight sword tweak
- 2024-12-28: Fixed laser fish bug; fixed bassball map assignment; replace menu shader with video texture; video clips

---

## January 2025 (72 commits)

**MAGFest 2025 release crunch.** Tiled terrains system. Sprite shapes (PR #65). Beach level. Acid factory level rework. Valley/Cargo Hold/Barrel/Firing Range level adjustments. Angler and electric eel fish. Stun gun rework. Bassball mode polish (goals, lighting, level tweaks). Cannon improvements (flame effect, pause at points, DOPunchScale). Skybox video texture replacement. Cabinet ritual re-added. Pipe race map POC. Camera fixes (hydrophobic fix, layer settings). Player join behavior. Menu bloops/blips. Gamemode images. Gamemode player number constraints + UI. Fish descriptions (unused). Level stats UI. Stats/telemetry system. Survey UI (then disabled). Controller deadzone. Pelican fixes. Underwater damage/bullet fixes. Laser pointer on sniper. Win quips re-added. Announcer volume. Deathmatch-only mode. Mini gunfish tweaks.

Key commits:
- 2025-01-03: Add tiled terrain sprites
- 2025-01-04: Gamemode player number constraints + UI
- 2025-01-06: Merge sprite-shapes PR #65
- 2025-01-13–14: Angler and eel sprites; tiled terrain border art
- 2025-01-16: Beach level; acid factory; firing range tweaks; valley/beach adjustments; beachball sprite; cannon improvements (flame, pause, DOPunchScale); menu bloops; gamemode images; new fishies; level tweaks; platform effector; Arduino louder; skybox video texture
- 2025-01-17: Pipe race map POC; beach tweaks; camera hydrophobic fix
- 2025-01-19: Cabinet ritual re-added; beach/valley/acid factory/barrel adjustments; bassball goals/lighting; player join behavior; camera fixes; healthUI bug fix; fish meta files; level stats graphics; start game countdown bug fix; player default input mode
- 2025-01-20: Massive polish day — stun gun fixes (many iterations); bassball tweaks; level tweaks; grenade launcher tweak; fish tweaks; mini gunfish; splash effect clamp; ritual/debug settings; cannon flame; pause/level loading in editor; fish descriptions; laser gun/grenade launcher tweaks; DOPunchScale cannon; debug cleanup
- 2025-01-21: Remove game mode select menu; tweak mini gunfish; disable survey UI; deathmatch only; cargo hold adjustments; stats on gamemode teardown; dummy survey; underwater bullet velocity; laser pointer on sniper
- 2025-01-22: Controller deadzone; underwater damage fix; pelican fixes; stun gun bug fix; win quips re-added; bullet tweaks; remove last fish standing/team text; lasergun collider tweak; deathmatch debug; marquee off-by-one fix
- 2025-01-24: Remove lives from deathmatch; skip level stats on last level; make announcer LOUDER; reduce auto timer for level stats
- 2025-01-30: Stats!
