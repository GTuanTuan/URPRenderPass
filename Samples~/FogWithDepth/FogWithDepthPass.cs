using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class FogWithDepthPass : ScriptableRenderPass
{
    FogWithDepthVolume volume;
    FogWithDepthFeature.Settings settings;

    string passName;

    RenderTargetIdentifier source;
    RenderTargetHandle dest;
    RenderTargetHandle temp;

    int mainTexID = Shader.PropertyToID("_MainTex");
    int noiseTexID = Shader.PropertyToID("_NoiseTex");
    int fogColorID = Shader.PropertyToID("_FogColor");
    int fogDensityID = Shader.PropertyToID("_FogDensity");
    int fogStartID = Shader.PropertyToID("_FogStart");
    int fogEndID = Shader.PropertyToID("_FogEnd");
    int speedXID = Shader.PropertyToID("_SpeedX");
    int speedYID = Shader.PropertyToID("_SpeedY");
    int noiseAmountID = Shader.PropertyToID("_NoiseAmount");
    int rayID = Shader.PropertyToID("_Ray");

    public void SetUp(FogWithDepthFeature.Settings settings,string passName ,RenderTargetIdentifier source, RenderTargetHandle dest)
    {
        this.settings = settings;
        this.passName = passName;
        this.source = source;
        this.dest = dest;
        temp.Init("_Temp");
        renderPassEvent = settings.Event;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.postProcessingEnabled) { return; }
        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<FogWithDepthVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) {  return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        Matrix4x4 Ray = Matrix4x4.identity;
        Camera camera = renderingData.cameraData.camera;

        float fov = camera.fieldOfView;
        float near = camera.nearClipPlane;
        Vector3 up = camera.transform.up;
        Vector3 right = camera.transform.right;
        Vector3 forward = camera.transform.forward;

        float halfH = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        Vector3 toTop = up * halfH;
        Vector3 toRight = right * halfH * camera.aspect;

        Vector3 TL = forward * near + toTop - toRight;
        Vector3 TR = forward * near + toTop + toRight;
        Vector3 BL = forward * near - toTop - toRight;
        Vector3 BR = forward * near - toTop + toRight;
        float scale = TL.magnitude / near;

        TL.Normalize();
        TR.Normalize();
        BL.Normalize();
        BR.Normalize();
        TL *= scale;
        TR *= scale;
        BL *= scale;
        BR *= scale;

        Ray.SetRow(0, BL);
        Ray.SetRow(1, BR);
        Ray.SetRow(2, TR);
        Ray.SetRow(3, TL);

        cmd.SetGlobalTexture(mainTexID, source);
        cmd.SetGlobalTexture(noiseTexID, volume.noiseTex.value);
        cmd.SetGlobalColor(fogColorID, volume.fogColor.value);
        cmd.SetGlobalFloat(fogDensityID, volume.fogDensity.value);
        cmd.SetGlobalFloat(fogStartID, volume.fogStart.value);
        cmd.SetGlobalFloat(fogEndID, volume.fogEnd.value);
        cmd.SetGlobalFloat(speedXID, volume.speedX.value);
        cmd.SetGlobalFloat(speedYID, volume.speedY.value);
        cmd.SetGlobalFloat(noiseAmountID, volume.noiseAmount.value);
        cmd.SetGlobalMatrix(rayID, Ray);

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        descriptor.colorFormat = RenderTextureFormat.ARGB32;
        cmd.GetTemporaryRT(temp.id, descriptor);
        cmd.GetTemporaryRT(dest.id, descriptor);
        //cmd.SetGlobalTexture(mainTexID, source);

        if (dest == RenderTargetHandle.CameraTarget)
        {
            cmd.Blit(source, temp.Identifier(), settings.mat);
            cmd.Blit(temp.Identifier(), source);
        }
        else
        {
            cmd.Blit(source, dest.Identifier(), settings.mat);
        }
        cmd.ReleaseTemporaryRT(temp.id);
        cmd.ReleaseTemporaryRT(dest.id);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}