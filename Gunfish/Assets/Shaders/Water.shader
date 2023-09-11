Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            fixed4 _Color;
            float _NodesX[1000];
            float _NodesY[1000];
            float _Coefficients[1000];
            int _NodeCount;
            int _Degree;
            
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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = _Color;
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
            ENDCG
        }
    }
}
