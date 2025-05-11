Shader "Custom/NormalFromHeightWithColor"
{
    Properties
    {
        _HeightTex ("Height Map", 2D) = "white" {}
        _ColorTex ("Color Texture", 2D) = "white" {}
        _Strength ("Normal Strength", Range(0, 5)) = 1
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
            #include "UnityCG.cginc"

            sampler2D _HeightTex;
            sampler2D _ColorTex;
            float4 _HeightTex_TexelSize;
            float _Strength;

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

            float3 heightToNormal(float2 uv)
            {
                float heightC = tex2D(_HeightTex, uv).r;
                float heightR = tex2D(_HeightTex, uv + float2(_HeightTex_TexelSize.x, 0)).r;
                float heightU = tex2D(_HeightTex, uv + float2(0, _HeightTex_TexelSize.y)).r;

                float3 dx = float3(_HeightTex_TexelSize.x, 0, (heightR - heightC) * _Strength);
                float3 dy = float3(0, _HeightTex_TexelSize.y, (heightU - heightC) * _Strength);

                return normalize(cross(dy, dx));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = heightToNormal(i.uv);
                float3 color = tex2D(_ColorTex, i.uv).rgb;

                // Output normal encoded as RGB, blended with color for demonstration
                // If you want raw normal only, comment out the blend line
                float3 normalRGB = normal * 0.5 + 0.5;
                return float4(color * normalRGB, 1.0);
            }
            ENDCG
        }
    }
}
