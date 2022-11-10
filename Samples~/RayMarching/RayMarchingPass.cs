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
    int mainTexID = Shader.PropertyToID("_SourceTex");

    RenderTargetIdentifier source;
    RenderTargetHandle dest;
    RenderTargetHandle temp;

    public void SetUp(RayMarchingFeature.Settings settings,string passName ,RenderTargetIdentifier source, RenderTargetHandle dest)
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
        volume = stack.GetComponent<RayMarchingVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) {  return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0;
        descriptor.colorFormat = RenderTextureFormat.ARGB32;
        cmd.GetTemporaryRT(temp.id, descriptor);
        cmd.GetTemporaryRT(dest.id, descriptor);
        cmd.SetGlobalTexture(mainTexID, source);

        cmd.Blit(null,source, settings.mat);
        cmd.ReleaseTemporaryRT(temp.id);
        cmd.ReleaseTemporaryRT(dest.id);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}