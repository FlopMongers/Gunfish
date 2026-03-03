# Milestone 10: New Fish & Features

**December 6--31, 2023 | 36 commits**

December was a content-heavy month. With the fish-gun split architecture from Milestone 9 in place, the team could add new fish and weapons more efficiently. Three new playable characters were introduced alongside new arenas, a powerup system prototype, and a round of systems work on ground detection, music, and UI.

## New Fish: Swordfish

The Swordfish (PR #45) was the first melee-focused character. Instead of a projectile weapon, it carries a Sword that deals damage through direct contact. Hit detection uses a `CompositeCollisionDetector` that aggregates collision events across the sword's collider geometry. The Swordfish has charge and dash force multipliers that reward aggressive movement -- building speed before impact increases damage output. This created a fundamentally different playstyle from the existing gun-based fish, requiring players to close distance rather than maintain range.

## New Fish: Flounder and Minigun Fish

The Flounder was paired with the Blunderbuss, a shotgun-type weapon that fires a spread of projectiles at close range. Its wide spread pattern complements the Flounder's flat body profile, making it effective in tight corridors.

The Minigun fish introduced sustained fire as a weapon archetype. This required the `AutomaticGun` subclass, which fires continuously while the input button is held rather than requiring per-shot presses. The `AutomaticGun` class handles fire rate timing and ammunition tracking internally, extending the base gun class with a held-input firing loop.

## Sharkmode Prototype

Sharkmode was prototyped as a temporary powerup effect. When active, it doubles the fish's underwater force and velocity parameters, makes the fish deal contact damage to other fish on collision, and triggers a dedicated music track through the `SharkmodeManager`. The effect is time-limited and visually distinct. This was the first powerup with a dedicated manager and music integration, establishing a pattern for how powerups would interact with the audio and game state systems.

## Ground Detection Rework

PR #37 replaced the existing ground detection system. The new `GroundDetector` uses raycasts rather than relying on collision callbacks. This improved reliability in cases where fish segments were resting on geometry edges or thin platforms that sometimes failed to generate consistent collision events. Accurate ground detection matters because fish movement behavior switches between ground flopping and aerial physics based on contact state.

## Fish Select Menu Overhaul

The character selection UI received a significant revision. The updated menu improved layout, navigation, and visual feedback for each player's selection state. Fish selection triggers a quip through the `MarqueeManager` (using `QuipType.FishSelection`), connecting the UI flow to the announcer system.

## New Levels

Three new arenas were added. Valley is a mountainous arena with elevation changes that create natural chokepoints. Pipes features tight corridors with broken pipe hazards and kill boxes that punish careless movement. Great Bay introduced a water current system that applies directional forces to fish within water zones, adding an environmental movement modifier that players must account for.

The existing Barrel level was also updated with dynamic physics objects -- crates and barrels that react to weapon fire and fish collisions, adding destructible clutter to the arena.

## Music Manager Rework

The `MusicManager` crossfading system was reworked. The dual-`AudioSource` architecture that handles fade transitions between tracks was refactored, breaking and then fixing the crossfade behavior in the process. The end result was more reliable track transitions, particularly when rapid scene changes or game state transitions triggered multiple crossfade requests in quick succession.

## Editor Scene Swapper

An editor-only tool was added for quick scene switching during development. The keyboard shortcuts Ctrl+T, Ctrl+Shift+T, and Ctrl+Alt+T cycle through scenes in the build list, allowing developers to jump between levels without navigating the Unity scene browser. This was a quality-of-life improvement for a project with a growing number of scene files.

## Team Deathmatch Statistics and Match Logic

Post-match stat tracking was added for team deathmatch mode, recording per-player and per-team performance data. `MatchManager` logic was updated to improve round and level progression -- how the game cycles through levels across a multi-round match and determines when to advance or end the session.

## FX, Balance, and Polish

An FX pass addressed audio panning (positioning sound effects in stereo space relative to the action) and replaced placeholder splash effects. An offscreen tracker system was added that renders arrow indicators pointing toward fish that have moved outside the camera bounds, preventing players from losing track of their character. An underwater rotation fix corrected how torque was applied from the fish head segment when submerged, which had been causing unintended spinning behavior. A balance pass tuned fish and level parameters across the roster.

## Drunken Sailor Logic

Work began on a music and audio logic system referred to internally as "Drunken Sailor." This system manages contextual audio playback based on game state, connecting gameplay events to musical cues beyond the basic menu/gameplay track switching.

## Summary

Milestone 10 nearly doubled the playable content. Three new fish with distinct weapon archetypes, three new arenas with unique hazards, and the sharkmode powerup prototype expanded the game significantly. The ground detection and music manager reworks addressed reliability issues that would have compounded under the heavier testing load expected as the team prepared for the first public showing.