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
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
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
            float _NodesX[500];
            float _NodesY[500];
            float _NodesdX[500];
            float _NodesdY[500];
            float _Coefficients[50];
            int _NodeCount;
            int _Degree;
            int closestIndexBefore(float x){
                int beforeIndex = _NodeCount - 1;
                for (int i = 0; i < _NodeCount; i++) {
                    float px = _NodesX[i];
                    if (px > x) {
                        beforeIndex = i - 1;
                        break;
                    }
                }
                return beforeIndex;
            }
            float ripple(float x){
                return  0.021*sin(5*(x-(_Time*30)))+
                        0.021*sin(11.5*(x+(_Time*25)))*sin(x+_Time*25)+
                        0.021*sin(4.6*(x-(_Time*18)))+
                        0.021*sin(2*(x+(_Time+16)));
            }
            float2 midpoint(float2 p1, float2 p2, float t){
                return lerp(p1,p2,t);
            }
            float2 cubicBezier(float2 controlPoints[4], float t){
                float2 midpoint0 = midpoint(controlPoints[0],controlPoints[1],t);
                float2 midpoint1 = midpoint(controlPoints[1],controlPoints[2],t);
                float2 midpoint2 = midpoint(controlPoints[2],controlPoints[3],t);
                float2 midmid0 = midpoint(midpoint0,midpoint1,t);
                float2 midmid1 = midpoint(midpoint1,midpoint2,t);
                float2 midmidmid = midpoint(midmid0,midmid1,t);
                return midmidmid;
            }
            float bezierHeight(float x){
                int prevIndex = closestIndexBefore(x);
                int nextIndex = prevIndex+1;
                float4 prev = (_NodesX[prevIndex],_NodesY[prevIndex],_NodesdX[prevIndex],_NodesdY[prevIndex]);
                float4 next = (_NodesX[nextIndex],_NodesY[nextIndex],_NodesdX[nextIndex],_NodesdY[nextIndex]);
                float2 control0 = (prev.x, prev.y);
                float2 control1 = (prev.x+prev[2],prev.y+prev[3]);
                float2 control2 = (next.x-next[2],next.y-next[3]);
                float2 control3 = (next.x, next.y);
                float2 controlPoints[4] = {control0,control1,control2,control3};
                float t = (x-prev.x)/(next.x-prev.x);
                return cubicBezier(controlPoints,t).y;
            }
            float polynomial(float x){
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
                return sum;
            }
            float evaluate(float x) {
                return bezierHeight(x) + ripple(x);
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
