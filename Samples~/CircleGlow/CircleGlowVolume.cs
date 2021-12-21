using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/CircleGlow")]
public class CircleGlowVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter Open=new FloatParameter(1);
    public bool IsActive() => Open.value!=0;

    public FloatParameter Speed = new FloatParameter(1.0f);

    public ClampedFloatParameter radius = new ClampedFloatParameter(150, 60, 260);

    public IntParameter r = new IntParameter(1);

    public IntParameter c = new IntParameter(1);

    public bool IsTileCompatible() => false;

}
