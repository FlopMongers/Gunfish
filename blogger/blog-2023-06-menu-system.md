# Milestone 4: Menu System

**May 2 -- June 11, 2023 | 6 commits**

The fourth milestone was a quieter period for the Gunfish project. Six commits across roughly six weeks reflect a deliberate focus on getting the menu flow right before building out additional gameplay features. The work centered on the game mode selection screen, the fish selection screen, and a restructuring of the menu architecture into a cleaner page-based state machine.

## Page-Based Menu Architecture

The menu system was refactored into a page-based state machine during this milestone. `MainMenu` manages a stack of `MenuPage` subclasses, where each page is responsible for its own input handling, visual layout, and transitions to other pages. Pushing a new page onto the stack displays it and suspends the previous page; popping returns to the prior page. This pattern is well-suited to the kind of linear navigation flow that Gunfish requires (Main Menu to Mode Select to Fish Select to Gameplay) while also supporting back-navigation without special-casing each transition.

Each `MenuPage` subclass handles its own lifecycle: initialization when pushed, cleanup when popped, and per-frame updates while active. Input is routed only to the topmost page, preventing background pages from responding to button presses. This eliminated a class of bugs where menu inputs would bleed through to inactive screens.

## GameModeSelectMenuPage

The `GameModeSelectMenuPage` was implemented as the first fully functional menu page under the new architecture. This screen presents the available game modes to players and handles selection logic. It reads from the `GameModeDetails` ScriptableObjects introduced in Milestone 3 to dynamically populate the list of available modes. When a mode is submitted, the page triggers the transition to fish selection, passing the chosen mode configuration forward through the menu stack.

The submit logic includes validation to ensure a mode is actually selected before allowing progression, and the page supports both gamepad and keyboard navigation for scrolling through available options.

## FishSelectMenuPage

Initial work began on the `FishSelectMenuPage`, which handles per-player fish selection. In a local multiplayer game, fish selection is more involved than a typical character select screen because multiple players need to make independent choices simultaneously on the same display. Each player's input device must map to their own selection cursor, and the UI must clearly indicate which player is choosing which fish.

The implementation at this stage was foundational rather than complete. The page structure, input routing per player, and basic layout were established, but visual polish and the full roster of selectable fish were deferred to later milestones. The important deliverable was proving that the per-player input isolation worked correctly within the menu page framework.

## PlayerManager Relocation

The `PlayerManager` class was relocated within the project's folder structure to better align with the emerging architecture. As the separation between menu-phase systems and gameplay-phase systems became clearer, it made sense to move `PlayerManager` to a location that reflected its role as a bridge between the two. This was a housekeeping change rather than a functional one, but keeping the project organized as it grew helped the team navigate the codebase efficiently.

## Menu Restructuring

Beyond the page-based refactor, additional restructuring work was done on the menu system's internal organization. This included consolidating menu-related scripts, cleaning up references between menu components, and ensuring that the scene hierarchy in the Main Menu scene matched the new code architecture. These changes reduced coupling between menu pages and made it easier to add new pages in the future without modifying existing ones.

## Development Pace

The reduced commit frequency during this period -- six commits over six weeks compared to nineteen commits in ten days during Milestone 1 -- reflects the nature of the work rather than reduced effort. Menu systems involve significant iteration on layout, input handling edge cases, and user flow that does not always map neatly to discrete commits. Getting the navigation model right at this stage avoided costly rework later when additional screens (stats, settings, level select) needed to slot into the same framework.

## Summary

Milestone 4 delivered the menu architecture that would carry the project through the rest of development. The page-based state machine, game mode selection, and initial fish selection screens established the pre-game flow that players would navigate before every match. With the menu foundation in place, subsequent milestones could focus on gameplay content -- new fish, new modes, new levels -- with confidence that the menu system could accommodate them.
