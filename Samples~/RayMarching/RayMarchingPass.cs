using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class RayMarchingPass : ScriptableRenderPass
{
    RayMarchingVolume volume;
    RayMarchingFeature.Settings settings;

    string passName;

    RenderTargetIdentifier source;
    RenderTargetHandle dest;

    public void SetUp(RayMarchingFeature.Settings settings,string passName ,RenderTargetIdentifier source, RenderTargetHandle dest)
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
        volume = stack.GetComponent<RayMarchingVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) {  return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        cmd.Blit(source, dest.Identifier(), settings.mat);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}