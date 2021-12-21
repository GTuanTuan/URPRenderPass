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
    RenderTargetHandle temp;

    int rID = Shader.PropertyToID("_RCount");
    int cID = Shader.PropertyToID("_CCount");
    int SpeedID = Shader.PropertyToID("_Speed");
    int radius = Shader.PropertyToID("_Radius");
    int _SourceTexID = Shader.PropertyToID("_SourceTex");

    public void SetUp(CircleGlowFeature.Settings settings,string passName ,RenderTargetIdentifier source, RenderTargetHandle dest)
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
        volume = stack.GetComponent<CircleGlowVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) {  return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        descriptor.colorFormat = RenderTextureFormat.ARGB32;
        cmd.GetTemporaryRT(temp.id, descriptor);
        cmd.GetTemporaryRT(dest.id, descriptor);

        cmd.SetGlobalTexture(_SourceTexID, source);
        cmd.SetGlobalFloat(SpeedID, volume.Speed.value);
        cmd.SetGlobalFloat(radius, volume.radius.value);
        cmd.SetGlobalInt(rID, volume.r.value);
        cmd.SetGlobalInt(cID, volume.c.value);

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