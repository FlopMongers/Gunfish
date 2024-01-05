Shader "Custom/Gunfish"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range (0.0, 0.03)) = .005
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

        Cull Off 
		Lighting Off 
		ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex	: POSITION;
                float4 color	: COLOR;
                float2 texcoord	: TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex	: SV_POSITION;
                fixed4 color	: COLOR;
                float2 texcoord	: TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            sampler2D _MainTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color * _Color;
                OUT.texcoord.x = IN.texcoord.x;
                OUT.texcoord.y = 1 - IN.texcoord.y;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                // apply alpha test manually if we're applying outline
                if (c.a < 0.5) discard; 
                // sample color around our pixel, size depends on _OutlineWidth
                fixed4 outlineColor = fixed4(0,0,0,0);
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        outlineColor += tex2D(_MainTex, IN.texcoord + fixed2(i,j) * _OutlineWidth);
                    }
                }
                // average sampled colors
                outlineColor /= 9;
                // if alpha of pixels around is less than ours then it's outline
                if (outlineColor.a < c.a) c.rgb = _OutlineColor.rgb;
                c.rgb *= c.a;
                return c; 
            }
            ENDCG
        }
    }
}
