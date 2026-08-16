Shader "WhalesIslandReef/D19/VegetationWind"
{
    Properties
    {
        // D7MaterialPolishController probes the conventional texture slot when
        // it creates runtime material copies. The vegetation presentation remains
        // color-driven, but declaring the slot keeps that read path error-free.
        _MainTex ("Base Map", 2D) = "white" {}
        _BaseColor ("Foliage Color", Color) = (0.22, 0.48, 0.19, 1)
        _WindDirection ("Wind Direction", Vector) = (0.8, 0.6, 0, 0)
        _WindSpeed ("Wind Speed", Float) = 3.5
        _WindAmplitude ("Wind Amplitude", Float) = 0.08
        _WindFrequency ("Wind Frequency", Float) = 0.75
        _WindGust ("Wind Gust", Range(0, 1)) = 0.25
        _WindPhase ("Deterministic Phase", Float) = 0
        _DistanceFade ("Distance Fade", Float) = 240
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            Name "D19VegetationForward"
            Tags { "LightMode" = "UniversalForward" }
            // LOD proxy quads are intentionally two-sided so a fixed camera
            // route does not disappear when it approaches from the reverse side.
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float3 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _WindDirection;
                float _WindSpeed;
                float _WindAmplitude;
                float _WindFrequency;
                float _WindGust;
                float _WindPhase;
                float _DistanceFade;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS);
                float heightWeight = saturate(input.positionOS.y * 0.55 + 0.25);
                float2 direction = normalize(_WindDirection.xy + float2(0.0001, 0.0001));
                float wave = sin(dot(world.xz, direction) * _WindFrequency + _Time.y * _WindSpeed + _WindPhase);
                float gust = 1.0 + sin(_Time.y * (_WindSpeed * 0.37) + _WindPhase) * _WindGust;
                world.xz += direction * wave * _WindAmplitude * heightWeight * gust;
                output.positionWS = world;
                output.positionHCS = TransformWorldToHClip(world);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 lightDir = normalize(float3(0.25, 0.85, 0.35));
                float lighting = 0.68 + 0.32 * saturate(dot(normalize(input.normalWS), lightDir));
                float fade = 1.0 - saturate(distance(_WorldSpaceCameraPos.xyz, input.positionWS) / max(_DistanceFade, 1.0));
                return half4(_BaseColor.rgb * lighting, _BaseColor.a * (0.88 + fade * 0.12));
            }
            ENDHLSL
        }
    }
}
