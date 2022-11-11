Shader "Custom/ScaningShader"
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
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

    half4 _MainTex_ST;
    half4 _MainTex_TexelSize;
    half _ScanWidth;
    half3 _ScanDir;
    half4 _ScanColor;
    half4 _EdgeColor;
    half _SampleDistance;
    half4 _Sensitity;
    half _MinOffset;
    float4x4 _Ray;

    float _WorldPosScale;
    float _TimeScale;
    float _AbsOffset;
    float _PowScale;
    float _SaturateScale;

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);

    struct appdata
    {
        float4 vertex : POSITION;
        half2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        half2 uv : TEXCOORD0;
        float4 ray:TEXCOORD1;
    };

    half CheckSame(half2 uv0) {
        half2 uv[5];
        uv[0] = uv0;
        uv[1] = uv0 + _MainTex_TexelSize.xy * half2(1, 1) * _SampleDistance;
        uv[2] = uv0 + _MainTex_TexelSize.xy * half2(-1, -1) * _SampleDistance;
        uv[3] = uv0 + _MainTex_TexelSize.xy * half2(-1, 1) * _SampleDistance;
        uv[4] = uv0 + _MainTex_TexelSize.xy * half2(1, -1) * _SampleDistance;
        float depth[5];
        float3 normal[5];
        depth[0] = LinearEyeDepth(SampleSceneDepth(uv[0]), _ZBufferParams);
        normal[0] = SampleSceneNormals(uv[0]);
        half offsetD = 0;
        half3 offsetN = half3(0, 0, 0);
        for (int i = 0; i < 4; i++)
        {
            offsetD += depth[0] - LinearEyeDepth(SampleSceneDepth(uv[i + 1]), _ZBufferParams);
            offsetN += normal[0] - SampleSceneNormals(uv[i + 1]);
        }
        int isSameD = saturate(abs(offsetD)) < _MinOffset;
        int isSameN = saturate(abs(offsetN.x + offsetN.y)) < length(half2(_MinOffset, _MinOffset));
        return isSameD * isSameN ? 0 : 1;
    }

    v2f vert(appdata v)
    {
        v2f o = (v2f)0;
        o.vertex = TransformObjectToHClip(v.vertex.xyz);
        float2 uv = v.texcoord;
        o.uv = uv;

#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            o.uv.y = 1 - o.uv.y;
#endif


        int index = 0;
        if (o.uv.x < 0.5 && o.uv.y>0.5) {
            index = 0;
        }
        else if (o.uv.x > 0.5 && o.uv.y > 0.5)
        {
            index = 1;
        }
        else if (o.uv.x < 0.5 && o.uv.y < 0.5)
        {
            index = 2;
        }
        else
        {
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
        float offset = CheckSame(i.uv);
        float depth = LinearEyeDepth(SampleSceneDepth(i.uv),_ZBufferParams);
        float depth1 = Linear01Depth(SampleSceneDepth(i.uv), _ZBufferParams);
        float3 worldPos = _WorldSpaceCameraPos + depth * i.ray.xyz;


        half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

        half4 withEdgeColor = lerp(_EdgeColor, baseColor, 1 - offset);

        //half4 scanValue = saturate(pow(abs(frac(worldPos.x * 0.1 - _Time.y * 0.2) - 0.2), 10) * 30) * (1 - depth);
        half4 scanValue = saturate(pow(abs(frac(worldPos.x * _WorldPosScale - _Time.y * _TimeScale) - _AbsOffset), _PowScale) * _SaturateScale) * (1 - depth1); ;
        //scanValue += step(0.999, scanValue);
        return (1 - scanValue - scanValue * offset) * baseColor + scanValue * _ScanColor + scanValue * offset * _EdgeColor;
        //return (1- scanValue- offset)*baseColor+ scanValue * _ScanColor + offset * _EdgeColor;
        //return worldPos.y;
        //return depth;
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
