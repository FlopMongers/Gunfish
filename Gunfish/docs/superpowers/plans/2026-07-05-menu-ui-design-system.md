# Menu UI Design System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Dark UI sprite skin on the menu-page flow (Splash/GameModeSelect/FishSelect) with a token-driven, procedurally-drawn (SDF shader) panel/button system, per `docs/superpowers/specs/2026-07-05-menu-ui-design-system-design.md`.

**Architecture:** A small static token module (`UISpacing`/`UITheme`) feeds two reusable components (`UIPanel`, `UIButton`) backed by one hand-written rounded-box SDF shader. An Editor-time tool (`MenuUIThemeApplier`, following the `GunfishPrefabBaker.cs` precedent) applies these components and `LayoutGroup`s to the real prefab assets, since prefab/scene YAML is never hand-edited per `CLAUDE.md`.

**Tech Stack:** Unity 6000.5.0f1, C#, UGUI (`UnityEngine.UI`), TextMeshPro, a hand-written ShaderLab/HLSL shader (no Shader Graph), `UnityEditor.PrefabUtility`.

## Global Constraints

- Scope is menu pages only: `SplashPage`, `GameModeSelectPage`, `FishSelectPage` (all live inside the single prefab `Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab`), plus `Assets/Resources/Prefabs/UI/Menu/FishSelectPanel.prefab` (instantiated ~4x as children inside `FishSelectPage`). In-match UI (HUD/pause/stats) is out of scope.
- Typography: Oswald only. Cardo is not used by any new code. Spacing scale: `XS=4, SM=8, MD=16, LG=24, XL=32, XXL=48, XXXL=64`.
- Never hand-edit `.prefab`/`.unity` YAML directly — all prefab changes go through Editor scripts using `PrefabUtility`, mirroring `Assets/Scripts/Editor/GunfishPrefabBaker.cs`.
- `Assets/Resources/Prefabs/UI/Menu/GameModeSelectPanel.prefab` is not referenced anywhere inside `MainMenuCanvas.prefab` (verified: its guid `eafeb6099f97d8f4984baee228ba905f` has zero occurrences there). It is likely orphaned, like `FishPowerup`. **Do not modify it in this plan** — the live `GameModeSelectPanel` GameObject is a separate, inlined hierarchy inside `MainMenuCanvas.prefab`.
- `LeftButton`/`RightButton` GameObjects are directional arrow-glyph icons (wired to `RectTransform leftArrow`/`rightArrow` fields, animated via `DOPunchScale`), not `Button` components and not flat rectangular chrome. A rounded-rect SDF cannot represent an arrow silhouette — **do not convert them** in this pass.
- `ConfirmHint`/`CancelHint`/`ReadyHint`/`DisabledHint` sprites have baked-in text/icons the user explicitly said "look mostly fine" — **do not convert them**.
- No CLI entry point and no automated test suite exist for this project (per `CLAUDE.md`). Unity is already open in the user's Editor for this project, so a second batch-mode instance cannot be launched to verify compiles (`Multiple Unity instances cannot open the same project` — confirmed). Every task's verification step is therefore: the user (or executor) checks the already-open Editor's Console for compiler/shader errors, and/or performs a visual check and reports back (screenshot or description). Tasks are written so code is complete and correct on inspection; "run the test" steps are phrased as explicit Editor actions, not automated commands.

---

### Task 1: Design tokens — `UISpacing` and `UITheme`

**Files:**
- Create: `Assets/Scripts/UI/Theme/UISpacing.cs`
- Create: `Assets/Scripts/UI/Theme/UITheme.cs`

**Interfaces:**
- Produces: `UISpacing.XS/SM/MD/LG/XL/XXL/XXXL` (float consts); `UITheme.TypeStyle` (struct: `Font`, `Size`, `Style`); `UITheme.PageTitle/SectionHeader/Body/ButtonLabel` (static `TypeStyle`); `UITheme.Surface900/Surface700/Surface500` (static `Color`).

- [ ] **Step 1: Write `UISpacing.cs`**

```csharp
public static class UISpacing {
    public const float XS = 4f;
    public const float SM = 8f;
    public const float MD = 16f;
    public const float LG = 24f;
    public const float XL = 32f;
    public const float XXL = 48f;
    public const float XXXL = 64f;
}
```

- [ ] **Step 2: Write `UITheme.cs`**

```csharp
using TMPro;
using UnityEngine;

public static class UITheme {
    public readonly struct TypeStyle {
        public readonly TMP_FontAsset Font;
        public readonly float Size;
        public readonly FontStyles Style;

        public TypeStyle(string fontResourcePath, float size, FontStyles style = FontStyles.Normal) {
            Font = Resources.Load<TMP_FontAsset>(fontResourcePath);
            Size = size;
            Style = style;
        }
    }

    public static readonly TypeStyle PageTitle =
        new TypeStyle("Fonts/Oswald/static/Oswald-Bold SDF", 64f, FontStyles.Bold);
    public static readonly TypeStyle SectionHeader =
        new TypeStyle("Fonts/Oswald/static/Oswald-SemiBold SDF", 32f);
    public static readonly TypeStyle Body =
        new TypeStyle("Fonts/Oswald/static/Oswald-Regular SDF", 24f);
    public static readonly TypeStyle ButtonLabel =
        new TypeStyle("Fonts/Oswald/static/Oswald-Medium SDF", 20f);

    public static readonly Color Surface900 = new Color32(0x05, 0x0F, 0x1C, 0xFF);
    public static readonly Color Surface700 = new Color32(0x0A, 0x27, 0x40, 0xFF);
    public static readonly Color Surface500 = new Color32(0x14, 0x3D, 0x5C, 0xFF);
}
```

`Oswald-SemiBold SDF` and `Oswald-Medium SDF` don't exist yet (Task 2 creates them) — `Resources.Load` returns `null` until then, which is safe since nothing calls `UITheme` yet in this task.

- [ ] **Step 3: Verify in Editor**

Focus the Unity Editor window (it will auto-recompile on regaining focus). Open Window → General → Console and confirm there are no new compiler errors mentioning `UISpacing` or `UITheme`.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/Theme/UISpacing.cs Assets/Scripts/UI/Theme/UISpacing.cs.meta Assets/Scripts/UI/Theme/UITheme.cs Assets/Scripts/UI/Theme/UITheme.cs.meta
git commit -m "Add UI spacing and type-scale design tokens"
```

Note: Unity generates `.meta` files automatically once the Editor regains focus and imports the new files — do this *before* `git add` (Step 3 already causes this).

---

### Task 2: Generate missing Oswald SDF font assets (manual, Editor-only)

No code — TMP Font Asset Creator only works interactively in the Editor.

- [ ] **Step 1:** In Unity, open Window → TextMeshPro → Font Asset Creator.
- [ ] **Step 2:** Source Font File: `Assets/Resources/Fonts/Oswald/static/Oswald-SemiBold.ttf`. Use the same Atlas Resolution / Character Set / Rendering Mode settings as the existing `Oswald-Bold SDF.asset` (open that asset's Inspector first to read its settings, so the new one matches).
- [ ] **Step 3:** Click Generate Font Atlas, then Save as `Oswald-SemiBold SDF` inside `Assets/Resources/Fonts/Oswald/static/`.
- [ ] **Step 4:** Repeat Steps 2–3 for `Assets/Resources/Fonts/Oswald/static/Oswald-Medium.ttf` → save as `Oswald-Medium SDF` in the same folder.
- [ ] **Step 5:** Confirm both new `.asset` files exist at `Assets/Resources/Fonts/Oswald/static/Oswald-SemiBold SDF.asset` and `Assets/Resources/Fonts/Oswald/static/Oswald-Medium SDF.asset`.
- [ ] **Step 6: Commit**

```bash
git add "Assets/Resources/Fonts/Oswald/static/Oswald-SemiBold SDF.asset" "Assets/Resources/Fonts/Oswald/static/Oswald-SemiBold SDF.asset.meta" "Assets/Resources/Fonts/Oswald/static/Oswald-Medium SDF.asset" "Assets/Resources/Fonts/Oswald/static/Oswald-Medium SDF.asset.meta"
git commit -m "Generate Oswald SemiBold/Medium TMP SDF font assets"
```

---

### Task 3: `UIRoundedRect` shader

**Files:**
- Create: `Assets/Shaders/UI/UIRoundedRect.shader`

**Interfaces:**
- Produces: shader named `Gunfish/UI/RoundedRect`, with material properties `_BorderColor` (Color), `_Size` (Vector, rect size in pixels), `_Radius` (Float, px), `_BorderWidth` (Float, px), `_Softness` (Float, px). Fill color is read from UGUI's per-vertex color (`Image.color`), not a separate property.

- [ ] **Step 1: Write the shader**

```shaderlab
Shader "Gunfish/UI/RoundedRect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _Size ("Rect Size (px)", Vector) = (100,100,0,0)
        _Radius ("Corner Radius (px)", Float) = 16
        _BorderWidth ("Border Width (px)", Float) = 0
        _Softness ("Edge Softness (px)", Float) = 1.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 localPos      : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _BorderColor;
            float4 _Size;
            float _Radius;
            float _BorderWidth;
            float _Softness;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, OUT);
                UNITY_TRANSFER_INSTANCE_ID(v, OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.localPos = (v.texcoord - 0.5) * _Size.xy;
                OUT.color = v.color;
                return OUT;
            }

            // Inigo Quilez rounded-box SDF: distance from p to a box of
            // half-extents b with corner radius r, centered at origin.
            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 halfSize = _Size.xy * 0.5;
                float r = min(_Radius, min(halfSize.x, halfSize.y));
                float dist = sdRoundedBox(IN.localPos, halfSize, r);

                float outerAlpha = 1.0 - smoothstep(0.0, _Softness, dist);
                float innerDist = dist + _BorderWidth;
                float fillMask = 1.0 - smoothstep(0.0, _Softness, innerDist);

                fixed4 col = lerp(_BorderColor, IN.color, fillMask);
                col.a *= outerAlpha;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
```

- [ ] **Step 2: Verify in Editor**

Focus Unity, let it import/compile the shader, then open Console and confirm no shader compile errors reference `UIRoundedRect.shader`. As a quick visual sanity check: create a temporary `Image` on any `Canvas` in a scratch scene, assign a new Material using shader `Gunfish/UI/RoundedRect`, set `_Size` to roughly the Image's pixel `Rect Width/Height`, and confirm it renders a solid rounded rectangle rather than a magenta "shader error" material. Delete the scratch Image/Material afterward (don't save the scratch scene).

- [ ] **Step 3: Commit**

```bash
git add "Assets/Shaders/UI/UIRoundedRect.shader" "Assets/Shaders/UI/UIRoundedRect.shader.meta"
git commit -m "Add rounded-box SDF shader for flat UI panels/buttons"
```

---

### Task 4: `UIPanel` component

**Files:**
- Create: `Assets/Scripts/UI/Theme/UIPanel.cs`

**Interfaces:**
- Consumes: shader `Gunfish/UI/RoundedRect` (Task 3).
- Produces: `UIPanel` (subclasses `Image` directly, rather than requiring a sibling `Image`), public properties `BorderColor`, `CornerRadius`, `BorderWidth` (get/set, each setter re-applies to the material). Fill color is the inherited `Image.color` — no separate `FillColor` property.

**Redesigned after live Editor testing surfaced Inspector clutter** (originally: `UIPanel : MonoBehaviour` + `[RequireComponent(typeof(Image))]` + a sibling `Button` for Task 5's `UIButton`, four stacked components total for one themed button). Decided with the user to have `UIPanel` subclass `Image` directly (collapsing `Image`+`UIPanel` into one component) and `UIButton` subclass `Button` directly (Task 5) — down to two components total. Verified the exact override signatures and base-call ordering against the real UGUI source shipped in this project (`Library/PackageCache/com.unity.ugui@.../Runtime/UGUI/UI/Core/{Graphic,Image,Selectable,Button}.cs`) rather than assuming them, since getting this wrong silently breaks masking/raycasting/interaction across every converted menu element:
- `Graphic`/`Image` override `OnEnable`/`OnDisable`/`OnRectTransformDimensionsChange`/`OnValidate` as `protected override` — call `base.X()` in the same order Unity's own code does (base first for `OnEnable`/`OnValidate`, base first then own cleanup after for `OnDisable`).
- `Graphic.OnValidate()` (and `Image`'s override of it) are wrapped in `#if UNITY_EDITOR` — our override must be too, or the project won't compile for a player build.
- `Graphic` already exposes `rectTransform` and a settable `material` property — no need to re-fetch or re-declare them.
- `color` (inherited from `Graphic`) already flows to the shader's per-vertex fill color automatically via Unity's own mesh generation — no manual `image.color = fillColor` line needed.

- [ ] **Step 1: Write `UIPanel.cs`**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class UIPanel : Image {
    [SerializeField] private Color borderColor = new Color32(0x02, 0x47, 0x8e, 0xFF);
    [SerializeField] private float cornerRadius = 16f;
    [SerializeField] private float borderWidth = 0f;
    [SerializeField] private float edgeSoftness = 1.5f;

    private static Shader roundedRectShader;
    private Material materialInstance;

    public Color BorderColor { get => borderColor; set { borderColor = value; Apply(); } }
    public float CornerRadius { get => cornerRadius; set { cornerRadius = Mathf.Max(0f, value); Apply(); } }
    public float BorderWidth { get => borderWidth; set { borderWidth = Mathf.Max(0f, value); Apply(); } }

    protected override void OnEnable() {
        base.OnEnable();
        sprite = null;
        type = Image.Type.Simple;

        if (roundedRectShader == null) {
            roundedRectShader = Shader.Find("Gunfish/UI/RoundedRect");
        }
        if (roundedRectShader == null) {
            Debug.LogError("UIPanel: shader 'Gunfish/UI/RoundedRect' not found.");
            return;
        }
        materialInstance = new Material(roundedRectShader);
        material = materialInstance;

        transform.hasChanged = false;
        Apply();
    }

    protected override void OnDisable() {
        base.OnDisable();
        if (materialInstance != null) {
            DestroyImmediate(materialInstance);
            materialInstance = null;
        }
    }

    private void Update() {
        if (transform.hasChanged) {
            Apply();
            transform.hasChanged = false;
        }
    }

    protected override void OnRectTransformDimensionsChange() {
        base.OnRectTransformDimensionsChange();
        Apply();
    }

#if UNITY_EDITOR
    protected override void OnValidate() {
        base.OnValidate();
        cornerRadius = Mathf.Max(0f, cornerRadius);
        borderWidth = Mathf.Max(0f, borderWidth);
        edgeSoftness = Mathf.Max(0f, edgeSoftness);
        Apply();
    }

    protected override void Reset() {
        base.Reset();
        color = UITheme.Surface700;
    }
#endif

    private void Apply() {
        if (materialInstance == null) return;

        Rect rect = rectTransform.rect;
        Vector3 scale = rectTransform.lossyScale;
        float width = rect.width * Mathf.Abs(scale.x);
        float height = rect.height * Mathf.Abs(scale.y);
        materialInstance.SetVector("_Size", new Vector4(width, height, 0, 0));
        materialInstance.SetFloat("_Radius", cornerRadius);
        materialInstance.SetFloat("_BorderWidth", borderWidth);
        materialInstance.SetFloat("_Softness", edgeSoftness);
        materialInstance.SetColor("_BorderColor", borderColor);
    }
}
```

`_Size` is inherently per-instance (every panel/button has a different `RectTransform` size), so each `UIPanel` owns its own cloned `Material` rather than sharing one asset — this trades the "single shared Material batches everything" framing from the spec for a simpler, per-instance-correct implementation. At the scale of a menu screen (a few dozen elements at most), the extra draw calls are negligible; this doesn't reintroduce the performance concern raised earlier, since that was about avoiding expensive per-pixel work (e.g. blur), not draw-call count. All shader property writes go through `materialInstance` exclusively — never a shared material asset.

Three independent change-detection hooks cover three disjoint gaps in Unity's API, found one at a time via live Editor testing: `Update()`+`transform.hasChanged` catches `Transform.Scale` changes (which never touch RectTransform dimensions); `OnRectTransformDimensionsChange()` catches Width/Height/anchor/pivot changes (which don't set `hasChanged`, since resizing around a fixed pivot doesn't move `localPosition`); `OnValidate()` catches direct Inspector field edits (which bypass the C# properties entirely via `SerializedProperty` reflection). None of the three is redundant with the others. `cornerRadius`/`borderWidth`/`edgeSoftness` are clamped to `Mathf.Max(0f, ...)` at both entry points that can set them (the property setters for code-driven changes, `OnValidate` for Inspector edits), since negative values are nonsensical for this shader.

No `[ExecuteAlways]` attribute needed on `UIPanel` itself — `Graphic` already carries it, and it's inherited. The shader lookup is guarded: if `Shader.Find` ever fails to resolve `Gunfish/UI/RoundedRect` (shader stripped, renamed, etc.), `OnEnable` logs an error and returns early rather than calling `new Material(null)`, which would throw and take down every `UIPanel` in the scene the same way.

- [ ] **Step 2: Verify in Editor**

Focus Unity, confirm Console is clean. In a scratch `Canvas`, add an empty GameObject with `RectTransform`, add `UIPanel` directly (no separate `Image` needed — `UIPanel` is one), set a non-white `color` and a `CornerRadius` of ~16 in the Inspector (component has `[ExecuteAlways]`, so it should preview live in Scene/Game view without entering Play mode). Confirm it renders a rounded rectangle that resizes correctly when you drag the `RectTransform`'s width/height handles, and also when scaled via the Transform's Scale field. Delete the scratch object afterward.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/Theme/UIPanel.cs Assets/Scripts/UI/Theme/UIPanel.cs.meta
git commit -m "Add UIPanel rounded-rect graphic component"
```

---

### Task 5: `UIButton` component

**Files:**
- Create: `Assets/Scripts/UI/Theme/UIButton.cs`

**Interfaces:**
- Consumes: `UIPanel` (Task 4), `UITheme.ButtonLabel` (Task 1).
- Produces: `UIButton` (subclasses `Button` directly, requires a sibling `UIPanel`), public field `label` (optional `TMP_Text`).

**Redesigned alongside Task 4** (see that task's redesign note): `UIButton` subclasses `Button` instead of requiring it, collapsing the 4-component stack (`Image`+`Button`+`UIPanel`+`UIButton`) down to 2 (`UIPanel`+`UIButton`). Verified against the real UGUI source: `Selectable.Awake()` is `protected override void Awake()` and already does `if (m_TargetGraphic == null) m_TargetGraphic = GetComponent<Graphic>()` — since `UIPanel` (Task 4) is now itself a `Graphic` (via `Image`), this means `targetGraphic` would resolve correctly even without explicit assignment, but the code below still sets it explicitly for clarity, matching the original design's intent. `transition` defaults to `Transition.ColorTint` already in `Selectable`, so the explicit assignment below is a deliberate, explicit re-assertion (matching the original design), not a required override of a different default.

- [ ] **Step 1: Write `UIButton.cs`**

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIPanel))]
public class UIButton : Button {
    [SerializeField] private TMP_Text label;

    protected override void Awake() {
        base.Awake();

        transition = Selectable.Transition.ColorTint;
        targetGraphic = GetComponent<UIPanel>();

        if (label != null && UITheme.ButtonLabel.Font != null) {
            label.font = UITheme.ButtonLabel.Font;
            label.fontSize = UITheme.ButtonLabel.Size;
            label.fontStyle = UITheme.ButtonLabel.Style;
        }
    }
}
```

- [ ] **Step 2: Verify in Editor**

Focus Unity, confirm Console is clean. On the scratch `UIPanel` object from Task 4, add this `UIButton` component; confirm no `RequireComponent` errors and that entering Play mode and clicking the button shows the default color-tint hover/press feedback. Delete the scratch object afterward.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/Theme/UIButton.cs Assets/Scripts/UI/Theme/UIButton.cs.meta
git commit -m "Add UIButton component wrapping Button + UIPanel"
```

---

### Task 6: Update `FishSelectPanel.cs` to drive border color through `UIPanel`

`FishSelectPanel.SetColor(Color)` currently sets `outline.color` directly on an `Image` — once `Outline` becomes a `UIPanel` (Task 7), its `Image.color` is the *fill*, not the border, so this call needs to target `UIPanel.BorderColor` instead.

**Files:**
- Modify: `Assets/Scripts/UI/FishSelectPanel.cs`

**Interfaces:**
- Consumes: `UIPanel.BorderColor` (Task 4).
- Produces: `FishSelectPanel.outline` is now typed `UIPanel` (was `Image`) — the existing Inspector reference on the prefab will need re-wiring by hand (Task 9), since Unity cannot auto-migrate a serialized reference across a component type change.

- [ ] **Step 1: Change the field type and `SetColor`**

In `Assets/Scripts/UI/FishSelectPanel.cs:14`, change:

```csharp
public Image outline;
```

to:

```csharp
public UIPanel outline;
```

In `Assets/Scripts/UI/FishSelectPanel.cs:56-58`, change:

```csharp
public void SetColor(Color color) {
    outline.color = color;
}
```

to:

```csharp
public void SetColor(Color color) {
    outline.BorderColor = color;
}
```

- [ ] **Step 2: Verify in Editor**

Focus Unity and check the Console. Expect a *new* warning/error of the form "the referenced script... is missing" is NOT what you're looking for — instead expect the `FishSelectPanel` component's `Outline` field in any Inspector where it's currently assigned (the `FishSelectPanel.prefab`, and any of its instances in `MainMenuCanvas.prefab`) to show as empty/`None`, since the serialized reference no longer matches the field's new type. This is expected and is fixed in Task 9 alongside the `UIPanel` conversion itself (the `Outline` GameObject doesn't have a `UIPanel` component to reference until Task 7 runs).

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/UI/FishSelectPanel.cs
git commit -m "Point FishSelectPanel border color at UIPanel instead of Image"
```

---

### Task 7: Editor tool — sprite-to-`UIPanel`/`UIButton` conversion

Following the `GunfishPrefabBaker.cs` precedent (Editor-time static class using `PrefabUtility`, no hand-edited YAML), this converts a name-confirmed allowlist of flat panel/border chrome GameObjects inside `FishSelectPanel.prefab` and the embedded `GameModeSelectPanel`/page hierarchy inside `MainMenuCanvas.prefab`. The allowlist deliberately excludes: arrow-glyph icons (`LeftButton`/`RightButton` — not representable as a rounded rect), baked-text hint icons (`ConfirmHint`/`CancelHint`/`ReadyHint`/`DisabledHint` — user said these look fine), and content-display images (anything showing a fish/game-mode sprite, not decorative chrome).

**Files:**
- Create: `Assets/Scripts/Editor/MenuUIThemeApplier.cs`

**Interfaces:**
- Consumes: `UIPanel`, `UIButton` (Tasks 4–5), `UITheme.Surface700` (Task 1).
- Produces: `MenuUIThemeApplier.ConvertChromeInPrefab(string prefabPath)` (used again by Task 8), two `[MenuItem]` entry points.

**Note from Task 4/5's redesign:** since `UIPanel` now subclasses `Image` directly (rather than requiring a sibling `Image`), every allowlisted chrome GameObject already has a plain `Image` that must be *replaced*, not augmented — adding `UIPanel` alongside an existing `Image` would leave two `Graphic` components both trying to render the same rect. The conversion below removes the existing `Image` before adding `UIPanel` (matching the original, pre-redesign design's intent, the old sprite's `color` is *not* preserved — every converted panel gets the theme's `UITheme.Surface700` fill directly, for a consistent look across all converted chrome, not whatever arbitrary color the old sprite happened to have). It also detects "already converted" via `image is UIPanel` rather than a separate `GetComponent<UIPanel>()` call, since a `UIPanel` found by `GetComponentsInChildren<Image>()` *is* the already-converted instance (inheritance means it's still an `Image` too). `panel.FillColor` from the original design no longer exists — fill color is set via the inherited `color` property directly.

- [ ] **Step 1: Write `MenuUIThemeApplier.cs`**

```csharp
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MenuUIThemeApplier {
    private static readonly HashSet<string> ChromeAllowlist = new HashSet<string> {
        "InnerPanel", "Outline", "TopPanel", "MiddlePanel", "BottomPanel", "DetailsPanel"
    };

    [MenuItem("Tools/Gunfish/UI Theme/Convert Chrome In FishSelectPanel Prefab")]
    public static void ConvertFishSelectPanel() {
        ConvertChromeInPrefab("Assets/Resources/Prefabs/UI/Menu/FishSelectPanel.prefab");
    }

    [MenuItem("Tools/Gunfish/UI Theme/Convert Chrome In MainMenuCanvas Prefab")]
    public static void ConvertMainMenuCanvas() {
        ConvertChromeInPrefab("Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab");
    }

    public static void ConvertChromeInPrefab(string prefabPath) {
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try {
            int converted = 0, skipped = 0;
            var images = root.GetComponentsInChildren<Image>(true);
            foreach (var image in images) {
                var go = image.gameObject;
                if (!ChromeAllowlist.Contains(go.name)) {
                    skipped++;
                    continue;
                }
                if (image is UIPanel) {
                    skipped++;
                    continue; // already converted
                }

                Object.DestroyImmediate(image);
                var panel = go.AddComponent<UIPanel>();
                panel.color = UITheme.Surface700;
                panel.BorderColor = Color.clear;
                panel.CornerRadius = 16f;
                panel.BorderWidth = 0f;
                converted++;
                Debug.Log($"MenuUIThemeApplier: converted '{go.name}' in {prefabPath} to UIPanel.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
            if (!success) {
                Debug.LogError($"MenuUIThemeApplier: failed to save {prefabPath}");
            } else {
                Debug.Log($"MenuUIThemeApplier: {prefabPath} done. Converted {converted}, skipped {skipped}.");
            }
        } finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
```

`BorderColor = Color.clear` and `BorderWidth = 0f` are placeholder-safe defaults (no visible border) rather than a guessed "correct" look — Task 9's screenshot review loop is where actual per-element radius/border/color values get tuned, since that requires visual judgment this tool can't make.

- [ ] **Step 2: Verify in Editor**

Focus Unity, confirm Console is clean (no compiler errors). Do **not** run the menu items yet — that happens in Task 9 alongside the FishSelectPanel row/page layout tasks, so all prefab changes can be reviewed together in one screenshot pass.

- [ ] **Step 3: Commit**

```bash
git add Assets/Scripts/Editor/MenuUIThemeApplier.cs Assets/Scripts/Editor/MenuUIThemeApplier.cs.meta
git commit -m "Add Editor tool to convert Dark UI chrome to UIPanel"
```

---

### Task 8: Editor tool — page-level title-to-content spacing and FishSelectPanel row spacing

Extends `MenuUIThemeApplier` with two more menu items operating on `MainMenuCanvas.prefab`: a `VerticalLayoutGroup` (spacing `UISpacing.LG`) on each of the three confirmed top-level page roots (`SplashPage`, `GameModeSelectPage`, `FishSelectPage`), and a `HorizontalLayoutGroup` (spacing `UISpacing.XL`) on the shared parent of the `FishSelectPanel` row — discovered via the `FishSelectPanel` component type rather than a guessed path, since the exact parent name wasn't confirmed from static inspection.

**Files:**
- Modify: `Assets/Scripts/Editor/MenuUIThemeApplier.cs`

**Interfaces:**
- Consumes: `UISpacing.LG`, `UISpacing.XL` (Task 1), `FishSelectPanel` (existing MonoBehaviour).
- Produces: `MenuUIThemeApplier.ApplyPageSpacing()`, `MenuUIThemeApplier.ApplyFishSelectRowSpacing()`.

- [ ] **Step 1: Add the page-spacing method**

Add to `MenuUIThemeApplier.cs`:

```csharp
    private static readonly string[] PageRootNames = { "SplashPage", "GameModeSelectPage", "FishSelectPage" };
    private const string MainMenuCanvasPath = "Assets/Resources/Prefabs/UI/Menu/MainMenuCanvas.prefab";

    [MenuItem("Tools/Gunfish/UI Theme/Apply Page Title Spacing")]
    public static void ApplyPageSpacing() {
        var root = PrefabUtility.LoadPrefabContents(MainMenuCanvasPath);
        try {
            foreach (var pageName in PageRootNames) {
                var pageTransform = FindDeepChild(root.transform, pageName);
                if (pageTransform == null) {
                    Debug.LogError($"MenuUIThemeApplier: could not find page root '{pageName}' in {MainMenuCanvasPath}");
                    continue;
                }
                var pageGo = pageTransform.gameObject;
                var layout = pageGo.GetComponent<VerticalLayoutGroup>();
                if (layout == null) {
                    layout = pageGo.AddComponent<VerticalLayoutGroup>();
                }
                layout.spacing = UISpacing.LG;
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                Debug.Log($"MenuUIThemeApplier: applied VerticalLayoutGroup (spacing {UISpacing.LG}) to '{pageName}'.");
            }

            PrefabUtility.SaveAsPrefabAsset(root, MainMenuCanvasPath, out bool success);
            if (!success) Debug.LogError($"MenuUIThemeApplier: failed to save {MainMenuCanvasPath}");
        } finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform FindDeepChild(Transform parent, string name) {
        if (parent.name == name) return parent;
        foreach (Transform child in parent) {
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
```

`childControlWidth`/`childControlHeight`/`childForceExpandWidth`/`childForceExpandHeight` are all left `false` — the layout group only enforces the *gap* between existing children, it doesn't resize them, so existing title/content sizing is preserved.

- [ ] **Step 2: Add the FishSelectPanel row-spacing method**

Add to `MenuUIThemeApplier.cs`:

```csharp
    [MenuItem("Tools/Gunfish/UI Theme/Apply FishSelect Row Spacing")]
    public static void ApplyFishSelectRowSpacing() {
        var root = PrefabUtility.LoadPrefabContents(MainMenuCanvasPath);
        try {
            var panels = root.GetComponentsInChildren<FishSelectPanel>(true);
            if (panels.Length == 0) {
                Debug.LogError($"MenuUIThemeApplier: no FishSelectPanel instances found in {MainMenuCanvasPath}");
                return;
            }

            var parent = panels[0].transform.parent;
            foreach (var panel in panels) {
                if (panel.transform.parent != parent) {
                    Debug.LogError("MenuUIThemeApplier: FishSelectPanel instances don't share a common parent; skipping row spacing.");
                    return;
                }
            }

            var parentGo = parent.gameObject;
            var layout = parentGo.GetComponent<HorizontalLayoutGroup>();
            if (layout == null) {
                layout = parentGo.AddComponent<HorizontalLayoutGroup>();
            }
            layout.spacing = UISpacing.XL;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            Debug.Log($"MenuUIThemeApplier: applied HorizontalLayoutGroup (spacing {UISpacing.XL}) to '{parentGo.name}' ({panels.Length} panels).");

            PrefabUtility.SaveAsPrefabAsset(root, MainMenuCanvasPath, out bool success);
            if (!success) Debug.LogError($"MenuUIThemeApplier: failed to save {MainMenuCanvasPath}");
        } finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
```

- [ ] **Step 3: Verify in Editor**

Focus Unity, confirm Console is clean (no compiler errors). Don't run these menu items yet — Task 9 runs all four `MenuUIThemeApplier` menu items together and reviews the combined result.

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/Editor/MenuUIThemeApplier.cs
git commit -m "Add page-title and FishSelectPanel row spacing to MenuUIThemeApplier"
```

---

### Task 9: Run the conversion, re-wire references, and visual QA pass (manual)

This is where the four automated passes actually get applied to the real prefabs, the one broken serialized reference from Task 6 gets fixed by hand, and the remaining manual/judgment-based cleanup (internal per-panel spacing, per-element radius/border tuning) happens with your eyes on the result.

- [ ] **Step 1: Run the conversion tools**

In Unity: Tools → Gunfish → UI Theme → run, in this order:
1. `Convert Chrome In FishSelectPanel Prefab`
2. `Convert Chrome In MainMenuCanvas Prefab`
3. `Apply Page Title Spacing`
4. `Apply FishSelect Row Spacing`

Check the Console after each — every converted/skipped GameObject is logged. If any `MenuUIThemeApplier: could not find page root...` or `...don't share a common parent...` error appears, stop and report it back before continuing (it means one of the structural assumptions in Task 8 didn't hold and needs revisiting, not a value to paper over).

- [ ] **Step 2: Re-wire `FishSelectPanel.Outline`**

Open `Assets/Resources/Prefabs/UI/Menu/FishSelectPanel.prefab` in Prefab Mode. Select the root, find the `FishSelectPanel` component's `Outline` field (now typed `UIPanel`, currently empty per Task 6). Drag the `Outline` child GameObject (now carrying a `UIPanel` component from Step 1) into that field. Save the prefab (Ctrl+S while in Prefab Mode).

- [ ] **Step 3: Apply internal per-panel spacing**

Still in Prefab Mode for `FishSelectPanel.prefab`: identify the shared parent of `TopPanel`/`MiddlePanel`/`BottomPanel`/`DetailsPanel` (or whichever of these actually nest together — confirm by looking at the Hierarchy, since exact nesting wasn't verified ahead of time). Add a `Vertical Layout Group` to that parent, set Spacing to `16` (matches `UISpacing.MD`), leave Control Child Size and Child Force Expand unchecked. Repeat for the equivalent stack inside the embedded `GameModeSelectPanel` hierarchy in `MainMenuCanvas.prefab` (its `DetailsPanel` region).

- [ ] **Step 4: Tune per-element radius/border/color**

For each `UIPanel` created in Step 1, select it and adjust `Corner Radius`/`Border Width`/`Border Color`/`Fill Color` in the Inspector until it looks right (`[ExecuteAlways]` means changes preview live). Use `UITheme.Surface700`/`Surface500` (`#0A2740`/`#143D5C`) as starting fill values for panels, and the relevant player color (already wired via `FishSelectPanel.SetColor`) for `Outline`'s border.

- [ ] **Step 5: Screenshot review**

Enter Play mode, step through Splash → GameModeSelect → FishSelect, and take a screenshot of each page. Share them for review — this is the checkpoint where token values (radius, spacing, colors) get adjusted based on what's actually on screen, per the spec's stated review loop.

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "Apply menu UI theme conversion to FishSelectPanel and MainMenuCanvas prefabs"
```

Review the `git status`/`git diff --stat` output before this commit — it will include binary/YAML prefab changes; confirm only the two intended prefabs (`FishSelectPanel.prefab`, `MainMenuCanvas.prefab`) changed before staging with `-A`.

---

## Self-Review Notes

- **Spec coverage:** Tokens (Task 1), missing fonts (Task 2), shader (Task 3), `UIPanel`/`UIButton` (Tasks 4–5), per-player border color plumbing (Task 6), prefab conversion tool (Tasks 7–8), and the page/row spacing fixes for FishSelect's "zero spacing" and the title-position inconsistency (Task 8) are all covered. Exact color/hex finalization and internal-panel nesting are explicitly manual (Task 9) per the spec's stated limits on what can be automated without visual judgment.
- **Orphaned prefab:** `GameModeSelectPanel.prefab` (standalone asset) is flagged as likely dead code but intentionally left untouched — deleting it is a separate decision outside this plan's scope.
- **Type consistency:** `UIPanel.BorderColor`/`FillColor`/`CornerRadius`/`BorderWidth` names are used identically across Tasks 4, 6, 7, and 9. `UISpacing.LG`/`XL`/`MD` values match the spec's token table throughout.
