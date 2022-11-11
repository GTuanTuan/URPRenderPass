using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class CircleGlowPass : ScriptableRenderPass
{
    CircleGlowVolume volume;
    CircleGlowFeature.Settings settings;

    string passName;

    RenderTargetIdentifier source;
    RenderTargetHandle dest;

    int rID = Shader.PropertyToID("_RCount");
    int cID = Shader.PropertyToID("_CCount");
    int SpeedID = Shader.PropertyToID("_Speed");
    int radius = Shader.PropertyToID("_Radius");

    public void SetUp(CircleGlowFeature.Settings settings,string passName ,RenderTargetIdentifier source, RenderTargetHandle dest)
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
        volume = stack.GetComponent<CircleGlowVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) {  return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);
        cmd.SetGlobalFloat(SpeedID, volume.Speed.value);
        cmd.SetGlobalFloat(radius, volume.radius.value);
        cmd.SetGlobalInt(rID, volume.r.value);
        cmd.SetGlobalInt(cID, volume.c.value);

        cmd.Blit(source, dest.Identifier(), settings.mat);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}