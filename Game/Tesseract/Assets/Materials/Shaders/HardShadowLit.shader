// Lit shader with hard, uniform cast shadows. The surface is lit without any shadow
// darkening, then every shadowed pixel is blended toward one global colour at one opacity
// (_HardShadowColor, set by PixelPerfectCamera). So a shadow is a single flat tint with a
// hard edge, and whatever it falls on still shows through. Soft-shadow filtering is never
// compiled in, and the shadow test is a step, so no edge pixel gets an in-between shade.
Shader "Soul Circuit/Hard Shadow Lit"
{
    Properties
    {
        // _MainTex, not _BaseMap: SpriteRenderer only ever sets _MainTex.
        [MainTexture] _MainTex("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        [Toggle(_ALPHATEST_ON)] _AlphaClip("Alpha Clip", Float) = 0
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        // 1 for sprites: their quads face the camera, so N.L would light them by tilt, not by form.
        [Toggle] _FlatLight("Flat Lighting", Float) = 0
        // Off by default: meshes without vertex colours (Unity's capsule) would otherwise read black.
        [Toggle] _VertexColor("Use Vertex Color", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _BaseColor;
            half _Cutoff;
            half _FlatLight;
            half _VertexColor;
        CBUFFER_END

        half4 Albedo(float2 uv, half4 vertexColor)
        {
            half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _BaseColor * lerp(half4(1, 1, 1, 1), vertexColor, _VertexColor);
            #if defined(_ALPHATEST_ON)
                clip(albedo.a - _Cutoff);
            #endif
            return albedo;
        }
        ENDHLSL

        Pass
        {
            Name "HardShadowForward"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            half4 _HardShadowColor;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1; float2 uv : TEXCOORD2; half4 color : COLOR; half fog : TEXCOORD3; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 albedo = Albedo(input.uv, input.color);
                float3 normal = normalize(input.normalWS);

                Light light = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half diffuse = lerp(saturate(dot(normal, light.direction)), 1, _FlatLight);
                half3 lit = albedo.rgb * (SampleSH(normal) + light.color * diffuse);

                // Shadow strength and filtering are ignored on purpose: in or out, nothing between.
                half inShadow = step(light.shadowAttenuation, 0.5);
                half3 color = lerp(lit, _HardShadowColor.rgb, inShadow * _HardShadowColor.a);
                return half4(MixFog(color, input.fog), 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _ALPHATEST_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection; // set by URP while drawing the main light's shadow map

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                Albedo(input.uv, input.color);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _ALPHATEST_ON

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                Albedo(input.uv, input.color);
                return input.positionCS.z;
            }
            ENDHLSL
        }
    }
}
