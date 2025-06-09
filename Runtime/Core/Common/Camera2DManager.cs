using UnityEngine;

namespace Core
{
    /// <summary>
    /// Use for 2D mobile game.
    /// </summary>
    public class Camera2DManager : MonoSingleton<Camera2DManager>
    {
        [Header("Camera reference")]
        [SerializeField] private Camera _mainCamera;

        [Header("Reference resolution (width:height)")]
        [SerializeField] private float _referenceWidth = 1080f;
        [SerializeField] private float _referenceHeight = 1920f;

        [Header("Reference orthographic size (for vertical or horizontal)")]
        [SerializeField] private float _referenceOrthoSizeVertical = 10.5f;
        [SerializeField] private float _referenceOrthoSizeHorizontal = 6.0f;

        [Header("Orientation priority")]
        [SerializeField] private bool _isVertical = true;

        public bool IsVerticalScale { get; set; } = false;

        public bool IsHorizontal { get; set; } = false;

        public bool IsVertical => _isVertical;

        private void Awake()
        {
            AdjustCamera();
        }

        private void AdjustCamera()
        {
            float targetAspect = _referenceWidth / _referenceHeight;
            float windowAspect = (float) Screen.width / Screen.height;

            if (_isVertical)
            {
                if (windowAspect > targetAspect)
                {
                    _mainCamera.orthographicSize = _referenceOrthoSizeVertical;
                    IsVerticalScale = false;
                }
                else
                {
                    float scaleFactor = targetAspect / windowAspect;
                    _mainCamera.orthographicSize = _referenceOrthoSizeVertical * scaleFactor;
                    IsVerticalScale = true;
                }
            }
            else
            {
                float reverseTargetAspect = _referenceHeight / _referenceWidth;
                float windowAspectInv = (float) Screen.height / Screen.width;

                if (windowAspectInv > reverseTargetAspect)
                {
                    _mainCamera.orthographicSize = _referenceOrthoSizeHorizontal;
                    IsHorizontal = false;
                }
                else
                {
                    float scaleFactor = reverseTargetAspect / windowAspectInv;
                    _mainCamera.orthographicSize = _referenceOrthoSizeHorizontal * scaleFactor;
                    IsHorizontal = true;
                }
            }
        }
    }
}