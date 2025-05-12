Shader "URP/voronoi"
{
    Properties
    {
        _RockColor ("Rock Color", Color) = (0.3, 0.3, 0.3, 1)
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0.0, 1)
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
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _RockColor;
            float4 _EdgeColor;
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
                for (int y2 = -1; y2 <= 1; ++y2)
                {
                    for (int x2 = -1; x2 <= 1; ++x2)
                    {
                        int2 cellToCheck = int2(x2, y2);
                        float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset);
                        float edgeDist = dot(0.5f * (closestOffset + cellOffset), normalize(cellOffset - closestOffset));
                        DistFromEdge = min(DistFromEdge, edgeDist);
                    }
                }
            }


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y * _Speed;
                float2 uv = IN.uv;

                float distFromCenter, distFromEdge;
                CustomVoronoi_float(uv + time, 4, _CellDensity, distFromCenter, distFromEdge);

                distFromCenter = sqrt(distFromCenter) * 1.5;
                distFromEdge = distFromEdge * 50;

                float edgeFactor = smoothstep(0.0, _EdgeThickness, distFromEdge);
                float centerFactor = 1.0 - smoothstep(0.0, 0.5, distFromCenter);

                float3 normal = normalize(float3(distFromEdge - distFromCenter, 1.0, distFromCenter - distFromEdge));
                float3 lightDir = normalize(_LightDirection.xyz);
                float lighting = max(0.0, dot(normal, lightDir));

                float4 edgeColor = _EdgeColor * _EmissionIntensity;

                float4 baseColor = lerp(edgeColor, _RockColor, centerFactor);
                baseColor = lerp(edgeColor, baseColor, edgeFactor);
                baseColor.rgb *= lighting;

                float3 emission = edgeColor.rgb * (1.0 - edgeFactor);

                return float4(baseColor.rgb + emission, 1.0);
            }
            ENDHLSL
        }
    }
}
