Shader "Custom/RayMarching"
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
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    float4 _MainTex_TexelSize;
    float4 _MainTex_ST;

    CBUFFER_START(UnityPerMaterial)

    CBUFFER_END


    float Sdf_Sphere(float3 p,float3 c,float r)
    {
        return length(p-c)-r;
    }

    float raymarching(float3 ro,float3 rd)
    {
        float dist = 0.0;
        for(int i = 0; i <256;i++)
        {
            float3 p = ro+rd*dist;
            float depth = Sdf_Sphere(p,float3(0,0,1),0.5);
            if(depth<=0.001)
            {
                return dist;
            }
            dist=dist+depth;
            if(dist>=1000) return 1000;
        }
        return 1000;
    }

    float3 calnormal(float3 p)
    {
        return normalize(float3(
            Sdf_Sphere(p+float3(0.001,0,0),float3(0,0,1),0.5)-Sdf_Sphere(p-float3(0.001,0,0),float3(0,0,1),0.5),
            Sdf_Sphere(p+float3(0,0.001,0),float3(0,0,1),0.5)-Sdf_Sphere(p-float3(0,0.001,0),float3(0,0,1),0.5),
            Sdf_Sphere(p+float3(0,0,0.001),float3(0,0,1),0.5)-Sdf_Sphere(p-float3(0,0,0.001),float3(0,0,1),0.5)
        ));
    }

    float3 callight(float3 p)
    {
        Light light = GetMainLight();
        float3 view_dir = normalize(float3(0,0,-1)-p);
        float3 half_dir = normalize(view_dir+light.direction);
        float3 normal = calnormal(p);
        float3 ambient_color = 0.1*light.color;
        float3 diffuse_color =  light.color * max(0,dot(light.direction,normal));
        float3 specular_color = light.color * pow(max(0,dot(normal,half_dir)), 2);
        return ambient_color+diffuse_color+specular_color;
    }

    float3 render(float3 ro,float3 rd,float2 uv)
    {
        half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        float dist = raymarching(ro,rd);
        if(dist>=1000) return float3(baseColor.rgb);
        float3 p = ro+rd*dist;
        return callight(p);
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

        float aspect = _ScreenParams.x/_ScreenParams.y;

        float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
        float3 ro =float3(0,0,-1);
        float3 to = float3((uv.x-0.5)*aspect,uv.y-0.5,0);
        float3 rd = normalize(to-ro);
        float3 color = render(ro,rd,uv);
        return half4(color,1);
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