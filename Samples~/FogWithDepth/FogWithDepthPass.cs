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
        renderPassEvent = settings.Event;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.isSceneViewCamera) return;
        if (!renderingData.postProcessingEnabled) { return; }
        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<FogWithDepthVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) {  return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        Matrix4x4 Ray = E.URP.RenderMathhelp.InterpolatedRay(renderingData.cameraData.camera);
        
        cmd.SetGlobalTexture(noiseTexID, volume.noiseTex.value);
        cmd.SetGlobalColor(fogColorID, volume.fogColor.value);
        cmd.SetGlobalFloat(fogDensityID, volume.fogDensity.value);
        cmd.SetGlobalFloat(fogStartID, volume.fogStart.value);
        cmd.SetGlobalFloat(fogEndID, volume.fogEnd.value);
        cmd.SetGlobalFloat(speedXID, volume.speedX.value);
        cmd.SetGlobalFloat(speedYID, volume.speedY.value);
        cmd.SetGlobalFloat(noiseAmountID, volume.noiseAmount.value);
        cmd.SetGlobalMatrix(rayID, Ray);

        cmd.Blit(source, dest.Identifier(), settings.mat);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}