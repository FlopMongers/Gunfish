Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NodesX ("NodesX", Float) = (-6, -2, 4, 6)
        _NodesY ("NodesY", Float) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _NodesX[4];
            float4 _NodesY[4];
                
            float evaluate(float x) {
                return sin(x);
            }

            float distanceBelowSurface(float x, float y){
                return max(0, evaluate(x) - y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                float deep = 0.2;
                float shallow = 1;
                float dist = distanceBelowSurface(i.worldPos.x, i.worldPos.y);
                float alpha = 1;
                if (dist <= 0) {
                    alpha = 0;
                }
                float brightness = lerp(shallow,deep, min(1, dist / 4));

                fixed4 col = fixed4(0,0,brightness, alpha);
                // fixed4 col = (0, 0, 0, 0);
                

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
