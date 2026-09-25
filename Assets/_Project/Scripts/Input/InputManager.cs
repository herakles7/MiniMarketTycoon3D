using System;
using UnityEngine;
using UnityEngine.EventSystems;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Input
{
    /// <summary>
    /// Unified input manager supporting mobile multi-touch gestures and Editor mouse fallback.
    /// Handles camera pan, zoom, and world tap selection with UI blocking.
    /// </summary>
    public class InputManager : MonoBehaviourSingleton<InputManager>
    {
        [Header("Drag / Pan Settings")]
        [SerializeField] private float _dragThresholdPixels = 10f;
        [SerializeField] private float _mousePanSensitivity = 1.0f;
        [SerializeField] private float _touchPanSensitivity = 1.0f;

        [Header("Zoom Settings")]
        [SerializeField] private float _mouseScrollSensitivity = 5.0f;
        [SerializeField] private float _touchPinchSensitivity = 0.02f;

        public event Action<Vector2> OnPan;
        public event Action<float> OnZoom;
        public event Action<Vector2> OnTap;

        private Vector2 _lastPointerPosition;
        private Vector2 _pointerDownPosition;
        private bool _isDragging = false;
        private bool _pointerDownOverUI = false;

        private void Update()
        {
            if (UnityEngine.Input.touchSupported && UnityEngine.Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleMouseInput()
        {
            // Zoom via Mouse Scroll
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                OnZoom?.Invoke(-scroll * _mouseScrollSensitivity);
            }

            // Mouse Down
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _pointerDownOverUI = IsPointerOverUI();
                if (!_pointerDownOverUI)
                {
                    _pointerDownPosition = UnityEngine.Input.mousePosition;
                    _lastPointerPosition = _pointerDownPosition;
                    _isDragging = false;
                }
            }
            // Mouse Drag
            else if (UnityEngine.Input.GetMouseButton(0) && !_pointerDownOverUI)
            {
                Vector2 currentPos = UnityEngine.Input.mousePosition;
                Vector2 delta = currentPos - _lastPointerPosition;
                _lastPointerPosition = currentPos;

                if (!_isDragging && (currentPos - _pointerDownPosition).sqrMagnitude > _dragThresholdPixels * _dragThresholdPixels)
                {
                    _isDragging = true;
                }

                if (_isDragging)
                {
                    OnPan?.Invoke(-delta * _mousePanSensitivity);
                }
            }
            // Mouse Up
            else if (UnityEngine.Input.GetMouseButtonUp(0) && !_pointerDownOverUI)
            {
                if (!_isDragging)
                {
                    OnTap?.Invoke(UnityEngine.Input.mousePosition);
                }

                _isDragging = false;
                _pointerDownOverUI = false;
            }
        }

        private void HandleTouchInput()
        {
            int touchCount = UnityEngine.Input.touchCount;

            // Two Finger Pinch Zoom
            if (touchCount >= 2)
            {
                Touch touchZero = UnityEngine.Input.GetTouch(0);
                Touch touchOne = UnityEngine.Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                OnZoom?.Invoke(deltaMagnitudeDiff * _touchPinchSensitivity);
                _isDragging = true; // Prevent accidental single-finger drag on release
                return;
            }

            // Single Finger Pan & Tap
            if (touchCount == 1)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _pointerDownOverUI = IsPointerOverUI(touch.fingerId);
                    if (!_pointerDownOverUI)
                    {
                        _pointerDownPosition = touch.position;
                        _lastPointerPosition = touch.position;
                        _isDragging = false;
                    }
                }
                else if (touch.phase == TouchPhase.Moved && !_pointerDownOverUI)
                {
                    Vector2 delta = touch.deltaPosition;

                    if (!_isDragging && (touch.position - _pointerDownPosition).sqrMagnitude > _dragThresholdPixels * _dragThresholdPixels)
                    {
                        _isDragging = true;
                    }

                    if (_isDragging)
                    {
                        OnPan?.Invoke(-delta * _touchPanSensitivity);
                    }
                }
                else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && !_pointerDownOverUI)
                {
                    if (!_isDragging)
                    {
                        OnTap?.Invoke(touch.position);
                    }

                    _isDragging = false;
                    _pointerDownOverUI = false;
                }
            }
        }

        private bool IsPointerOverUI(int fingerId = -1)
        {
            if (EventSystem.current == null) return false;

            if (fingerId >= 0)
            {
                return EventSystem.current.IsPointerOverGameObject(fingerId);
            }

            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
