using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class ClickScanningPass : ScriptableRenderPass
{
    ClickScanningVolume volume;
    ClickScanningFeature.Settings settings;

    string passName;

    RenderTargetIdentifier source;
    RenderTargetHandle dest;

    int rayID = Shader.PropertyToID("_Ray");
    int clickPosID = Shader.PropertyToID("_ClickPos");
    int widthID = Shader.PropertyToID("_Width");
    int speedID = Shader.PropertyToID("_Speed");
    int scanColorID = Shader.PropertyToID("_ScanColor");
    int outlineColorID = Shader.PropertyToID("_OutlineColor");
    int focusID = Shader.PropertyToID("_Focus");
    int timerID = Shader.PropertyToID("_Timer");

    ClickScanningTest forTest;

    public void SetUp(ClickScanningFeature.Settings settings, string passName, RenderTargetIdentifier source, RenderTargetHandle dest)
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
        volume = stack.GetComponent<ClickScanningVolume>();
        if (volume == null) return;
        if (!volume.IsActive()) { return; }

        if (!GameObject.Find("Camera")) { return; }
        forTest = GameObject.Find("Camera").GetComponent<ClickScanningTest>();
        if (!forTest) { return; }

        if (forTest.Clicked)
        {
            forTest.Timer += Time.fixedDeltaTime/2;

            CommandBuffer cmd = CommandBufferPool.Get(passName);

            Camera camera = renderingData.cameraData.camera;
            Matrix4x4 Ray = E.URP.RenderMathhelp.InterpolatedRay(camera);

            cmd.SetGlobalMatrix(rayID, Ray);
            cmd.SetGlobalVector(clickPosID, forTest.ClickPos);
            cmd.SetGlobalFloat(widthID, volume.width.value);
            cmd.SetGlobalFloat(speedID, volume.speed.value);
            cmd.SetGlobalFloat(focusID, volume.focus.value);
            cmd.SetGlobalFloat(timerID, forTest.Timer);
            cmd.SetGlobalColor(scanColorID, volume.scanColor.value);
            cmd.SetGlobalColor(outlineColorID, volume.outlineColor.value);

            cmd.Blit(source, dest.Identifier(), settings.mat);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}