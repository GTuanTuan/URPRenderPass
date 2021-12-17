using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ClickScanningFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;
        public Material mat = null;
        public Shader shader = null;
    }
    public Settings settings;
    ClickScanningPass pass;
    public override void AddRenderPasses( ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        settings.mat = new Material(settings.shader);
        settings.mat.hideFlags = HideFlags.DontSave;
        pass.SetUp(settings,"ClickScanning", renderer.cameraColorTarget,RenderTargetHandle.CameraTarget);
        renderer.EnqueuePass(pass);
    }

    public override void Create()
    {
        pass = new ClickScanningPass();
    }
}
