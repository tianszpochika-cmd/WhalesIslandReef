Shader "WhalesIslandReef/D19/UnderwaterVolume"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _UnderwaterColor ("Underwater Color", Color) = (0.02, 0.24, 0.30, 1)
        _UnderwaterAlpha ("Underwater Alpha", Range(0, 1)) = 0.22
        _WaterLevel ("Sea Level", Float) = 0
        _CameraY ("Camera Y", Float) = 5
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+20" "RenderType" = "Transparent" }
        Pass
        {
            Name "D19UnderwaterTint"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float3 positionOS : POSITION; };
            struct Varyings { float4 positionHCS : SV_POSITION; float3 positionWS : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                half4 _UnderwaterColor;
                float _UnderwaterAlpha;
                float _WaterLevel;
                float _CameraY;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                if (_CameraY >= _WaterLevel - 0.02) discard;
                float depthFactor = saturate((_WaterLevel - _CameraY) / 8.0);
                return half4(_UnderwaterColor.rgb, _UnderwaterAlpha * (0.65 + depthFactor * 0.35));
            }
            ENDHLSL
        }
    }
}
