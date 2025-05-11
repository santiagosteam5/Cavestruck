Shader "Unlit/LavaVoronoi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RockColor ("Rock Color", Color) = (0.3, 0.3, 0.3, 1)
        _EdgeColor ("Edge Color", Color) = (1, 1, 0, 1)
        _EmissionColor ("Emission Color", Color) = (1, 1, 0, 1)
        _EmissionIntensity ("Emission Intensity", Float) = 1.0
        _Speed ("Animation Speed", Float) = 1.0
        _CellDensity ("Cell Density", Float) = 5.0
        _EdgeThickness ("Edge Thickness", Float) = 0.1
        _LightDirection ("Light Direction", Vector) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _RockColor;
            float4 _EdgeColor;
            float4 _EmissionColor;
            float4 _LightDirection;
            float _Speed;
            float _CellDensity;
            float _EdgeThickness;
            float _EmissionIntensity;

            float2 randomVector(float2 UV, float offset)
            {
                float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
                UV = frac(sin(mul(UV, m)) * 46839.32);
                return float2(sin(UV.y * offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
            }

            void CustomVoronoi_float(float2 UV, float AngleOffset, float CellDensity, out float DistFromCenter, out float DistFromEdge)
            {
                int2 cell = floor(UV * CellDensity);
                float2 posInCell = frac(UV * CellDensity);

                DistFromCenter = 8.0f;
                float2 closestOffset;

                for (int y = -1; y <= 1; ++y)
                {
                    for (int x = -1; x <= 1; ++x)
                    {
                        int2 cellToCheck = int2(x, y);
                        float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset);

                        float distToPoint = dot(cellOffset, cellOffset);

                        if (distToPoint < DistFromCenter)
                        {
                            DistFromCenter = distToPoint;
                            closestOffset = cellOffset;
                        }
                    }
                }

                DistFromEdge = 8.0f;

                for (int y = -1; y <= 1; ++y)
                {
                    for (int x = -1; x <= 1; ++x)
                    {
                        int2 cellToCheck = int2(x, y);
                        float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset);

                        float distToEdge = dot(0.5f * (closestOffset + cellOffset), normalize(cellOffset - closestOffset));

                        DistFromEdge = min(DistFromEdge, distToEdge);
                    }
                }
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 uv = i.uv;

                float distFromCenter, distFromEdge;
                CustomVoronoi_float(uv + time, time, _CellDensity, distFromCenter, distFromEdge);

                distFromCenter = sqrt(distFromCenter) * 0.6;
                distFromEdge = distFromEdge * 30;

                float edgeFactor = smoothstep(0.0, _EdgeThickness, distFromEdge);
                float centerFactor = 1.0 - smoothstep(0.0, 0.5, distFromCenter);

                float3 normal = normalize(float3(
                    distFromEdge - distFromCenter, // Diferencia entre bordes y centro
                    1.0,                          // Altura simulada
                    distFromCenter - distFromEdge  // Diferencia inversa
                ));

                float3 lightDir = normalize(_LightDirection.xyz);
                float lighting = max(0.0, dot(normal, lightDir));

                float4 edgeColor = _EdgeColor * _EmissionIntensity;

                float4 color = lerp(edgeColor, _RockColor, centerFactor);
                color = lerp(edgeColor, color, edgeFactor);

                color.rgb *= lighting;

                return color;
            }
            ENDHLSL
        }
    }
}
