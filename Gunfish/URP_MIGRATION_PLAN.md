# Gunfish: Built-in RP → URP (2D Renderer) Migration Plan

## Context

Gunfish currently runs on Unity's Built-in Render Pipeline, which is deprecated. The user has already added `com.unity.render-pipelines.universal` (17.5.0) to the package manifest but hasn't configured anything yet — no URP Asset exists, and `GraphicsSettings`/`QualitySettings` still point at `{fileID: 0}` (no pipeline assigned).

Research corrected two assumptions from CLAUDE.md and my own earlier read of the project:
- **Actual Unity version is 6000.5.0f1 (Unity 6)**, not 2021.3.45f2 as CLAUDE.md states. This is a stale-doc issue worth fixing separately, and it's good news for the migration — Unity 6's URP is far more mature (Render Graph, better 2D lighting) than the 2021 LTS version.
- The project's 2D lighting plugin, **FunkyCode SmartLighting2D**, is used far more narrowly than its full feature set suggests: only simple point/lantern lights (`Light2D`) and shadow-casting occluders (`LightCollider2D`) across 45 files (25 prefabs, 20 scenes). Fog-of-war, room masking, translucency, day-shadows, and normal-mapped lit sprites are **not used anywhere** in the shipped game, and zero C# game code is coupled to the plugin's API. This means it maps cleanly onto URP's native `Light2D` (Point) + `ShadowCaster2D` — a like-for-like swap, not a redesign.

Other scope, confirmed by direct file reads:
- Two project-owned shaders (`Gunfish.shader` outline-pulse, `Water.shader` procedural waves) are simple Unlit CGPROGRAM passes with no lighting model — mechanical port to URP HLSL.
- Post-processing is PPv2 (3.5.4): 3 `PostProcessProfile` assets, one shared `GameCamera.prefab` rig (Main Camera + Cinemachine + a global `PostProcessVolume`) instanced in 21 scenes, plus 3 scenes with their own embedded copies. Zero C# scripts touch the PPv2 API — all effects are static/baked into profiles, which lowers migration risk.
- TextMeshPro already ships URP shader graph variants in the project (`TMP_SDF-URP Lit/Unlit.shadergraph`) — just needs assignment, no new import.
- No CLI/automated test suite exists (per CLAUDE.md) — every phase ends in manual Play-mode verification.
- CLAUDE.md forbids hand-editing `.meta`, `Packages/manifest.json`, `ProjectSettings/*.asset`, or prefab/scene YAML directly. The one sanctioned exception: a **C# Editor script** (`Assets/Scripts/Editor/`) that performs these mutations through Unity's own APIs at Editor-run time — this repo already has precedent for that pattern (`GunfishEditor.cs`, `SceneSelectorEditor.cs`, both plain `Editor` classes with `[MenuItem]` static methods). New migration tooling should follow the same convention.

**Decisions flagged for the user (not silently decided):**
1. `Basic.asset`'s **AutoExposure** has no 1:1 URP Volume equivalent. Recommend dropping it and compensating via Color Adjustments tuning by eye.
2. Recommend a single **Global Volume** GameObject per scene (mirrors the existing `isGlobal:1` PPv2 setup) over per-camera Volumes — simpler given `GameCamera.prefab` is instanced everywhere.

---

## Phase 0 — Safety net

- Create a dedicated branch, e.g. `git checkout -b urp-migration`, confirm `git status` is clean before starting (bulk scene/prefab edits are hard to review without a clean starting diff).

---

## Phase 1 — Pipeline bootstrap (manual Editor UI, user-driven)

Test on one throwaway scene (e.g. duplicate `Assets/Scenes/Templates/dm_sm_square.unity`) before touching real gameplay scenes.

1. `Assets > Create > Rendering > URP Asset (with 2D Renderer)` → creates a `UniversalRenderPipelineAsset` + `Renderer2DData`, pre-linked. Suggested path: `Assets/Settings/URP/Gunfish_URP_Asset.asset` + `Gunfish_Renderer2D.asset`.
2. `Edit > Project Settings > Graphics` → assign the new URP Asset as the Scriptable Render Pipeline.
3. `Edit > Project Settings > Quality` → assign the same URP Asset to all 6 quality levels (this is a small local-multiplayer game — one shared asset is fine to start, refine later if needed).
4. Confirm `Assets/UniversalRenderPipelineGlobalSettings.asset` (existing stub) gets populated automatically once step 2 completes.
5. Run `Window > Rendering > Render Pipeline Converter` (Material Upgrade, Built-in → URP 2D) — **only against the throwaway scene first**, verify sprites render with no magenta/missing-shader materials, then proceed scene-by-scene during rollout (Phase 3/4), not all at once.

**Claude Code can pre-stage:** a lightweight Editor sanity-check script (`Assets/Scripts/Editor/URPSetupValidator.cs`, `[MenuItem]`-driven) that reads back `GraphicsSettings.currentRenderPipeline` / `QualitySettings.renderPipeline` and logs pass/fail to the Console, so the user can confirm steps 1–4 landed correctly.

**Risk:** bulk-running the Converter across all scenes/prefabs in one pass with no diff review is the single highest-risk action here — sequence it scene-by-scene instead.

---

## Phase 2 — Shader porting (Claude Code writes these directly — plain text, not serialized assets)

### `Assets/Resources/Shaders/Gunfish.shader` (outline-pulse, LineRenderer material)
- `CGPROGRAM/ENDCG` → `HLSLPROGRAM/ENDHLSL`; `#include "UnityCG.cginc"` → `#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"`.
- `UnityObjectToClipPos(v.vertex)` → `TransformObjectToHClip(v.vertex.xyz)`.
- `sampler2D _MainTex` / `tex2D(...)` → `TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);` / `SAMPLE_TEXTURE2D(...)`.
- `fixed4`/`fixed2` → `half4`/`half2`. Add `"RenderPipeline"="UniversalPipeline"` to Tags.
- No lighting macros in the original — stays a pure unlit pass. `Player.cs:49`'s `material.SetColor("_OutlineColor", ...)` needs no changes.

### `Assets/Resources/Shaders/Water.shader` (procedural wave/tint, driven by `WaterMaterialInterface.cs`)
- Same mechanical transforms as above, plus `mul(unity_ObjectToWorld, v.vertex)` → `TransformObjectToWorld(v.vertex.xyz)`.
- **Flag, don't silently fix:** `_NodesX[1000]`, `_NodesY[1000]`, `_Coefficients[1000]` are large float-array properties set via `Material.SetFloatArray` (`WaterMaterialInterface.cs:91-93`). Put small scalar properties (`_Color`, `_NodeCount`, `_Degree`) inside `CBUFFER_START(UnityPerMaterial)/CBUFFER_END`, but leave the three large arrays outside the CBUFFER (matches how `SetFloatArray` already targets them) to avoid constant-buffer overflow. This opts the material out of SRP Batching, which is a non-issue given how few Water instances exist per scene — but it's a "verify visually, no automated test exists" item: after porting, load a scene with water and confirm ripples animate instead of freezing flat.
- No change needed in `WaterMaterialInterface.cs` — `SetFloatArray` API is pipeline-agnostic.

**Sequencing:** author anytime, but expect compile/resolve to only succeed once Phase 1's URP packages are active in Graphics Settings.

**Verification:** Play mode on a scene with the Gunfish outline and a scene with water (e.g. `sand_lot.unity`) — confirm outline pulse animates and water ripples/tints correctly.

---

## Phase 3 — Lighting migration (SmartLighting2D → native URP `Light2D`/`ShadowCaster2D`)

Highest file count (45: 25 prefabs + 20 scenes), most mechanically repetitive. Must go through a **custom Editor script** (sanctioned exception to the no-hand-edit-YAML rule), following the existing `Assets/Scripts/Editor/` convention.

### New file: `Assets/Scripts/Editor/URP2DLightingMigrator.cs`

Field mapping (verified directly against `Assets/Plugins/FunkyCode/SmartLighting2D/Components/Lightmap/Light2D.cs` and `.../LightCollider/LightCollider2D.cs`):

| FunkyCode `Light2D` | URP `Light2D` (Point) | Notes |
|---|---|---|
| `color` | `color` | direct copy |
| `size` | `pointLightOuterRadius` | direct copy |
| `coreSize` | `pointLightInnerRadius` | direct copy |
| `falloff` | `falloffIntensity` | scale may not match exactly — tune by eye |
| `lightStrength` | `intensity` | FunkyCode defaults this to 0 and drives visible brightness via `color` instead — don't blindly copy, verify visually per light |

| FunkyCode `LightCollider2D` | URP equivalent | Notes |
|---|---|---|
| `shadowType == SpritePhysicsShape` or `Collider2D`/`CompositeCollider2D` | add `ShadowCaster2D` | both systems derive shape from the same sprite physics shape / attached collider — near-direct swap for the common case (confirmed default is `SpritePhysicsShape`) |
| `shadowType == MeshRenderer`/`SkinnedMeshRenderer` | no direct equivalent | flag in script's log output for manual handling (likely rare — most of the 22 environment prefabs are sprite-based) |

`LightingManager2D` (child of `Assets/Resources/Prefabs/GameCamera.prefab`) has **no direct equivalent needed** — URP's 2D Renderer handles global light blending via the `Renderer2DData` asset's Light Blend Styles (configured once in Phase 1). Leave the `LightingManager2D` object in place but disabled until the whole migration is verified, then remove manually — don't auto-delete via script.

**Script behavior:**
1. Two menu commands: one for the currently open scene, one for a selected prefab asset (pilot one file at a time).
2. For each `FunkyCode.Light2D`/`LightCollider2D` found via `GetComponentsInChildren<T>(true)`, add the URP sibling component with mapped fields, but **do not remove the old component automatically** — disable it and log a before/after summary to the Console so the user can A/B compare.
3. Mark scene/prefab dirty (`EditorUtility.SetDirty` / `EditorSceneManager.MarkSceneDirty`) — don't auto-save; the user's own save is the review gate.
4. Separate menu command "Remove legacy SmartLighting2D components," run manually only after visual approval.

**Rollout order:**
1. Pilot on one lantern-style prefab (e.g. `ChainedLamp.prefab`) + one occluder-heavy prefab.
2. Pilot on one full scene (e.g. `sand_lot.unity` — confirmed to use both component types, or a Deathmatch scene like `acid_factory.unity`).
3. Visually verify lights/shadows in Play mode.
4. Roll out to the remaining ~23 prefabs and ~18 scenes.
5. Handle `GameCamera.prefab`'s `LightingManager2D` child last, after confirming URP's Light Blend Styles cover global lighting correctly — then remove it manually.

**Risk:** falloff/intensity scale mismatch is a "tune per light," not a guaranteed formula — call this out so the user doesn't assume copied numbers look identical without adjustment.

---

## Phase 4 — Post-processing migration (PPv2 → URP Volume)

### Convert 3 profiles (Claude Code can write a one-off Editor script to do this programmatically, reducing manual transcription of ~15 numeric values)
New file: `Assets/Scripts/Editor/PPv2ToURPVolumeConverter.cs` — reads each `PostProcessProfile`'s settings and constructs a matching `VolumeProfile` + overrides via `ScriptableObject.CreateInstance<VolumeProfile>()` + `AssetDatabase.CreateAsset`.

- `Assets/Scenes/Profiles/Basic.asset` → `Basic_URP.asset`: Vignette (0.131), Bloom (intensity 1, diffusion 4 → Bloom Scatter), Color Adjustments (saturation 33.9, contrast 42.8). **Drop AutoExposure** (flagged decision above), compensate via Color Adjustments exposure tuning.
- `Assets/Scenes/Profiles/MainMenu.asset` → `MainMenu_URP.asset`: Vignette (0.537), Lens Distortion (-49, verify sign convention), Color Adjustments (hue shift -15, contrast 55.2), Bloom (0.35 + dirt texture → URP Bloom's Dirt Texture/Dirt Intensity slot), Grain left off.
- `Assets/Scenes/Profiles/Skybox.asset` → `Skybox_URP.asset`: Depth of Field (Bokeh mode — closer to PPv2's physical-aperture model; focus distance 2.21 → Focus Distance, aperture 0.7 → Aperture), Color Adjustments (saturation -12.5).

### Rig update (manual, Editor UI — touches serialized prefab/scene state)
- `Assets/Resources/Prefabs/GameCamera.prefab`: remove `PostProcessLayer` from Main Camera (Unity auto-adds `UniversalAdditionalCameraData`); swap the child "Post-Process Volume" GameObject's `PostProcessVolume` for a native `Volume` (`isGlobal: true`, weight 1, Profile = `Basic_URP.asset`). This propagates to all 21 instancing scenes automatically.
- Confirm `Gunfish_Renderer2D.asset` (from Phase 1) has **Post-processing enabled** in its render features — easy-to-miss checkbox, required for Volumes to apply at all.
- Cinemachine rig is **unaffected** — no PPv2-Cinemachine adapter exists (`PostProcessLayer` sits directly on the physical Camera), so the 15 script files using `CinemachineImpulseSource` for hit-shake need no changes.
- 3 standalone scenes need the same swap individually (they don't inherit from `GameCamera.prefab`): `Assets/Scenes/MainMenu.unity` → `MainMenu_URP.asset`, `Assets/Scenes/Stats.unity`, `Assets/Scenes/Prototype/LevelStaging/ConveyorBelts.unity` (verify which profile each currently references).

**Verification:** Play mode on `MainMenu.unity`, `Stats.unity`, and one gameplay scene — confirm Bloom/Vignette/Color Grading look visually reasonable (not identical — PPv2 and URP math differ) and exposure isn't blown out now that AutoExposure is gone.

---

## Phase 5 — TextMeshPro shader assignment (low risk, quick)

`Edit > Project Settings > TextMesh Pro` (or the TMP Settings asset) → assign the already-present `TMP_SDF-URP Lit.shadergraph` / `TMP_SDF-URP Unlit.shadergraph` (in `Assets/Plugins/TextMesh Pro/Shaders/`) once URP is active. No new import needed. Can be done anytime after Phase 1.

---

## Phase 6 — Verification (manual, no CI exists)

Spot-check rather than exhaustively testing all 21 scenes:

| Mode | Scenes to check | What to look for |
|---|---|---|
| Deathmatch | `acid_factory.unity`, `valley.unity`, `beach.unity` | Lantern lights, shadow occluders, water/acid shader, Gunfish outline, Bloom/Vignette/Color Grading |
| Race | `RaceModeTestMap.unity`, one `dm_*.unity` template | Lighting/shadows on hazards, post-processing consistency |
| Bassball | `sand_lot.unity` | Water ripple correctness (flagged SRP-Batcher scene), lighting, `Goal.prefab` lights |
| Menus/UI | `MainMenu.unity`, `Stats.unity` | Standalone Volume swap, TMP rendering, Lens Distortion/Vignette |

Also do a general 4-player local input smoke test (unrelated to rendering, but standard practice after any pipeline-level change touching this many scenes/prefabs).

---

## Recommended sequencing

0. Branch → 1. Pipeline bootstrap (throwaway scene) → 2. Shader porting → 3. Lighting migration (pilot → rollout) → 4. Post-processing (last, most subjective/tune-by-eye) → 5. TMP shader assignment (anytime after Phase 1) → 6. Full verification pass.

---

## Critical files

- `Assets/Resources/Shaders/Gunfish.shader`, `Assets/Resources/Shaders/Water.shader` — direct URP HLSL ports
- `Assets/Scripts/LevelObjects/Water/WaterMaterialInterface.cs` — verify `SetFloatArray` still binds post-port
- `Assets/Resources/Prefabs/GameCamera.prefab` — master rig: Cinemachine + lighting manager + post-process volume, instanced in 21 scenes
- `Assets/Scripts/Editor/URP2DLightingMigrator.cs` (new) — bulk `Light2D`/`ShadowCaster2D` conversion, follows `SceneSelectorEditor.cs` convention
- `Assets/Scripts/Editor/PPv2ToURPVolumeConverter.cs` (new) — profile conversion
- `Assets/Scenes/Profiles/Basic.asset`, `MainMenu.asset`, `Skybox.asset` — PPv2 profiles to convert
- `ProjectSettings/GraphicsSettings.asset` / `ProjectSettings/QualitySettings.asset` — pipeline assignment targets (Editor UI only, never hand-edit)
