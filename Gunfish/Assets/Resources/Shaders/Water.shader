Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPos: TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            int _NodeCount;
            int _Degree;
            CBUFFER_END

            // Set per-material via Material.SetFloatArray (WaterMaterialInterface.cs) and kept
            // outside UnityPerMaterial: 1000-element arrays would overflow the SRP Batcher's
            // constant buffer, so this material opts out of batching (fine given the low instance count).
            float _NodesX[1000];
            float _NodesY[1000];
            float _Coefficients[1000];

            // _Time (not _Time.y) kept to match the original Built-in RP shader's behavior:
            // HLSL implicitly truncates the float4 to its .x component (t/20), so the wave
            // timing here is driven by t/20, not t. Preserved as-is to avoid changing the look of the water.
            float ripple(float x){
                return  0.021*sin(5*(x-(_Time*30)))+
                        0.021*sin(11.5*(x+(_Time*25)))*sin(x+_Time*25)+
                        0.021*sin(4.6*(x-(_Time*18)))+
                        0.021*sin(2*(x+(_Time+16)));
            }
            float evaluate(float x) {
                float sum = 0;
                for (int i = 0; i < _Degree + 1; i++) {
                    if (i >= _NodeCount)
                        continue;
                    float xpow = 1;
                    for (int j = 0; j < i; j++) {
                        xpow *= x;
                    }
                    sum += _Coefficients[i] * xpow;
                }
                return sum + ripple(x);
            }

            float distanceBelowSurface(float x, float y){
                return max(0, evaluate(x) - y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color = _Color;
                float deep = 0.2;
                float shallow = 1;
                float dist = distanceBelowSurface(i.worldPos.x, i.worldPos.y);
                float alpha = color.a;
                if (dist <= 0) {
                    alpha = 0;
                }
                float brightness = lerp(shallow, deep, min(1, dist / 4));
                color *= brightness;

                color.a = alpha;

                return color;
            }
            ENDHLSL
        }
    }
}
