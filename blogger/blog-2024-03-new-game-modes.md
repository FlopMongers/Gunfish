# Milestone 12: Post-MAGFest -- New Game Modes & Polish

**February 1 -- March 2, 2024 | 20 commits**

Coming off the MAGFest 2024 debut in January, the FlopMongers spent February 2024 expanding Gunfish with two new game modes, a complete menu system overhaul, and character art for the full roster. This milestone reflected a shift from event-readiness work to broader content and polish, addressing gaps identified during the public showing while pushing the game in new directions.

## Race Mode

A new Race game mode was implemented via a dedicated `RaceMatchManager`. The mode uses checkpoint-based progression through a level, where each player advances through a series of spatial triggers. The first fish to cross all checkpoints wins the race. The system tracks per-checkpoint completion times for each player and marks players who fail to finish within the time limit as DNF (did not finish). This added a non-combat game type to the roster, giving Gunfish a traversal-focused alternative to its fighting modes.

## Bassball Mode

Bassball, a team sport mode, was built using a `BassballMatchManager`. Two teams compete to knock a `BassballBall` -- a physics-driven ball object -- into the opposing team's `Goal`. Scoring is tracked per round, with the team reaching the target score first winning the match. The mode required new level objects (goals, ball) and team assignment logic layered on top of the existing match framework. A late-night commit message -- "I should sleep but bassball is in" -- timestamped at 2 AM documented the session that brought the mode online.

## Pause Menu and Audio Controls

A pause menu was added to gameplay, allowing players to halt the action mid-match. The pause screen includes volume sliders powered by a `VolumeController` component that interfaces with Unity's audio mixer system. Players can adjust master, music, and sound effect levels independently. This also involved removing a lowpass filter that had been applied to the master audio mixer, with the filter later re-exposed as an optional setting for use during specific game states.

## Fish Select UI and Menu System Overhaul

The fish selection screen was reworked with a new `FishSelectMenu` implementation. A bubble transition test scene was created to prototype visual transitions between menu states. Before proceeding with the full overhaul, the team preserved the legacy menu UI as a backup -- a precaution against the scope of the rebuild.

The complete new menu system was then built from scratch. Page transitions use DOTween for animated movement between menu screens, replacing the previous static panel switching. Menu icons, aspect ratio handling, and transition animations were all implemented as part of this pass, producing a more polished presentation layer for the game's front end.

## Character Art

Proper character artwork was created for the full fish roster: bassic fish, salmon, swordfish, flounder, flyfish, needlefish, and pufferfish. These replaced placeholder sprites that had been used through development and the MAGFest showing. Each fish received finished art for its base appearance, and several received companion gun sprites to match.

## Gameplay Fixes and Cleanup

Several gameplay bugs identified during or after MAGFest were addressed. Swordfish damage calculations were corrected. Status effect removal was fixed so that effects properly clean up when they expire or are overridden. FX spatial blend settings were adjusted to ensure particle and audio effects play with correct 2D/3D positioning relative to the camera.

The cabinet-only ritual system -- a controller ordering sequence used at events to assign player slots in a specific order -- was removed. This had been purpose-built for the MAGFest cabinet and was not needed for general play. Its removal simplified the player join flow.

## System Refactors

Two refactoring passes were completed during this milestone. The powerup system was cleaned up, reorganizing how powerups are defined, spawned, and applied to fish. The player layer system was also refactored, adjusting how player-associated objects are assigned to Unity physics layers for collision filtering.

## Performance and Package Updates

FX particle bursts were clamped to a maximum of eight particles per burst. This was a performance optimization to prevent particle system spikes during heavy combat, where multiple explosions or hit effects could fire simultaneously. Unity package dependencies were also updated during this period.

## Summary

Milestone 12 added two new game modes, rebuilt the entire menu system, replaced placeholder art with finished character sprites, and cleaned up gameplay bugs surfaced by the MAGFest debut. The twenty commits across February 2024 represented a productive post-event sprint. After this milestone, the project would enter a seven-month hiatus with no commits on main from April through September 2024.
