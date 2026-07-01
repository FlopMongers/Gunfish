# Milestone 3: Multi-Controller and Git LFS

**March 5 -- April 16, 2023 | 14 commits**

The third milestone spanned about six weeks and covered a mix of gameplay features, repository infrastructure, and early content work. With fourteen commits, the FlopMongers introduced the first concrete game mode, experimented with physics tuning tools, enabled Git LFS for binary asset management, and iterated on the menu and data architecture. This was also a period of establishing conventions and tooling that would support the team's workflow going forward.

## DeathMatchManager

The first game mode to receive its own dedicated manager was Deathmatch. `DeathMatchManager` subclasses the base `MatchManager` and adds elimination-specific logic: tracking which players are still alive, determining when a round ends (last fish standing), awarding points, and advancing through a multi-round match. This was the simplest game mode conceptually, making it a natural first target. Having a concrete mode also provided a testable end-to-end loop -- players could now join, fight, be eliminated, see a round end, and start the next round.

## Auxiliary Gameplay Managers

Several supporting manager classes were added during this period to handle responsibilities that did not belong in the match manager itself. These auxiliary systems covered concerns like camera management, score tracking, and gameplay event coordination. Breaking these out into separate managers kept the `MatchManager` subclasses focused on mode-specific logic rather than accumulating unrelated responsibilities.

## BOING: Physics Tuning Test Level

A test level called BOING was created specifically for tuning the elastic bouncing behavior of the fish physics. The level features surfaces and geometry designed to produce exaggerated bounce interactions, making it easier to observe and adjust parameters like restitution, joint stiffness, and collision response. Test levels like this are a common gamedev practice -- purpose-built environments that isolate a specific system for rapid iteration without the noise of a full gameplay scenario.

## Git LFS (PR #11)

Git Large File Storage was enabled for the repository via pull request #11. Unity projects accumulate binary assets quickly -- sprite sheets, audio clips, textures, and other files that do not diff well and bloat repository history. LFS replaces these files with lightweight pointers in the Git history while storing the actual binary data on a separate server. This was a necessary infrastructure step as the team began adding art and audio assets in earnest. The `.gitattributes` file was configured to track common Unity binary formats.

## GameModeDetails ScriptableObjects

Game mode definitions were moved into `GameModeDetails` ScriptableObjects. Each asset describes a game mode's display name, description, associated match manager type, and configuration parameters. This data-driven approach mirrors the `GunfishData` pattern used for fish definitions -- new game modes can be authored as assets rather than requiring code changes. It also enabled the menu system to dynamically populate mode selection UI from the available ScriptableObject assets.

## Menu System Encapsulation

The menu management code was restructured to better encapsulate navigation state and page transitions. The earlier menu implementation from Milestone 2 was functional but loosely organized. This pass introduced clearer boundaries around menu pages and their lifecycle, laying groundwork for the page-based state machine architecture that would be fully realized in the next milestone.

## Addressables: Tried and Removed

The Unity Addressables package was imported, evaluated, and ultimately removed. Addressables provide a system for loading assets by address rather than direct reference, which can be useful for managing memory and download size in larger projects. For Gunfish's scope -- a local multiplayer game with all assets available at build time -- the added complexity was not justified. The team reverted to standard `Resources` folder loading and direct asset references.

## Fish Concept Art

Early concept art for the FlyFish character was produced during this period. While not a code deliverable, character design work was happening in parallel with engineering. The FlyFish design explored how to visually communicate a fish-with-weapon concept while keeping the silhouette readable at the game's typical camera distance.

## Gunfish Spawning on Debug Start

A quality-of-life improvement was added to allow fish to spawn automatically when entering play mode from a gameplay scene in the Unity editor. Previously, testing required navigating through the menu flow every time. The debug spawn shortcut saved significant iteration time during development.

## Summary

This milestone was as much about infrastructure and process as it was about features. Enabling Git LFS, evaluating and discarding Addressables, and restructuring the menu system were investments in the project's long-term health. On the gameplay side, `DeathMatchManager` delivered the first fully playable game mode, and the BOING test level provided a dedicated environment for the physics tuning work that would continue throughout development.
