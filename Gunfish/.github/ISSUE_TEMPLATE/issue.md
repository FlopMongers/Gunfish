---
name: Issue
about: Feature or bug for Gunfish
title: '[Area] Short description'
labels: ''
assignees: ''
---

## Background

<!-- Context that helps someone (or future-you) understand why this matters.

     Note which subsystem(s) this touches:
     [ ] Player / Gunfish / Gun (Player/, GunfishRigidbody, weapon subclasses)
     [ ] Match logic (MatchManager subclass, win conditions, score)
     [ ] Managers (GameManager, PlayerManager, GameModeManager, LevelManager)
     [ ] Level objects / hazards (LevelObjects/, GameLogic/)
     [ ] UI / menus (UI/, GameUIManager)
     [ ] Audio (MusicManager, Audio/)
     [ ] ScriptableObject data (GameMode, GunfishData, SceneList)
     [ ] Build / tooling
-->

## Problem / Requirement

<!-- For a bug: what is broken, when does it happen, what is the expected vs actual behaviour?
     For a feature: what capability is missing and why is it needed?

     If it's a bug, include:
     - Steps to reproduce (game mode, level, number of players)
     - Expected behaviour
     - Actual behaviour
     - Platform(s) affected: [ ] macOS Editor  [ ] macOS Build  [ ] Windows Build
-->

## Proposed Solution

<!-- Your intended approach. Be specific enough to act on.

     Key scripts / assets involved:
     -

     If this touches input, confirm:
     [ ] Using New Input System action assets (PlayerInput) — not Input.GetKey

     If this touches data / config, confirm:
     [ ] Change is in a ScriptableObject asset, not hardcoded in a script

     If this touches singletons, confirm:
     [ ] Cross-scene: inherits PersistentSingleton<T>
     [ ] Scene-scoped: inherits Singleton<T>
-->

## Testing Plan

<!-- Check every layer that applies to this change. -->

- [ ] **Editor Play mode** — tested via ▶ in Unity Editor
- [ ] **Multiplayer** — tested with multiple controllers if player interaction is affected
- [ ] **All three game modes** — DeathMatch, Race, Bassball checked if match logic changed
- [ ] **macOS build** — File → Build Settings → Build and run
- [ ] **Visual** — art, animation, or camera change verified by eye

## Acceptance Criteria

<!-- Checkboxes that must all be ticked before this is considered done. -->

- [ ] 
- [ ] 
- [ ] 
