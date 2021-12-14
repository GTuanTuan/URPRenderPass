using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/FogWithDepth")]
public class FogWithDepthVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter Open = new FloatParameter(0);
    public ColorParameter fogColor = new ColorParameter(Color.white);
    public FloatParameter fogDensity = new FloatParameter(1.0f);
    public FloatParameter fogStart = new FloatParameter(0.0f);
    public FloatParameter fogEnd = new FloatParameter(2.0f);
    public FloatParameter speedX = new FloatParameter(0.1f);
    public FloatParameter speedY = new FloatParameter(0.1f);
    public FloatParameter noiseAmount = new FloatParameter(1f);
    public TextureParameter noiseTex = new TextureParameter(null);
    public bool IsActive() => Open.value!=0;

    public bool IsTileCompatible() => false;

}
