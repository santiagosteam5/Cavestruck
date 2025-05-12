Shader "Custom/MagicOrbURP"
{
    Properties
    {
        _MainColor("Main Color", Color) = (0.2, 0.6, 1, 1)
        _GlowColor("Glow Color", Color) = (0.5, 0.8, 1, 1)
        _GlowIntensity("Glow Intensity", Float) = 2.0
        _PulseSpeed("Pulse Speed", Float) = 2.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 300
        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha One
            Cull Off
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            float4 _MainColor;
            float4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;
                float fresnel = pow(1.0 - saturate(dot(IN.viewDir, normalize(IN.worldNormal))), 3.0);

                float pulse = sin(time * _PulseSpeed) * 0.5 + 0.5;
                float glow = pulse * _GlowIntensity;

                float3 finalColor = _MainColor.rgb + (_GlowColor.rgb * glow) + (fresnel * _GlowColor.rgb);

                return float4(finalColor, 0.6); // Alpha ligeramente etérea
            }
            ENDHLSL
        }
    }
}
