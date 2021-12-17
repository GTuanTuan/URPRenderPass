using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Custom/ClickScanning")]
public class ClickScanningVolume : VolumeComponent,IPostProcessComponent
{
    public FloatParameter Open=new FloatParameter(1);
    public Vector3Parameter ClickPos = new Vector3Parameter(Vector3.zero);
    public FloatParameter width = new FloatParameter(1);
    public FloatParameter speed = new FloatParameter(10);
    public FloatParameter focus = new FloatParameter(1);
    public ColorParameter scanColor = new ColorParameter(new Color32(0,255,255,255));
    public ColorParameter outlineColor = new ColorParameter(new Color32(255, 172, 255, 255), true, true, true);
    public bool IsActive() => Open.value!=0;

    public void SetClickPos(Vector3 pos)
    {
        ClickPos = new Vector3Parameter(pos);
    }

    public bool IsTileCompatible() => false;

}
