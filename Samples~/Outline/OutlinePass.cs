using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class OutlinePass : ScriptableRenderPass
{
    OutlineVolume volume;
    OutlineFeature.Settings settings;

    string passName;

    RenderTargetIdentifier source;
    RenderTargetHandle temp0;
    RenderTargetHandle temp1;
    // RenderTargetHandle tempSource;
    RenderTargetHandle tempDest;
    ShaderTagId tagId;

    DrawingSettings drawingSettings = new DrawingSettings();
    FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);

    int edgeColorID = Shader.PropertyToID("_BaseColor");
    int _SourceTex = Shader.PropertyToID("_SourceTex");
    int _BaseTes = Shader.PropertyToID("_BaseTes");
    int _MainTex = Shader.PropertyToID("_MainTex");
    int _SampleProps = Shader.PropertyToID("_SampleProps");

    public void SetUp(OutlineFeature.Settings settings, string passName, RenderTargetIdentifier source)
    {
        ConfigureInput(ScriptableRenderPassInput.Normal);
        this.settings = settings;
        this.passName = passName;
        this.source = source;
        temp0.Init("_Temp0");
        temp1.Init("_Temp1");
        //tempSource.Init("_TempSource");
        tempDest.Init("_TempDest");
        renderPassEvent = settings.Event;
        tagId = new ShaderTagId("UniversalForward");
        drawingSettings.SetShaderPassName(0, tagId);
        drawingSettings.overrideMaterial = settings.mat;
        drawingSettings.overrideMaterialPassIndex = 0;
        filteringSettings = new FilteringSettings(RenderQueueRange.all);
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.postProcessingEnabled) { return; }
        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<OutlineVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) { return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        //render object
        //cmd.GetTemporaryRT(temp1.id, renderingData.cameraData.cameraTargetDescriptor, FilterMode.Bilinear);
        //cmd.GetTemporaryRT(tempDest.id, renderingData.cameraData.cameraTargetDescriptor, FilterMode.Bilinear);
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        //descriptor.msaaSamples = 1;
        //descriptor.depthBufferBits = 0;
        descriptor.colorFormat = RenderTextureFormat.ARGB32;
        cmd.GetTemporaryRT(temp0.id, descriptor);
        cmd.GetTemporaryRT(temp1.id, descriptor);
        //cmd.GetTemporaryRT(tempSource.id, descriptor);
        cmd.GetTemporaryRT(tempDest.id, descriptor);
        RenderTargetIdentifier temp0ID = temp0.Identifier();
        RenderTargetIdentifier temp1ID = temp1.Identifier();
        //RenderTargetIdentifier tempSourceID = tempSource.Identifier();
        RenderTargetIdentifier tempDestID = tempDest.Identifier();

        //cmd.Blit(source, tempSourceID);

        cmd.SetRenderTarget(temp0ID);
        cmd.ClearRenderTarget(true, true, Color.clear);

        drawingSettings.sortingSettings = new SortingSettings(renderingData.cameraData.camera);
        drawingSettings.overrideMaterial = settings.mat;
        drawingSettings.overrideMaterialPassIndex = 0;
        filteringSettings.layerMask = volume.layer.value;

        cmd.SetGlobalColor(edgeColorID, volume.baseColor.value);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

        //render outline
        cmd.SetGlobalTexture(_SourceTex, temp0ID);
        cmd.SetGlobalVector(_SampleProps, new Vector4(1, 1, volume.uvOffsetSize.value, volume.checkSize.value));
        cmd.Blit(temp0ID, temp1ID, settings.mat, 1);

        //final
        cmd.SetGlobalTexture(_SourceTex, temp1ID);
        cmd.SetGlobalTexture(_BaseTes, source);
        Blit(cmd, temp1ID, tempDestID, settings.mat, 2);
        Blit(cmd, tempDestID, source);
        context.ExecuteCommandBuffer(cmd);
        cmd.ReleaseTemporaryRT(temp0.id);
        cmd.ReleaseTemporaryRT(temp1.id);
        //cmd.ReleaseTemporaryRT(tempSource.id);
        cmd.ReleaseTemporaryRT(tempDest.id);
        CommandBufferPool.Release(cmd);
    }
}