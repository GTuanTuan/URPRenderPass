using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/Outline")]
public class OutlineVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter Open = new FloatParameter(0);
    public LayerMaskParameter layer = new LayerMaskParameter(0);
    public ColorParameter baseColor = new ColorParameter(Color.blue, true, true, false);
    public FloatParameter checkSize = new FloatParameter(1);
    public FloatParameter uvOffsetSize = new FloatParameter(1);

    public bool IsActive() => Open.value != 0;

    public bool IsTileCompatible() => false;

}
