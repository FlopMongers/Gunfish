# Milestone 7: Water System

**September 4--28, 2023 | 16 commits**

September 2023 was a focused month dedicated primarily to building a water simulation system from scratch. With only sixteen commits over four weeks, this milestone traded volume for depth -- the water system touched rendering, physics, procedural mesh generation, and particle effects, and required all of those pieces to work together coherently. A collision damage system and several housekeeping tasks rounded out the month.

## Water Shader and Surface Generation

The water system began with a custom shader for visual rendering. The shader handles the translucent, refractive appearance of water volumes and integrates with the surface mesh to produce a cohesive look.

The water surface itself is a procedurally generated mesh built at runtime by a surface generator. Rather than using a static sprite or tiled texture, the system constructs a mesh whose top edge is defined by an array of `WaterSurfaceNode` objects. Each node represents a discrete point along the water's surface and can move independently on the vertical axis, allowing the surface to deform dynamically in response to gameplay.

## Spring-Based Wave Propagation

Each `WaterSurfaceNode` implements spring physics. When a node is displaced from its rest position -- by a fish entering the water, an explosion, or any other force -- it oscillates back toward equilibrium following spring dynamics (displacement, velocity, damping). Critically, each node also perturbs its immediate neighbors on every physics step, transferring a portion of its displacement outward. This neighbor-to-neighbor propagation produces traveling waves along the water surface without requiring a fluid simulation. The result is visually convincing wave behavior from a computationally inexpensive system built on an array of coupled springs.

## Buoyancy

A buoyancy system was implemented to apply upward forces to fish segments submerged in water. Because each fish is a chain of independent `Rigidbody2D` segments, buoyancy is calculated per-segment based on how far below the water surface that segment sits. This means a fish partially in water experiences differential forces along its body -- segments deeper in the water get more upward force -- which interacts with the hinge joint chain to produce natural-looking bobbing and tilting at the surface.

## Underwater Physics

Fish movement parameters change when underwater. Velocity limits, force multipliers, and torque values are modified so that swimming feels distinct from flopping on land. These parameters were tuned separately from the land movement values established in earlier milestones, giving underwater combat a different pace and feel. The transition between land and water physics happens when fish segments cross the surface boundary defined by the `WaterSurfaceNode` array.

## Splash Effects and Ripples

Visual feedback for water interaction was added through splash particle effects and surface ripples. Splash particles are scaled by the impact force of the object entering the water -- a fish cannon-balling in at high velocity produces a larger splash than one drifting gently off a ledge. Ripples propagate along the surface from the point of entry, driven by the same spring-node system that handles waves. Together, these effects communicate the physics of water interaction to players.

## Collision Damage

A first pass at collision damage was implemented through an `OomphCalculator` system. This applies damage to fish based on the physics of collisions -- velocity, mass, and impact angle factor into how much damage a collision deals. This extended the damage model beyond bullets and explosions to include environmental and fish-to-fish physics collisions, rewarding aggressive movement and punishing careless landings.

## Sea Urchin

The sea urchin level object received its initial implementation and a subsequent rework within this milestone. Sea urchins are spiky contact hazards that deal damage to fish on touch, functioning as static or semi-static environmental dangers placed in arenas.

## Housekeeping

DOTween, a tweening animation library, was imported for use in UI animations and effect sequences. The sprites folder was reorganized for better asset management, and the FunkyCode Smart Lighting 2D plugin was relocated from its default import location to a dedicated Plugins folder, following the project's emerging convention for third-party code.

## Summary

Milestone 7 added a complete water simulation built on procedural mesh generation, spring-coupled surface nodes, per-segment buoyancy, and tuned underwater physics. The system is lightweight enough to run alongside the existing combat simulation while producing visually convincing wave behavior, splashes, and ripples. Combined with the new collision damage model, this milestone significantly expanded the environmental variety available to level designers.
