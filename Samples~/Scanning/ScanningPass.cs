using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class ScanningPass : ScriptableRenderPass
{
    ScanningVolume volume;
    ScanningFeature.Settings settings;

    string passName;

    RenderTargetIdentifier source;
    RenderTargetHandle dest;

    int scanWidthID = Shader.PropertyToID("_ScanWidth");
    int scanColorID = Shader.PropertyToID("_ScanColor");
    int scanDirID = Shader.PropertyToID("_ScanDir");
    int outlineMinOffsetID = Shader.PropertyToID("_MinOffset");
    int depthNormalTexID = Shader.PropertyToID("_CameraDepthNormalsTexture");
    int edgeColorID = Shader.PropertyToID("_EdgeColor");
    int sampleDistaceID = Shader.PropertyToID("_SampleDistance");
    int sensitityID = Shader.PropertyToID("_Sensitity");
    int rayID = Shader.PropertyToID("_Ray");

    int _WorldPosScale = Shader.PropertyToID("_WorldPosScale");
    int _TimeScale = Shader.PropertyToID("_TimeScale");
    int _AbsOffset = Shader.PropertyToID("_AbsOffset");
    int _PowScale = Shader.PropertyToID("_PowScale");
    int _SaturateScale = Shader.PropertyToID("_SaturateScale");
    public void SetUp(ScanningFeature.Settings settings, string passName, RenderTargetIdentifier source, RenderTargetHandle dest)
    {
        ConfigureInput(ScriptableRenderPassInput.Normal);
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
        volume = stack.GetComponent<ScanningVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) { return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        Matrix4x4 Ray = E.URP.RenderMathhelp.InterpolatedRay(renderingData.cameraData.camera);

        cmd.SetGlobalFloat(scanWidthID, volume.scanWidth.value);
        cmd.SetGlobalVector(scanDirID, volume.scanDir.value);
        cmd.SetGlobalColor(scanColorID, volume.scanColor.value);
        cmd.SetGlobalFloat(outlineMinOffsetID, volume.outlineMinOffset.value);
        cmd.SetGlobalColor(edgeColorID, volume.edgeColor.value);
        cmd.SetGlobalFloat(sampleDistaceID, volume.sampleDistance.value);
        cmd.SetGlobalVector(sensitityID, new Vector4(volume.depth.value, volume.normal.value, 1, 1));
        cmd.SetGlobalMatrix(rayID, Ray);

        cmd.SetGlobalFloat(_WorldPosScale, volume._WorldPosScale.value);
        cmd.SetGlobalFloat(_TimeScale, volume._TimeScale.value);
        cmd.SetGlobalFloat(_AbsOffset, volume._AbsOffset.value);
        cmd.SetGlobalFloat(_PowScale, volume._PowScale.value);
        cmd.SetGlobalFloat(_SaturateScale, volume._SaturateScale.value);

        cmd.Blit(source, dest.Identifier(), settings.mat);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}