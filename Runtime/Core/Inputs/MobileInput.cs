using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core
{
    public class MobileInput : MonoBehaviour
    {
        [SerializeField]
        private bool _isEnableMultiTouch = false;

        [Header("LayerMask to filter touchable objects")]
        [SerializeField]
        private LayerMask _inputLayerMask;

        [SerializeField]
        private float _timeHoldDrag = 0.06f;

        [SerializeField]
        private float _minMagnitudeDirection = 5f;

        private float _timeHold;
        private bool _isTouchDown;
        private Vector2 _touchStartPosition;
        private Vector2 _lastTouchPosition;
        private Camera _mainCamera;

        [Inject]
        private InputFilter _inputFilter;

        protected void Awake()
        {
            Input.multiTouchEnabled = _isEnableMultiTouch;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.touchCount <= 0)
            {
                if (_isTouchDown)
                {
                    HandleTouchUp(_lastTouchPosition);
                }
                return;
            }

            Touch touch = Input.GetTouch(0);
            _lastTouchPosition = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsTouchOnLayer(touch.position))
                        HandleTouchDown(touch);
                    break;

                case TouchPhase.Moved:
                    if (!_isTouchDown) return;

                    _timeHold += Time.deltaTime;
                    if (_timeHold > _timeHoldDrag)
                    {
                        HandleDrag(touch);
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_isTouchDown)
                        HandleTouchUp(touch.position);
                    break;
            }
        }

        private bool IsTouchOnLayer(Vector2 screenPosition)
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            Vector2 worldPoint = _mainCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 0f, _inputLayerMask);
            return hit.collider != null;
        }

        private void HandleTouchDown(Touch touch)
        {
            _isTouchDown = true;
            _touchStartPosition = touch.position;
            _timeHold = 0f;

            _inputFilter?.OnUserPress(_touchStartPosition);
        }

        private void HandleTouchUp(Vector2 position)
        {
            _isTouchDown = false;
            _timeHold = 0f;

            _inputFilter?.OnUserRelease(position);
        }

        private void HandleDrag(Touch touch)
        {
            Vector2 direction = touch.position - _touchStartPosition;
            bool isInputMoving = direction.sqrMagnitude >= _minMagnitudeDirection;

            if (isInputMoving)
            {
                Vector2 dragPosition = touch.position;
                _touchStartPosition = touch.position;

                _inputFilter?.OnUserDrag(dragPosition);
            }
        }
    }
}