# Milestone 2: Game Loop Architecture

**February 4--26, 2023 | 8 commits**

With the fish physics prototype in place, February shifted focus to the systems needed to turn the prototype into a playable game. Across eight commits over three weeks, the FlopMongers built the core game loop architecture: match management, player representation, input routing, controller support, and the first menu scene. This milestone established the flow a player would follow from launching the game to fighting in an arena.

## MatchManager

The `MatchManager` base class was introduced as the central authority for gameplay sessions. It handles the lifecycle of a match: managing rounds, spawning fish into the level, tracking round outcomes, and driving level progression. The design is abstract enough to be subclassed by specific game modes (deathmatch, objective-based modes, etc.) while providing the shared logic that every mode needs -- spawn timing, round transitions, and win condition evaluation.

## Player Class

The `Player` class was built to represent a human player within the game. Each `Player` instance holds references to the player's active fish, their input device, and their current state (alive, dead, spectating). The class acts as the connective tissue between the input system and the gameplay objects -- it receives input events and routes them to the appropriate fish controller. Separating the player concept from the fish concept was a deliberate architectural choice, since fish are destroyed and respawned between rounds while the player persists across an entire session.

## PlayerManager and Device Management

`PlayerManager` handles player spawning and input device assignment. When a controller is connected and a player joins, `PlayerManager` creates a `Player` instance and binds it to the device. This system needed to support multiple gamepads connected simultaneously, which required careful handling of device enumeration and assignment through the Unity Input System's `PlayerInput` component. The manager ensures that each physical controller maps to exactly one player and that devices are correctly reassigned if a controller disconnects and reconnects.

## GunfishController

The `GunfishController` component serves as the interface between player input and fish movement. It translates input actions (stick direction, button presses) into forces and commands that the `GunfishRigidbody` system understands. This layer of indirection keeps the input system decoupled from the physics code -- the fish body does not need to know whether it is being driven by a gamepad, keyboard, or (hypothetically) AI.

## InputManager Defaults

Default input mappings were configured through Unity's Input System. This included setting up action maps for gameplay controls (movement, firing, aiming) and UI navigation. The defaults were established early to provide a consistent baseline for testing, with the understanding that rebinding support could be layered on later.

## Multi-Controller Testing

A significant portion of this milestone's effort went into verifying that multiple gamepads could connect and operate simultaneously without conflicts. Local multiplayer depends entirely on this working correctly, so the team tested with multiple physical controllers to confirm device assignment, input isolation (one player's stick input not leaking to another player's fish), and hot-plug behavior. This was validation work rather than feature work, but it was essential for confidence in the input architecture.

## Main Menu Scene and UI Assets

The main menu scene was created along with initial placeholder UI sprites. The menu at this stage was functional rather than polished -- enough to start the game, but not yet structured into the page-based system that would come later. Basic navigation allowed transitioning from the main menu into gameplay for testing purposes.

## Game Loop Design

The overall game loop flow was established during this period: Main Menu leads to Fish Select, which leads to Level Load, then Gameplay, then a Stats screen, then either the next level or a return to the main menu. Not all of these screens were fully implemented in February, but the flow was defined and the scene management infrastructure from Milestone 1 was extended to support it. This gave the team a clear roadmap for which UI screens and transitions needed to be built in subsequent milestones.

## Summary

February's work transformed the project from a physics toy into the skeleton of a game. Players could connect controllers, join a session, spawn into a level, and play through a basic match loop. The separation of concerns between `Player`, `MatchManager`, `GunfishController`, and `PlayerManager` created an architecture flexible enough to support the game modes and features that would follow.
