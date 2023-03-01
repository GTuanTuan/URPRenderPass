using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FogWithDepthFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;
        public Material mat = null;
        public Shader shader = null;
    }
    public Settings settings;
    RenderTargetHandle m_CameraColorAttachment;
    FogWithDepthPass pass;
    public override void AddRenderPasses( ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.shader == null) return;
        settings.mat = new Material(settings.shader);
        settings.mat.hideFlags = HideFlags.DontSave;
        pass.SetUp(settings,"FogWithDepth", renderer.cameraColorTarget, m_CameraColorAttachment);
        renderer.EnqueuePass(pass);
    }

    public override void Create()
    {
        if (settings.shader == null) return;
        pass = new FogWithDepthPass();
        m_CameraColorAttachment.Init("_CameraColorAttachmentA");
    }
}
