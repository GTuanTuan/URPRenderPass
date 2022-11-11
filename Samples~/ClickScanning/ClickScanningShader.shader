Shader "Custom/ClickScanning"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
    }

    HLSLINCLUDE

    #pragma exclude_renderers gles gles3 glcore
    #pragma target 4.5

    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON


    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    float4x4 _Ray;

    CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_TexelSize;
    float4 _MainTex_ST;
    float3 _ClickPos;
    float _Width;
    float _Speed;
    float4 _ScanColor;
    float4 _OutlineColor;
    float _Focus;
    float _Timer;
    CBUFFER_END

    struct appdata
    {
        float4 positionOS       : POSITION;
        float4 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 ray:TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    int SetRay(float2 uv) {
        int index = 0;
        if (uv.x > 0.5 && uv.y > 0.5) {
            index = 1;
        }
        else if (uv.x < 0.5 && uv.y < 0.5) {
            index = 2;
        }
        else if (uv.x > 0.5 && uv.y < 0.5) {
            index = 3;
        }
#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            index = 3 - index;
#endif
        return index;
    }
    //half4 Mlerp(half4 ray1,half4 ray2,)
    half4 GetRay(half2 uv) {
        half4 Ray0 = half4(1,1,1,1);
        half4 mRay;
        if (uv.x < 0.5 && uv.y > 0.5) {
            mRay = lerp( Ray0, _Ray[0], length(uv - half2(0.5, 0.5) / 0.7));
        }
        else if (uv.x > 0.5 && uv.y > 0.5) {
            mRay = lerp( Ray0, _Ray[1], length(uv - half2(0.5, 0.5) / 0.7));
        }
        else if (uv.x < 0.5 && uv.y < 0.5) {
            mRay = lerp( Ray0, _Ray[2], length(uv - half2(0.5, 0.5) / 0.7));
        }
        else if (uv.x > 0.5 && uv.y < 0.5) {
            mRay = lerp( Ray0, _Ray[3], length(uv - half2(0.5, 0.5) / 0.7));
        }
        return mRay;
    }

    v2f vert(appdata input)
    {
        v2f output = (v2f)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.vertex = vertexInput.positionCS;
        output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);

        output.ray = _Ray[SetRay(output.uv)];
        return output;
    }

    half4 frag(v2f input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
       
        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

        float depth = LinearEyeDepth( SampleSceneDepth(uv), _ZBufferParams);
        float depth01 = Linear01Depth(SampleSceneDepth(uv), _ZBufferParams);
        half dc = depth01 > 0.99 ? 0 : 1;
       
        //input.ray = GetRay(uv);
        float3 worldPos = _WorldSpaceCameraPos + depth * input.ray.xyz;

        float x = length(worldPos - _ClickPos);
        float y = _Timer * _Speed;

        //half4 scan = frac(saturate(x - y));
        half4 scan = pow(frac(saturate((x - y) / _Width)), _Focus);

        half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

        return scan *_ScanColor * dc + baseColor*(1-scan*dc);
        //return scan;
        //return  input.ray;
        //return half4(worldPos, 1);
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
            Name "Pass 1"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}