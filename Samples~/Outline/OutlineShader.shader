Shader "Custom/Outline"
{
    HLSLINCLUDE
    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

    struct PureAttributes
    {
        float4 positionOS       : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct PureVaryings
    {
        float4 vertex : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };



    TEXTURE2D(_SourceTex);
    SAMPLER(sampler_SourceTex);
    float4 _SourceTex_TexelSize;

    TEXTURE2D(_BaseTes);
    SAMPLER(sampler_BaseTes);

    TEXTURE2D(_ApplyTex);
    SAMPLER(sampler_ApplyTex);

    float4 _BaseColor;

    float4 _SampleProps;

    bool CheckSame(float2 uv, half uvOffset)
    {
        half4 c0 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv);
        half4 c1 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(-uvOffset, uvOffset) * _SourceTex_TexelSize.xy);
        half4 c2 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(0, uvOffset) * _SourceTex_TexelSize.xy);
        half4 c3 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(uvOffset, uvOffset) * _SourceTex_TexelSize.xy);
        half4 c4 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(-uvOffset, 0) * _SourceTex_TexelSize.xy);
        half4 c5 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(uvOffset, 0) * _SourceTex_TexelSize.xy);
        half4 c6 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(-uvOffset, -uvOffset) * _SourceTex_TexelSize.xy);
        half4 c7 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(0, -uvOffset) * _SourceTex_TexelSize.xy);
        half4 c8 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv + half2(uvOffset, -uvOffset) * _SourceTex_TexelSize.xy);
        float diff0 = length(c1.rgb - c8.rgb);
        float diff1 = length(c2.rgb - c7.rgb);
        float diff2 = length(c3.rgb - c6.rgb);
        float diff3 = length(c4.rgb - c5.rgb);
        return diff0 > _SampleProps.w || diff1 > _SampleProps.w
            || diff2 > _SampleProps.w || diff3 > _SampleProps.w;
    }

    PureVaryings vert(PureAttributes input)
    {
        PureVaryings output = (PureVaryings)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.vertex = vertexInput.positionCS;

        return output;
    }


    half4 frag(PureVaryings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        return _BaseColor;
    }
    half4 fragHard(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
        bool isBorder = CheckSame(uv, _SampleProps.z);
        return isBorder ? _BaseColor : 0;
    }
    half4 fragFinal(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

        half4 baseColor = SAMPLE_TEXTURE2D(_BaseTes, sampler_BaseTes, uv);
        half4 outlineColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv);

        return baseColor * (1 - outlineColor.a) + outlineColor.a * outlineColor;
    }

    ENDHLSL
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
            "IgnoreProjector" = "True"
        }
        LOD 100
        Pass
        {
            Name "Pure Unlit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
        Pass
        {
            Name "Hard Unlit"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment fragHard
            ENDHLSL
        }
        Pass
        {
            Name "Final Unlit"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment fragFinal
            ENDHLSL
        }
    }
}