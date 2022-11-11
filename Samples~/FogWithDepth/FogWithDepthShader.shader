Shader "Custom/FogWithDepth"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
    }
    HLSLINCLUDE

    #pragma multi_compile_instancing
    #pragma multi_compile _ DOTS_INSTANCING_ON

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

    float4 _MainTex_ST;
    half4 _MainTex_TexelSize;
    float4 _NoiseTex_ST;
    half4 _NoiseTex_TexelSize;
    float4 _FogColor;
    float _FogDensity;
    float _FogStart;
    float _FogEnd;
    float _SpeedX;
    float _SpeedY;
    float _NoiseAmount;
    float4x4 _Ray;

    TEXTURE2D(_MainTex);
    TEXTURE2D(_NoiseTex);
    SAMPLER(sampler_MainTex);
    SAMPLER(sampler_NoiseTex);

    struct appdata
    {
        float4 vertex : POSITION;
        half2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        half2 uv : TEXCOORD0;
        half2 uv_Depth:TEXCOORD1;
        float4 ray:TEXCOORD2;
    };

    v2f vert(appdata v)
    {
        v2f o = (v2f)0;
        o.vertex = TransformObjectToHClip(v.vertex.xyz);
        o.uv = v.texcoord;
        o.uv_Depth = v.texcoord;

#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            o.uv_Depth.y = 1 - o.uv_Depth.y;
#endif
        int index = 0;
        if (v.texcoord.x < 0.5 && v.texcoord.y < 0.5) {
            index = 0;
        }
        else if (v.texcoord.x > 0.5 && v.texcoord.y < 0.5) {
            index = 1;
        }
        else if (v.texcoord.x > 0.5 && v.texcoord.y > 0.5) {
            index = 2;
        }
        else {
            index = 3;
        }
#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            index = 3 - index;
#endif
        o.ray = _Ray[index];
        return o;
    }

    half4 frag(v2f i) : SV_Target
    {
        float linearDepth = LinearEyeDepth(SampleSceneDepth(i.uv_Depth),_ZBufferParams);

        float3 worldPos = _WorldSpaceCameraPos + linearDepth * i.ray.xyz;

        float2 speed = _Time.y * float2(_SpeedX, _SpeedY);
        float noise = (SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.uv + speed).r - 0.5) * _NoiseAmount;

        float fogDensity = (_FogEnd - worldPos.y) / (_FogEnd - _FogStart);
        fogDensity = saturate(fogDensity * _FogDensity * (1 + noise));

        half4 finalColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
        finalColor.rgb = lerp(finalColor.rgb,_FogColor.rgb, fogDensity);

        return finalColor;
    }

    ENDHLSL
    SubShader
    {
        LOD 100
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}