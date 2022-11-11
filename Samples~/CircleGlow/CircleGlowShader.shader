Shader "Custom/CircleGlow"
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
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    float4 _MainTex_TexelSize;
    float4 _MainTex_ST;
    float _RCount;
    float _Radius;
    float _CCount;
    float _Speed;

    CBUFFER_START(UnityPerMaterial)

    CBUFFER_END

    float4 GetCircle(float t,float2 uv) {
        float2 p = 2 * _ScreenParams.xy * uv - _ScreenParams.xy;
        p = cos(t) * p + sin(t) * float2(p.y, -p.x);
        float wDelta = _ScreenParams.x / _RCount;
        float hDelta = _ScreenParams.y / _CCount;
        float2 pDelta = float2(wDelta, hDelta);
        if (fmod(floor(p.y / hDelta + 0.5), 2) == 0) {
            p.x += wDelta * 0.5;
        }
        float2 pTemp = p + 0.5 * pDelta;
        float2 p2 = pTemp - pDelta * floor(pTemp / pDelta) - 0.5 * pDelta;
        //p2 = cos(t) * p2 + sin(t) * float2(p2.y, -p2.x);

        float2 cell = floor(p / pDelta + 0.5);
        float cellR = frac(sin(dot(cell.xy, float2(12.9898, 78.233))) * 43758.5453);

        float3 c = float3(0.4, 0.1, 0.2);
        c *= frac(cellR * 3.33 + 3.33);

        float radius = lerp(_Radius-50, _Radius+50, cellR);

        float sdf = (length(p2 / radius) - 1) * radius;
        float circle = 1.0 - smoothstep(0.0, 1.0, sdf * 0.04);
        float glow = exp(-sdf * 0.025) * 0.3 * (1.0 - circle);
        return (circle+ glow)*half4(cellR, cellR, cellR, 1);
    }

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
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    v2f vert(appdata input)
    {
        v2f output = (v2f)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        output.vertex = vertexInput.positionCS;
        output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
         
        return output;
    }

    half4 frag(v2f input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
        half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

        half4 bgColor = lerp(half4(0.3, 0.1, 0.3, 1), half4(0.1, 0.4, 0.5, 1), uv.y);

        float4 color1 = GetCircle(_Time.x* _Speed,uv+0.1);
        float4 color2 = GetCircle(-_Time.x* _Speed,uv-0.1);
        float4 color3 = GetCircle(_Time.x* _Speed,uv+0.2);

        half4 color = baseColor+bgColor + color1 + color2 + color3;

        return color;
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