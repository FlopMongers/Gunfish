# Menu UI Design System — Design

## Problem

The main menu (Splash, GameModeSelect, FishSelect pages) currently uses the "Dark UI" free asset pack (`Assets/Dark UI/Free/`), a generic dark-fantasy-RPG skin, applied inconsistently:

- Hand-painted 9-slice PNGs pixelate at scale; panel background and border sprites (e.g. `InnerPanel` and its child `Outline` in `GameModeSelectPanel`) are separate pieces that drift out of alignment.
- No consistent spacing/margin scale — title position and title-to-content gap vary page to page; `FishSelectPanel` instances sit with no gap between them at all.
- Two unrelated font families in use (Cardo, a serif, for some titles; Oswald, a sans-serif, elsewhere) with no consistent pairing rule.
- UI elements (e.g. the Game Mode Select "Button Medium Round") are scaled ad hoc, producing visibly distorted art.

The Dark UI pack's dark-fantasy identity also doesn't match the game's actual brand palette (Deep Sea Blue, Aquamarine, Coral Orange, Sun Yellow, Neon Green, Bubblegum Pink, Bright Red — all bright, aquatic/arcade), documented in `CLAUDE.md`.

## Scope

**In scope:** the menu-page flow only — `SplashMenuPage`, `GameModeSelectMenuPage`, `FishSelectMenuPage`, and the shared page-title treatment (`MainMenu.cs` orchestrates transitions between these three).

**Out of scope (deferred to a follow-up pass):** in-match UI — `HealthUI`, `MatchUI`, `PauseManager`, `StatsUI`. The design system built here (tokens, `UIPanel`/`UIButton` components, conversion tooling) is intended to be reused for that pass, not rebuilt.

**Explicitly not addressed by this spec:** exact color hex values beyond deriving a neutral surface scale from the existing brand colors — the user expects to readjust colors later; this spec establishes the *system* (tokens + components), not final art direction sign-off.

## Direction

Replace the Dark UI sprite-based skin with a flat, procedurally-drawn UI system:

- Panels and buttons are rounded rectangles drawn by a single unlit shader implementing a rounded-box SDF (signed distance field), rather than hand-painted sprites. This is resolution-independent (no pixelation at any scale) and draws fill + border as one shape, so they cannot visually drift apart the way two separate sprites can.
- Typography consolidates to **Oswald only** (already imported and partially in use), using weight for hierarchy instead of switching families. Cardo is dropped from menu UI.
- Spacing consolidates to a fixed token scale applied via `HorizontalLayoutGroup`/`VerticalLayoutGroup` and fixed anchor margins, replacing hand-tuned per-instance anchor positions.

### Why an SDF shader over sprites, and why it's not a performance risk

A rounded-box SDF is a closed-form distance formula (`abs`/`max`/`min`/`length`, no loops or branching) plus one `smoothstep` for anti-aliased edges and a color lerp for fill vs. border — cheaper per-pixel than sampling and alpha-blending a filtered PNG, and it only costs pixels actually covered by each panel/button (a small fraction of the frame, only on menu screens with no gameplay/physics/rendering competing for budget). Real-time blur (e.g. soft drop shadows) is the one genuinely expensive technique in this space and is avoided — if a shadow is wanted, it's faked as a second, larger copy of the same SDF shape with a wider soft edge, not a blur pass.

All panels/buttons share **one Material asset**, so UGUI batches them into the same draw call, same as the current sprite atlas does — but this only holds if per-instance variation (per-player border color, differing corner radius/border width) is encoded as per-vertex data rather than as separate Material property values. Fill/border *color* already rides for free on UGUI's existing per-vertex tint (the same mechanism that lets many differently-colored `Image`s share the Default UI Material today). Corner radius and border width need to be encoded into a spare UV channel (a standard SDF-UI technique) rather than set per-Material — creating a distinct Material per button/panel instance would silently break the batching this design relies on, so the shader/component pair needs to support this from the start, not as a later optimization.

## Design tokens

**Spacing scale** (8px base unit):

| Token | Value | Use |
|---|---|---|
| xs | 4 | icon-to-label gaps |
| sm | 8 | tight internal gaps |
| md | 16 | internal panel padding; gap between related controls |
| lg | 24 | title-to-content gap |
| xl | 32 | gap between major sections/panels |
| xxl | 48 | primary-action gaps on top-level pages (e.g. Splash) |
| xxxl | 64 | outer page margins |

**Type scale** (Oswald only):

| Role | Weight | Size | Use |
|---|---|---|---|
| Page Title | Bold | 64 | page titles ("Select Mode", "Select Fish") |
| Section Header | SemiBold | 32 | sub-headers within a page |
| Body | Regular | 24 | descriptions, fish bio text |
| Button/Label | Medium | 20 | button captions, hint text |

Oswald Bold and Regular SDF font assets already exist at `Assets/Resources/Fonts/Oswald/`. SemiBold and Medium SDF variants need generating via TMP's Font Asset Creator — an in-Editor step, not scriptable.

**Color system:** the existing brand palette (Deep Sea Blue, Aquamarine, Coral Orange, Sun Yellow, Neon Green, Bubblegum Pink, Bright Red) remains the accent palette — reserved for borders/selection state, per-player color coding, and CTA buttons, never as large fill areas. A neutral surface scale is derived from Deep Sea Blue for backgrounds:

| Token | Use |
|---|---|
| Surface-900 | page background |
| Surface-700 | panel fill |
| Surface-500 | inset/recessed elements (e.g. unselected fish slots) |

Exact hex values are deferred — the user may readjust these later; this spec fixes the token *names and roles*, not final values.

## Components

**`UIPanel`** — a `MonoBehaviour` driving the rounded-box SDF shader/material. Exposed params: fill color, border color, border width, corner radius, optional soft-edge "shadow" copy. Replaces the current two-sprite panel+outline setup.

**`UIButton`** — same shape as `UIPanel`, driven by `UnityEngine.UI.Selectable`'s existing Normal/Highlighted/Pressed/Disabled color-tint transition (no sprite swapping needed). Label always uses the Button/Label type token with consistent padding from the spacing scale.

Both read from the same token values by construction, so no per-instance scaling drift (the "Button Medium Round" problem) is possible.

## Implementation approach

- The shader (`.shader`) and `UIPanel`/`UIButton` components are plain code/text files, authored directly.
- `FishSelectPanel.prefab` and `GameModeSelectPanel.prefab` (`Assets/Resources/Prefabs/UI/Menu/`) are real prefab assets; per `CLAUDE.md`, prefab files are not hand-edited as YAML. Following the existing precedent of `GunfishPrefabBaker.cs` (an Editor-time static class that builds/modifies prefabs via Unity's `PrefabUtility` APIs and saves them back out), an Editor utility applies the new components and layout groups programmatically:
  1. Load each menu prefab via `PrefabUtility.LoadPrefabContents`.
  2. Swap Dark UI `Image` sprite references for `UIPanel`/`UIButton` + the shared material.
  3. Add/configure `HorizontalLayoutGroup`/`VerticalLayoutGroup` per the breakdown below, using token spacing values.
  4. Save via `PrefabUtility.SaveAsPrefabAsset`.
- This is a single utility scoped to this design system's rollout (not a generalized batch-conversion tool for arbitrary future components) — reused as-is for the deferred HUD/pause/stats pass rather than rebuilt.
- Visual judgment calls (does a radius/spacing/shadow value actually look right) cannot be verified by running Unity directly. These go through a review loop: apply a pass, user opens the Editor and shares a screenshot, tokens/tool logic get adjusted based on what's visible. Anchor/spacing values are not hand-tuned by guessing without a screenshot to verify against.
- All changes go through git, so a pass that looks wrong can be reverted like any other commit.

## Per-page application

**Splash**: Page Title token, centered. Primary action button(s) below at `xxl` (48) spacing.

**GameModeSelect**: Page Title top; `lg` (24) gap to the `GameModeSelectPanel` (now a `UIPanel` background). Inside the panel, image+name in a `VerticalLayoutGroup` (`md`/16 spacing). Left/right arrows and confirm/cancel hints remain fixed-anchor elements at `md` (16) margin from the panel edge — they're positioned relative to the panel, not part of a linear stack. Buttons become `UIButton`.

**FishSelect**: Page Title top; `lg` (24) gap to the row of `FishSelectPanel`s, which sit in a `HorizontalLayoutGroup` with `xl` (32) spacing (this is what fixes the current zero-spacing problem). Inside each panel: fish image → description → hints in a `VerticalLayoutGroup` (`md`/16 spacing). Per-player border color (currently `FishSelectPanel.SetColor`) tints the `UIPanel` border directly instead of a separate outline sprite.

**Page-level title-to-content pattern** (applies to all three pages): title + content wrapped in one top-level `VerticalLayoutGroup` with `lg` (24) spacing, so the title-to-content gap is enforced structurally instead of a hand-tuned anchor per page.

## Follow-up (not this pass)

- Applying this same token/component system to in-match UI (`HealthUI`, `MatchUI`, `PauseManager`, `StatsUI`).
- Finalizing exact neutral surface and accent hex values (user intends to revisit).
