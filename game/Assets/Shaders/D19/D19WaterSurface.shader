Shader "WhalesIslandReef/D19/WaterSurface"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _DeepColor ("Deep Water Color", Color) = (0.015, 0.12, 0.20, 1)
        _ShallowColor ("Shallow Water Color", Color) = (0.04, 0.42, 0.48, 1)
        _FoamColor ("Foam Color", Color) = (0.72, 0.93, 0.88, 1)
        _UnderwaterColor ("Underwater Tint", Color) = (0.02, 0.22, 0.28, 1)
        _WaterLevel ("Sea Level", Float) = 0
        _WaveScale ("Wave Scale", Float) = 0.35
        _WaveSpeed ("Wave Speed", Float) = 0.6
        _WaveAmplitude ("Wave Amplitude", Float) = 0.12
        _WaveLength ("Wave Length", Float) = 18
        _WindDirection ("Wind Direction", Vector) = (0.8, 0.6, 0, 0)
        _WindSpeed ("Wind Speed", Float) = 4
        _WaterTransparency ("Water Transparency", Range(0, 1)) = 0.78
        _Turbidity ("Turbidity", Range(0, 1)) = 0.22
        _DistanceFade ("Distance Fade", Float) = 260
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "D19WaterForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fragment _ _D19_WATER_LOW _D19_WATER_MEDIUM _D19_WATER_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 color : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _FoamColor;
                half4 _UnderwaterColor;
                float _WaterLevel;
                float _WaveScale;
                float _WaveSpeed;
                float _WaveAmplitude;
                float _WaveLength;
                float4 _WindDirection;
                float _WindSpeed;
                float _WaterTransparency;
                float _Turbidity;
                float _DistanceFade;
            CBUFFER_END

            float Wave(float2 positionXZ, float2 direction, float scale, float speed, float amplitude)
            {
                return sin(dot(positionXZ, direction) * scale + _Time.y * speed) * amplitude;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 worldPosition = TransformObjectToWorld(input.positionOS);
                float2 direction = normalize(_WindDirection.xy + float2(0.0001, 0.0001));
                float scale = 6.2831853 / max(_WaveLength, 1.0);
                float displacement = Wave(worldPosition.xz, direction, scale, _WaveSpeed, _WaveAmplitude);
                #if defined(_D19_WATER_MEDIUM) || defined(_D19_WATER_HIGH)
                    displacement += Wave(worldPosition.xz, float2(-direction.y, direction.x), scale * 1.7, _WaveSpeed * 1.35, _WaveAmplitude * 0.35);
                #endif
                #if defined(_D19_WATER_HIGH)
                    displacement += Wave(worldPosition.xz, direction * 0.45 + float2(0.2, -0.1), scale * 2.7, _WaveSpeed * 1.8, _WaveAmplitude * 0.16);
                #endif
                worldPosition.y += displacement;
                output.positionWS = worldPosition;
                output.positionHCS = TransformWorldToHClip(worldPosition);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 direction = normalize(_WindDirection.xy + float2(0.0001, 0.0001));
                float scale = 6.2831853 / max(_WaveLength, 1.0);
                float wave = Wave(input.positionWS.xz, direction, scale, _WaveSpeed, _WaveAmplitude);
                float3 normal = normalize(float3(-wave * 0.45, 1.0, wave * 0.35));
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.positionWS);
                float fresnel = pow(1.0 - saturate(dot(normal, viewDirection)), 3.0);
                float shallowMask = saturate(input.color.r);
                float foamMask = saturate(input.color.g);
                float3 waterColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, shallowMask);
                waterColor = lerp(waterColor, _UnderwaterColor.rgb, _Turbidity * 0.25);
                waterColor += fresnel * 0.18;
                waterColor = lerp(waterColor, _FoamColor.rgb, foamMask * 0.5);
                float distanceFade = 1.0 - saturate(distance(_WorldSpaceCameraPos.xyz, input.positionWS) / max(_DistanceFade, 1.0));
                float alpha = saturate(_WaterTransparency * (0.45 + distanceFade * 0.55));
                alpha = max(alpha, foamMask * 0.42);
                return half4(waterColor, alpha);
            }
            ENDHLSL
        }
    }
}
