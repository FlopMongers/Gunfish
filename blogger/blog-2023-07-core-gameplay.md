# Milestone 5: Core Gameplay Loop

**July 3--27, 2023 | 30 commits**

After months of building out fish physics, weapons, and level infrastructure, Milestone 5 brought the pieces together into something resembling a game. Over thirty commits across July 2023, the FlopMongers implemented the core combat loop -- shooting, hit detection, death, and scoring -- alongside the supporting UI, audio, and input systems needed to run a multiplayer session from start to finish.

## Shooting and Hit Detection

The shooting mechanics were implemented in full during this milestone. Guns fire bullet projectiles with physics-driven trajectories, and each shot applies kickback force to the fish body, pushing the shooter backward through the hinge-joint segment chain. This creates a risk-reward dynamic where firing a weapon also disrupts the shooter's position and momentum.

Fish-on-fish hit detection was wired up so that bullets and collisions between fish bodies register as damage events. This was the first time fish could actually hurt each other, turning the physics sandbox into an actual combat system.

## Fish Selection and Deathmatch UI

A `FishSelect` UI system was built to handle per-player fish selection. Each connected player gets a selection panel where they can browse and lock in a fish before a match begins. This feeds into the player spawning pipeline that constructs the chosen fish at round start.

The `DeathMatchUI` skeleton was assembled to serve as the in-match HUD, displaying stocks and lives for each player. Health bars received their first implementation pass as UI indicators positioned near each fish, giving players a read on remaining health during combat.

## Marquee Text and Announcer System

A marquee text system was implemented with support for action callbacks. Scrolling text overlays display announcer messages during gameplay events -- kills, round transitions, and other key moments. The callback system allows gameplay logic to trigger after specific text events finish displaying, keeping announcements synchronized with game state changes.

## Camera Feedback

Camera shake and freeze frame effects were added to kill events. When a fish is eliminated, the camera shakes briefly and the game hitches for a few frames, selling the impact of the kill. These are standard fighting game techniques for communicating significant gameplay moments through screen feedback.

## Music Manager

A `MusicManager` was implemented as a `PersistentSingleton` that survives scene transitions. It handles crossfading between menu and gameplay music tracks using a dual `AudioSource` architecture -- one source fades out while the other fades in, allowing smooth transitions without audio gaps. Pause functionality was also added during this milestone, integrated with Unity's `AudioMixer` to mute or duck audio when the game is paused.

## Effect Framework

A base `Effect` class was introduced as a framework for modifying fish behavior in response to environmental conditions. Effects are subtypes that attach to fish and alter their physics or capabilities. The first concrete implementation was a sand flop modification that changes how fish move when on sand surfaces, but the architecture was designed to support arbitrary behavior modifications from level hazards, power-ups, or status conditions.

## Controller Support and Input

Multiple gamepad support was brought online using Unity's Input System package. The implementation handles per-player device assignment so that each connected controller maps to a distinct player slot. This was essential infrastructure for local multiplayer, where up to four players need simultaneous, independent input.

## Underwater Movement

Different physics behavior was implemented for fish inside water zones. When a fish enters a water volume, its velocity limits, force multipliers, and movement characteristics change to simulate underwater movement. This creates a distinct feel between flopping on land and swimming through water, adding environmental variety to combat arenas.

## Miscellaneous

A physics-driven beach ball was added as a level object -- a dynamic body that fish can knock around arenas. A Python socket connection test was also committed as groundwork for future Arduino-based arcade cabinet integration, establishing that the game could communicate with external hardware over a network socket.

## Summary

Milestone 5 transformed Gunfish from a physics prototype into a playable combat game. Players could now select fish, fight with guns that have real kickback and damage, see their health and stocks on screen, hear music that transitions between game states, and feel kills through camera feedback. The remaining work was filling out content and polish, but the fundamental gameplay loop was operational.
