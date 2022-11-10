using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/RayMarching")]
public class RayMarchingVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter Open=new FloatParameter(0);
    public bool IsActive() => Open.value!=0;

    public bool IsTileCompatible() => false;

}
