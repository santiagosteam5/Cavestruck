Shader "Unlit/voronoi"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LavaColor ("Lava Color", Color) = (1, 0.5, 0, 1)
        _RockColor ("Rock Color", Color) = (0.3, 0.3, 0.3, 1)
        _EdgeColor ("Edge Color", Color) = (1, 1, 0, 1)
        _Speed ("Animation Speed", Float) = 1.0
        _CellDensity ("Cell Density", Float) = 5.0
        _EdgeThickness ("Edge Thickness", Float) = 0.2
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
            float4 _LavaColor;
            float4 _RockColor;
            float4 _EdgeColor;
            float _Speed;
            float _CellDensity;
            float _EdgeThickness;

            // Generar un vector aleatorio con rotación dinámica
            float2 randomVector(float2 UV, float offset, float angle)
            {
                float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
                UV = frac(sin(mul(UV, m)) * 46839.32);

                // Rotar el vector usando una matriz de rotación
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2x2 rotationMatrix = float2x2(cosA, -sinA, sinA, cosA);

                float2 randomVec = float2(sin(UV.y * offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
                return mul(rotationMatrix, randomVec);
            }

            // Basado en el código de Inigo Quilez
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
                        float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset, AngleOffset);

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
                        float2 cellOffset = float2(cellToCheck) - posInCell + randomVector(cell + cellToCheck, AngleOffset, AngleOffset);

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

                // Animar el ángulo y tamaño de las celdas
                float AngleOffset = time * 2.0; // Cambiar el ángulo dinámicamente
                float animatedCellDensity = _CellDensity + sin(time * 0.5) * 2.0;

                // Calcular la distancia desde el centro y el borde de la celda
                float distFromCenter, distFromEdge;
                CustomVoronoi_float(uv + time, AngleOffset, animatedCellDensity, distFromCenter, distFromEdge);

                // Normalizar las distancias
                float edgeFactor = smoothstep(0.0, _EdgeThickness, distFromEdge);
                float centerFactor = 1.0 - smoothstep(0.0, 0.5, distFromCenter);

                // Colorear las rocas y la lava
                float4 color = lerp(_EdgeColor, _RockColor, centerFactor);
                color = lerp(_LavaColor, color, edgeFactor);

                return color;
            }
            ENDHLSL
        }
    }
}
