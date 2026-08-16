Shader "WhalesIslandReef/D19/WaterFoam"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _Color ("Foam Color", Color) = (0.78, 0.96, 0.92, 0.75)
        _ScrollSpeed ("Scroll Speed", Float) = 0.25
        _Softness ("Softness", Range(0.01, 1)) = 0.55
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Name "D19WaterFoamForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : TEXCOORD1; };
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _ScrollSpeed;
                float _Softness;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float pulse = 0.75 + 0.25 * sin((input.uv.x + _Time.y * _ScrollSpeed) * 18.0);
                float edge = saturate(input.color.a * pulse);
                edge = smoothstep(0.0, max(_Softness, 0.01), edge);
                return half4(_Color.rgb, _Color.a * edge);
            }
            ENDHLSL
        }
    }
}
