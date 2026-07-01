# Milestone 6: First Playable Demo

**August 3--30, 2023 | 55 commits**

August 2023 was the highest-volume month of development to date. Across fifty-five commits, the FlopMongers pushed Gunfish from a functional prototype to something that could be shown to people outside the team for the first time. This milestone touched nearly every system in the project -- weapons, levels, UI, audio, scene management, and external hardware integration -- and included a significant code restructuring pass.

## Weapons and Ammo

The gun system was expanded with shotgun and pistol weapon types, each with distinct projectile spread, fire rate, and damage characteristics. An ammo system was implemented so that guns have limited shots before requiring a reload or running dry, adding resource management to combat. A cannon prototype was also built as a directional launcher object -- a level fixture that fires fish or objects in a specified direction.

## Barrel Level and Level Objects

The Barrel level was completed as the first fully realized arena, featuring dynamic objects that react to physics and combat. Several new level object types were added: sea mines that explode on bump contact, crates that function as breakable containers dropping items when destroyed, and fish hooks that use line-based mechanics to catch and hold fish. Environmental kill boxes were implemented as death zones that instantly eliminate fish on contact, providing hard boundaries and hazard areas within arenas.

## UI and Scene Flow

The UI pipeline received substantial work. A loading screen with a countdown overlay (3-2-1-GO) was added to bridge the gap between fish selection and round start. A player loading screen was implemented to show readiness state. The level stats screen was built to display post-round statistics, giving players a results summary before returning to selection. Scene loading was reworked to use async loading with veil animations, eliminating hard cuts between scenes and providing visual continuity during transitions.

## Announcer and Audio

The marquee announcer system was extended with voice quips -- audio clips that play alongside text for kill events, fish selection confirmations, and countdown sequences. This gave the game a fighting-game-style commentary layer that reacts to what happens during play.

## Arduino Cabinet Integration

An `ArduinoManager` singleton was implemented to communicate with arcade cabinet hardware over serial connections. The system opens COM ports at 9600 baud and sends data to an Arduino microcontroller that drives LED strips mounted in the cabinet. LED brightness and color respond to audio loudness levels sampled from the game's output, creating a reactive lighting effect synchronized to gameplay audio. This was purpose-built for the physical arcade cabinet the team was constructing for public demos.

## Smart Lighting 2D

The FunkyCode Smart Lighting 2D plugin was integrated into the project, providing dynamic 2D lighting with shadow casting. This added atmospheric depth to arenas through point lights, shadow casters on level geometry, and ambient light control. A camera follow system was also implemented during this period to track active players and keep the action framed.

## Spawn Effects and Visual Feedback

Spawn FX were added to give fish a visual entrance when they appear in the arena at round start. Combined with the existing camera shake and freeze frame systems, this filled out the suite of feedback effects that communicate game events to players.

## Code Restructuring

PR #28 delivered a significant refactoring pass across the codebase alongside a gun system rework. This restructured how weapons, fish, and game state interacted, cleaning up the rapid prototyping debt that had accumulated over previous milestones. The refactor touched enough of the project to warrant its own pull request and represented a deliberate pause in feature work to improve code quality and maintainability.

## Water Experiments

Initial experiments with water physics were committed during this milestone. These were early attempts at implementing water volumes with buoyancy and surface behavior, predating the dedicated water system that would be built in September. The experiments established that water would need its own focused development pass rather than being bolted onto existing systems.

## First External Showing

This milestone marked the first time Gunfish was demonstrated to people outside the development team. The combination of working combat, a complete arena, audio feedback, and polished scene transitions made the game presentable enough for external feedback.

## Summary

Milestone 6 was a brute-force push to get Gunfish across the playability threshold. Fifty-five commits added weapons, arenas, UI flow, hardware integration, lighting, and a major refactor. The result was a game that could be handed to four players and produce a coherent, entertaining experience from fish selection through combat to results -- the definition of a first playable.
