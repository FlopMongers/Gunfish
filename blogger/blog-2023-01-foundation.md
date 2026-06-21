# Milestone 1: Project Foundation

**January 12--22, 2023 | 19 commits**

The Gunfish project began in mid-January 2023 with an intensive burst of foundational work. Over the course of ten days and nineteen commits, the FlopMongers established the core repository structure, prototyped the central physics mechanic, and laid the groundwork for the systems that would support the rest of development. This post covers what was built during that initial sprint.

## Repository Setup and Cleanup

The first order of business was standing up a clean Unity project and establishing version control conventions. Initial commits handled project scaffolding, folder structure, and the removal of placeholder or auto-generated files that ship with a fresh Unity project. The goal was a minimal, organized starting point that the team could build on without tripping over default clutter.

## Hinge-Joint Fish Physics Prototype

The defining technical decision of this milestone was the proof-of-concept for floppy, segmented fish bodies using Unity's `HingeJoint2D` component. Each fish is constructed as a chain of `Rigidbody2D` segments connected end-to-end by hinge joints. This gives the fish a ragdoll-like quality where the body bends, swings, and reacts to forces in a physically plausible (and deliberately exaggerated) way. The prototype validated that this approach could produce the kind of chaotic, elastic movement the game design called for.

The fish body chain is rendered using a `LineRenderer` that traces through the segment positions each frame, producing a smooth visual outline over the underlying rigid body segments. This decouples the visual shape of the fish from the discrete physics objects that drive its motion.

## GunfishGenerator and Procedural Fish Construction

Rather than hand-placing segments and joints per fish prefab, a `GunfishGenerator` system was built to procedurally construct fish bodies at runtime. Given a set of parameters, the generator creates the appropriate number of segments, attaches `CircleCollider2D` components to each, and wires them together with hinge joints. This made it straightforward to experiment with different fish proportions and segment counts without manually rebuilding prefabs.

## ScriptableObject Data Model

Fish definitions are stored as `GunfishData` ScriptableObjects. Each asset holds the fish's physical dimensions, segment count, a width curve that controls how thick each segment is along the body, and physics parameters including mass, angular damping, and joint frequency. This data-driven approach means new fish can be authored by creating a new ScriptableObject asset and filling in values, rather than duplicating and modifying prefabs.

## Custom Gunfish Shader

A custom shader was written to handle sprite rendering on the fish segments. The shader inverts the Y axis to correct for the orientation mismatch that arises when sprites are mapped onto the physics-driven segment chain. Without this correction, fish sprites render upside-down or mirrored depending on the segment's rotation. The shader ensures consistent visual output regardless of how the physics simulation orients each piece.

## Unity Input System Import

The Unity New Input System package was imported during this milestone. This replaced the legacy `Input` class with the action-based input system that supports multiple devices, rebinding, and the kind of per-player input routing that a local multiplayer game requires. No extensive input configuration was done at this stage -- the package was brought in and made available for later work.

## Fish Movement and GunfishRigidbody

Basic movement was implemented through a `GunfishRigidbody` component that acts as a physics wrapper around the multi-segment fish body. Rather than applying forces to a single rigidbody, `GunfishRigidbody` coordinates force application across the segment chain so the fish moves as a coherent unit while still exhibiting the floppy joint-driven behavior. Initial movement forces -- swimming thrust and rotation -- were tuned to feel responsive without fighting the joint physics.

## Scene and Level Manager Scaffolding

Initial versions of `LevelManager` and `SceneManager` were set up to handle scene loading and level lifecycle. These were skeletal implementations at this stage, providing the basic hooks for loading a level, tracking its state, and transitioning between scenes. The intent was to have the infrastructure in place early so that gameplay code could be built on top of it rather than retrofitted later.

## Summary

In ten days, the project went from an empty repository to a working prototype of a physics-based fish that flops, moves, and renders correctly in a Unity scene. The architectural choices made here -- hinge-joint segment chains, procedural generation, ScriptableObject data, and the new input system -- defined the technical direction for the months of development that followed.
