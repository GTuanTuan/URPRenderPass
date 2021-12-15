using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/Scanning")]
public class ScanningVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter Open = new FloatParameter(0);
    public FloatParameter scanWidth = new FloatParameter(0.2f);
    public ColorParameter scanColor = new ColorParameter(Color.blue, true, false, false);
    public Vector3Parameter scanDir = new Vector3Parameter(new Vector3(1, 0, 0));
    public FloatParameter outlineMinOffset = new FloatParameter(0.1f);
    public ColorParameter edgeColor = new ColorParameter(Color.black);
    public FloatParameter sampleDistance = new FloatParameter(1);
    public FloatParameter depth = new FloatParameter(1);
    public FloatParameter normal = new FloatParameter(1);
    public FloatParameter _WorldPosScale = new FloatParameter(1);
    public FloatParameter _TimeScale = new FloatParameter(1);
    public FloatParameter _AbsOffset = new FloatParameter(1);
    public FloatParameter _PowScale = new FloatParameter(1);
    public FloatParameter _SaturateScale = new FloatParameter(1);

    public bool IsActive() => Open.value != 0;

    public bool IsTileCompatible() => false;

}
