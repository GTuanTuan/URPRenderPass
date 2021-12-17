using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.URP
{
    public class RenderMathhelp:MonoBehaviour
    {
        public static Matrix4x4 InterpolatedRay(Camera camera)
        {
            Matrix4x4 Ray = Matrix4x4.identity;

            float near = camera.nearClipPlane;
            float fov = camera.fieldOfView;

            Vector3 up = camera.transform.up;
            Vector3 forward = camera.transform.forward;
            Vector3 right = camera.transform.right;

            float halfH = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            float halfW = halfH * camera.aspect;

            Vector3 LT = forward * near + up * halfH - right * halfW;
            Vector3 RT = forward * near + up * halfH + right * halfW;
            Vector3 LB = forward * near - up * halfH - right * halfW;
            Vector3 RB = forward * near - up * halfH + right * halfW;

            float scale = LT.magnitude / near;

            LT = LT.normalized * scale;
            RT = RT.normalized * scale;
            LB = LB.normalized * scale;
            RB = RB.normalized * scale;

            Ray.SetRow(0, LT);
            Ray.SetRow(1, RT);
            Ray.SetRow(2, LB);
            Ray.SetRow(3, RB);

            return Ray;
        }
    }
}
