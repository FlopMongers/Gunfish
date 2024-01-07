Shader "Custom/Gunfish"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("Alpha", Range (0.0, 1.0)) = 1.0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range (0.0, 0.03)) = .01
        _OutlineAlpha ("Outline Alpha", Range (0.0, 1.0)) = 1.0
        _OutlineFrequency ("Outline Frequency", Range(0.0, 4.0)) = 1.0 
    }

    SubShader
    {
        Tags
		{
        	"Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
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

            sampler2D _MainTex;
            float _Alpha;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineAlpha;
            float _OutlineFrequency;
            

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color;
                OUT.texcoord.x = IN.texcoord.x;
                OUT.texcoord.y = 1 - IN.texcoord.y;
                return OUT;
            }

            // sin^2(2πft)
            // Squaring the sin gives it a nice bounce
            float getOutlineMultiplier()
            {
                // https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
                // _Time    :	float4   :  Time since level load (t/20, t, t*2, t*3), use to animate things inside the shaders.
                // Therefore _Time.y is just t
                float time = _Time.y;
                float outlineMultiplier = 0.0;

                // Treat near-zero frequencies as always active to avoid flickering
                if (_OutlineFrequency > 0.00000001) {
                    // Sinusoidally move between -1 to 1 based on frequency
                    outlineMultiplier = sin(2 * 3.1415926535 * _OutlineFrequency * time);
                    // Map sin from 0 to 1
                    outlineMultiplier = 0.5 * (1.0 + outlineMultiplier);
                    // sin^2
                    outlineMultiplier *= outlineMultiplier;
                }

                // return the compliment to make the "bounce" linger on full border instead of no border
                return 1.0 - outlineMultiplier;
            }

            float4 sampleColors(v2f IN)
            {
                // Sample color around our pixel, size depends on _OutlineWidth
                fixed4 outlineColor = fixed4(0,0,0,0);
                for (int j = -1; j <= 1; j++)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        outlineColor += tex2D(_MainTex, IN.texcoord + fixed2(i,j) * _OutlineWidth);
                    }
                }
                // Average sampled colors
                outlineColor /= 9;
                return outlineColor;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                // Apply alpha test manually if we're applying outline
                if (c.a < 0.5) discard; 

                fixed4 outlineColor = sampleColors(IN);

                // If alpha of pixels around is less than ours then it is an outline
                if (outlineColor.a < c.a) {
                    float outlineMultiplier = getOutlineMultiplier();
                    float outlineAmount = _OutlineAlpha * outlineMultiplier;
                    c.rgb = _OutlineColor.rgb * outlineAmount + c.rgb * (1 - outlineAmount);
                }

                c.a = _Alpha;
                c.rgb *= c.a;
                return c; 
            }
            ENDCG
        }
    }
}
