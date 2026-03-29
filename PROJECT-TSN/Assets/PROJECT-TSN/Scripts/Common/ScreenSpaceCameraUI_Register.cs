using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TST
{
    public class ScreenSpaceCameraUI_Register : MonoBehaviour
    {
        private void Awake()
        {
            Canvas canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = UIManager.Singleton.UICamera;
        }
    }
}
