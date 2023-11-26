Shader "Unlit/Trail"
{
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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * 2. - 1.;
                float a = smoothstep(0.4, 1.0, abs(uv.y)) + smoothstep(-0.3, 1.0, abs(uv.y)) * 0.3;
                //a *= 1.0 - i.uv.x;
                fixed4 col = fixed4(fixed3(0.0, 0.3, 1) * a * a + smoothstep(0.8, 1.0, uv.y * uv.y) * a, saturate(a));
                return col;
            }
            ENDCG
        }
    }
}
