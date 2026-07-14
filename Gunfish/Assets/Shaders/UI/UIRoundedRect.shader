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
        _OutlineOnly ("Outline Only", Float) = 0

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
            float _OutlineOnly;
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

                fixed4 fillColor = IN.color;
                fillColor.a *= (1.0 - _OutlineOnly);

                fixed4 col = lerp(_BorderColor, fillColor, fillMask);
                col.a *= outerAlpha;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                // Discard fully-transparent pixels outside the rounded silhouette so a
                // stencil Mask using this graphic clips to the SDF shape, not the quad.
                clip(col.a - 0.001);

                return col;
            }
            ENDCG
        }
    }
}
