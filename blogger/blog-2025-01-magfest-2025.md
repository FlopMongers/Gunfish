# Milestone 13: MAGFest 2025 -- Revival & Release

**October 21, 2024 -- January 30, 2025 | 78 commits**

After seven months of inactivity -- zero commits on main from April through September 2024 -- the FlopMongers returned to Gunfish in October with the goal of bringing the game back to MAGFest in January 2025. What started as a slow revival with six commits over three months accelerated into a seventy-two-commit January sprint that added new fish, new levels, new systems, and significant gameplay polish. This was the largest single push since the original MAGFest 2024 preparation.

## Revival Phase (October--December 2024)

The first commits after the hiatus dealt with getting the project back into a buildable state. Compilation errors that had accumulated from Unity version drift were fixed. A proof-of-concept castle level was started as a new arena. Sword parameters were tweaked, a laser fish bug was fixed, and bassball map assignment was corrected. The menu background shader was replaced with a video texture, and video clips were introduced to replace other shader-based visual effects -- a pragmatic swap that reduced GPU overhead while improving visual consistency.

## Tiled Terrains

A new terrain art system was built using hand-authored tilesets. Four material types were created -- metal, wood, sand, and rocks -- each with a base tile and a matching border tile for edge treatment. These assets live in `Resources/Sprites/Level Assets/TiledTerrains/` and provide a modular palette for constructing arena geometry with consistent visual style. Unity's SpriteShape system was also integrated via PR #65, enabling organic terrain outlines that follow spline curves rather than rigid tile boundaries.

## Stats and Telemetry

A `StatsManager` was implemented using a SQLite database backend to collect gameplay telemetry automatically during matches. The system records structured data across multiple tables: `MatchResult` for overall match outcomes, `LevelResult` for per-level results, `PlayerMatchResult` for individual player performance, `PlayerDamage` for damage events, `PlayerSpawn` for spawn tracking, and `PowerUpPickup` for item collection. Data collection hooks into existing `MatchManager` callbacks, requiring no manual instrumentation of gameplay code. A survey UI was also built but disabled before the event.

## New Fish

Two new characters were added to the roster. The angler fish is a deep-sea themed character paired with the laser gun, bringing a new visual identity and weapon pairing to selection. The electric eel is paired with the stun gun, which fires paralysis projectiles using a `Zap_Effect` that temporarily locks down the target fish's movement and input. The stun gun itself received a rebalancing pass to ensure the paralysis duration and projectile behavior were tuned for fair play.

## New and Reworked Levels

The beach level was built as a sandy shore arena featuring water zones, a beachball physics object, a cannon, and a turtle. The acid factory level received a significant rework, transforming it into an industrial arena with acid water hazards, conveyor belt platforms, cannons, and button-activated moving platforms. Adjustments were made to the Valley, Cargo Hold, Barrel, and Firing Range levels to improve camera framing and gameplay flow. A pipe race map was prototyped as a proof of concept.

## Gameplay Improvements

Cannon behavior was overhauled with several additions: a flame particle effect on firing, pause behavior at patrol waypoints, and a `DOPunchScale` animation for visual feedback on launch. Controller deadzone support was added to filter out stick drift on worn gamepads -- important for an arcade cabinet setting where controllers see heavy use. Underwater combat received fixes so that bullets no longer lose velocity when passing through water volumes and damage registers correctly in submerged conditions. A laser pointer was added to the sniper weapon as an aiming aid. The pelican -- an environmental hazard -- was moved to the correct physics layer and given pathfinding fixes to prevent it from flying into platforms.

## Bassball Polish

The bassball game mode received additional work with goal improvements, lighting adjustments, and level-specific tweaks to make the sport mode more readable and playable in a loud convention environment.

## Cabinet and UI

The skybox was replaced with a video texture to match the menu background treatment. The cabinet ritual system -- previously removed after MAGFest 2024 -- was re-added for the convention setting, providing controlled player slot assignment on the physical cabinet. Player join behavior was changed so that players join when a button is pressed rather than automatically on controller connection, preventing phantom joins from bumped controllers.

Menu sound effects were added for navigation feedback. Game mode selection received icon images for each mode and player count constraints with validation UI to prevent starting modes with incompatible player counts. Fish descriptions were written for the selection screen but left unused. The level stats UI was improved for better post-round readability. Win quips were re-added to the match end sequence. The announcer volume was increased to cut through convention floor noise.

For the event itself, the game was locked to deathmatch-only mode, restricting the game mode selector to the most crowd-friendly option. Mini gunfish parameters were tweaked for balance.

## Summary

Milestone 13 brought Gunfish back from a seven-month hiatus and delivered the game to its second MAGFest appearance. Seventy-eight commits added two new fish, two new levels, a telemetry system, a terrain art pipeline, and dozens of gameplay and UI improvements. The January sprint alone -- seventy-two commits in thirty days -- matched the intensity of the original MAGFest preparation, demonstrating that the project could survive an extended pause and return to full production velocity when the next deadline appeared.
