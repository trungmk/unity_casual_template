using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class WidgetScalerCustom : MonoBehaviour
    {
        private CanvasScaler _canvasScaler;


        private void Awake()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            _canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        }

        private void Update()
        {
            float h = Screen.safeArea.height;
            float w = Screen.safeArea.width;
            float ratio = (w + h) / h;
            _canvasScaler.scaleFactor = ratio;
        }
    }

}

