Shader "Custom/RainbowPowerupEffect"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [HDR] _BaseColor ("Base Color", Color) = (1,1,1,1)
        _PulseSpeed ("Pulse Speed", Range(0.1, 10.0)) = 2.0
        _RainbowSpeed ("Rainbow Speed", Range(0.1, 10.0)) = 1.0
        _RainbowScale ("Rainbow Scale", Range(0.1, 20.0)) = 8.0
        _GlowIntensity ("Glow Intensity", Range(0.0, 5.0)) = 1.5
        _EffectIntensity ("Effect Intensity", Range(0.0, 1.0)) = 1.0
        _PowerUpActive ("Power Up Active", Range(0, 1)) = 0
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }
        
        LOD 300
        ZWrite On
        Cull Back
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Pragmas para funcionalidad
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float _PulseSpeed;
                float _RainbowSpeed;
                float _RainbowScale;
                float _GlowIntensity;
                float _EffectIntensity;
                float _PowerUpActive;
            CBUFFER_END
            
            // Función optimizada para generar color arcoíris
            float3 Rainbow(float t)
            {
                // Ajuste para colores más vibrantes
                t = frac(t) * 6.283185; // 2*PI
                return float3(
                    sin(t) * 0.5 + 0.5,
                    sin(t + 2.094395) * 0.5 + 0.5, // 2*PI/3
                    sin(t + 4.188790) * 0.5 + 0.5  // 4*PI/3
                );
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Muestrear textura base
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _BaseColor;
                
                // Salida temprana si el efecto está desactivado
                if (_PowerUpActive < 0.001)
                    return half4(baseColor.rgb, 1.0);
                
                // Calcular efecto de pulsación
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse = lerp(1.0, pulse, _EffectIntensity);
                
                // Calcular patrón arcoíris basado en posición mundial
                float rainbowValue = dot(input.positionWS, float3(0.3, 0.59, 0.11)) * _RainbowScale + _Time.y * _RainbowSpeed;
                float3 rainbowColor = Rainbow(rainbowValue);
                
                // Mezclar con el color base
                float3 effectColor = lerp(baseColor.rgb, rainbowColor, _EffectIntensity * 0.7);
                
                // Aplicar pulsación y brillo
                effectColor *= 1.0 + (pulse * _GlowIntensity * _PowerUpActive);
                
                // Conservar el alpha original
                half alpha = baseColor.a;
                
                // Mezcla final controlada por _PowerUpActive
                float3 finalColor = lerp(baseColor.rgb, effectColor, _PowerUpActive);
                
                // Aplicar niebla
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
        
        // Pase ShadowCaster para sombras
        Pass
        {
            Name "ShadowCaster"
            Tags {"LightMode" = "ShadowCaster"}
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Pase DepthOnly para URP
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}
            
            ZWrite On
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "UnityEditor.ShaderGraphLitGUI"
}