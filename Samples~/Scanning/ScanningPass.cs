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
    RenderTargetHandle temp;

    int mainTexID = Shader.PropertyToID("_MainTex");
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
        temp.Init("_Temp");
        renderPassEvent = settings.Event;

        //Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.postProcessingEnabled) { return; }
        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<ScanningVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) { return; }

        CommandBuffer cmd = CommandBufferPool.Get(passName);

        Matrix4x4 Ray = Matrix4x4.identity;

        Camera camera = renderingData.cameraData.camera;

        float near = camera.nearClipPlane;
        float fov = camera.fieldOfView;

        Vector3 up = camera.transform.up;
        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        float halfH = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfW = halfH * camera.aspect;

        Vector3 TL = forward * near + up * halfH - right * halfW;
        Vector3 TR = forward * near + up * halfH + right * halfW;
        Vector3 BL = forward * near - up * halfH - right * halfW;
        Vector3 BR = forward * near - up * halfH + right * halfW;

        float scale = TL.magnitude / near;

        TL = TL.normalized * scale;
        TR = TR.normalized * scale;
        BL = BL.normalized * scale;
        BR = BR.normalized * scale;

        Ray.SetRow(0, TL);
        Ray.SetRow(1, TR);
        Ray.SetRow(2, BL);
        Ray.SetRow(3, BR);

        cmd.SetGlobalTexture(mainTexID, source);
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

        if (dest == RenderTargetHandle.CameraTarget)
        {
            cmd.GetTemporaryRT(temp.id, renderingData.cameraData.cameraTargetDescriptor, FilterMode.Bilinear);
            cmd.Blit(source, temp.Identifier(), settings.mat);
            cmd.Blit(temp.Identifier(), source);
            cmd.ReleaseTemporaryRT(temp.id);
        }
        else
        {
            cmd.Blit(source, dest.Identifier(), settings.mat);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}