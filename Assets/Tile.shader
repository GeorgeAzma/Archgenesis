Shader "Unlit/Tile"
{

    Properties 
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Width ("Width", Float) = 0.02
        _Tiling ("Tiling", Float) = 0.1
        _Hardness ("Hardness", Float) = 0.9
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            half4 _Color;
            float _Width;
            float _Tiling;
            float _Hardness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.normal = v.normal;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 water(float2 uv, float seed) 
            {
                const float TAU = 6.28318530718;
                const float MAX_ITER = 5;
                float2 p = ((uv*TAU) % TAU)-250.0;
	            float2 i = p;
	            float c = 1.0;
	            float inten = .005;
	            for (int n = 0; n < MAX_ITER; n++) 
	            {
		            float t = 4. * (_Time+seed) * (1.0 - (3.5 / float(n+1)));
		            i = p + float2(cos(t - i.x) + sin(t + i.y), sin(t - i.y) + cos(t + i.x));
		            c += 1.0/length(float2(p.x / (sin(i.x+t)/inten),p.y / (cos(i.y+t)/inten)));
	            }
	            c /= float(MAX_ITER);
	            c = 1.17-pow(c, 1.4);
                float col = pow(abs(c), 8.0);
	            float3 color = float3(col, col, col);
                //color = clamp(color + float3(0.0, 0.35, 0.5), 0.0, 1.0);
                return color;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 scale = float3(
                    length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
                    length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
                    length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
                );
                float2 scyz = scale.yz;
                float2 scxz = scale.xz;
                float2 scxy = scale.xy;
                float3 norm = abs(i.normal);
                float2 scale2d = norm.x > 0.0 ? scyz : (norm.y > 0.0 ? scxz : scxy);
                float hardness = saturate(_Hardness * min(1.0, i.vertex.z * 256.));
                float2 tiling = _Tiling * scale2d;
                float2 uv = (i.uv * tiling) % float2(1, 1);
                float d = _Width;
                float dx = d + tiling.x * 2. * fwidth(i.uv.x);
                float dy = d + tiling.y * 2. * fwidth(i.uv.y);
                float c = smoothstep(1.0 - dx, 1.0, uv.x);
                c = max(c, smoothstep(1.0 - dy, 1.0, uv.y));
                c = max(c, smoothstep(dx, 0.0, uv.x));
                c = max(c, smoothstep(dy, 0.0, uv.y));
                float c2 = c;
                c = smoothstep(hardness * 0.5, 1.0 - hardness * 0.5, c);
                float colf = c * c * c * c * c * c * c + c * 0.3;
                float seed = float(int(i.uv.x * tiling.x).x) * 2.69148 + float(int(i.uv.y * tiling.y)) * 1.283; 
                float3 wat = water(uv, seed) * 0.1;
                float3 col = wat + float3(colf, colf, colf) * 0.05;
                col = lerp(wat, col, c);
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
}
