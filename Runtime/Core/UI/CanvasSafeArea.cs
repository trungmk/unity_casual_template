using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core
{
    /// https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/ 
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class CanvasSafeArea : MonoBehaviour
    {
        [SerializeField] private RectTransform _safeAreaRect = null;
 
        private Rect _lastSafeArea = Rect.zero;
        
        private Canvas _canvas;
 
        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }
 
        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea)
            {
                _lastSafeArea = Screen.safeArea;
                ApplySafeArea();
            }
        }
   
        void Start()
        {
            _lastSafeArea = Screen.safeArea;
            ApplySafeArea();
        }
 
        void ApplySafeArea()
        {
            if (_safeAreaRect == null)
            {
                return;
            }
 
            Rect safeArea = Screen.safeArea;
 
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            var pixelRect = _canvas.pixelRect;
            anchorMin.x /= pixelRect.width;
            anchorMin.y /= pixelRect.height;
            anchorMax.x /= pixelRect.width;
            anchorMax.y /= pixelRect.height;
 
            _safeAreaRect.anchorMin = anchorMin;
            _safeAreaRect.anchorMax = anchorMax;
        }
    }
}

