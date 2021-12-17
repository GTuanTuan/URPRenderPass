using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickScanningTest : MonoBehaviour
{
    public Mouse mouse = Mouse.current;
    public float Timer;
    public float StartTimer = 0;
    public bool Clicked = false;
    public Vector3 ClickPos;
    private void Update()
    {
        if (mouse != null)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                Timer = StartTimer;
                Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
                RaycastHit[] hits = Physics.RaycastAll(ray);
                if (hits.Length > 0)
                {
                    ClickPos = hits[0].point;
                    Clicked = true;
                }
            }
        }
    }
}
