using UnityEngine;
using MiniMarketTycoon.Input;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Hybrid isometric/third-person camera controller tailored for portrait mobile store management.
    /// Provides smooth pan, zoom, and bounding box restrictions to keep the supermarket in frame.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Rig References")]
        [SerializeField] private Transform _targetPivot;
        [SerializeField] private Camera _targetCamera;

        [Header("Viewing Angle")]
        [SerializeField] private float _pitchAngle = 48f;
        [SerializeField] private float _yawAngle = 30f;

        [Header("Pan Settings")]
        [SerializeField] private float _panSpeed = 0.035f;
        [SerializeField] private float _panDamping = 10f;
        [SerializeField] private Vector2 _panBoundsX = new Vector2(-10f, 10f);
        [SerializeField] private Vector2 _panBoundsZ = new Vector2(-12f, 12f);

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSpeed = 1.2f;
        [SerializeField] private float _zoomDamping = 8f;
        [SerializeField] private float _minZoom = 10f;
        [SerializeField] private float _maxZoom = 28f;
        [SerializeField] private float _currentZoom = 18f;

        private Vector3 _currentPivotPosition;
        private Vector3 _targetPosition;
        private float _targetZoom;

        private void Awake()
        {
            if (_targetCamera == null)
            {
                _targetCamera = GetComponentInChildren<Camera>();
                if (_targetCamera == null)
                {
                    _targetCamera = Camera.main;
                }
            }

            _currentPivotPosition = Vector3.zero;
            _targetPosition = _currentPivotPosition;
            _targetZoom = _currentZoom;

            ApplyCameraTransform(true);
        }

        private void Start()
        {
            SubscribeInput();
        }

        private void SubscribeInput()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnPan -= HandlePan;
                InputManager.Instance.OnPan += HandlePan;
                InputManager.Instance.OnZoom -= HandleZoom;
                InputManager.Instance.OnZoom += HandleZoom;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.HasInstance)
            {
                InputManager.Instance.OnPan -= HandlePan;
                InputManager.Instance.OnZoom -= HandleZoom;
            }
        }

        private void LateUpdate()
        {
            // Smoothly interpolate pivot position
            _currentPivotPosition = Vector3.Lerp(_currentPivotPosition, _targetPosition, Time.unscaledDeltaTime * _panDamping);

            // Smoothly interpolate camera zoom distance
            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.unscaledDeltaTime * _zoomDamping);

            ApplyCameraTransform(false);
        }

        private void ApplyCameraTransform(bool instant)
        {
            if (_targetCamera == null) return;

            Vector3 cameraOffset = Quaternion.Euler(_pitchAngle, _yawAngle, 0f) * new Vector3(0f, 0f, -_currentZoom);
            _targetCamera.transform.position = _currentPivotPosition + cameraOffset;
            _targetCamera.transform.rotation = Quaternion.Euler(_pitchAngle, _yawAngle, 0f);

            if (_targetPivot != null && _targetPivot != _targetCamera.transform)
            {
                _targetPivot.position = _currentPivotPosition;
                _targetPivot.rotation = Quaternion.Euler(0f, _yawAngle, 0f);
            }
        }

        private void HandlePan(Vector2 screenDelta)
        {
            Vector3 forward = Quaternion.Euler(0f, _yawAngle, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, _yawAngle, 0f) * Vector3.right;

            Vector3 moveDelta = (right * screenDelta.x + forward * screenDelta.y) * _panSpeed * (_currentZoom / _maxZoom);
            _targetPosition += moveDelta;

            // Clamp within store bounds
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _panBoundsX.x, _panBoundsX.y);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, _panBoundsZ.x, _panBoundsZ.y);
        }

        private void HandleZoom(float zoomDelta)
        {
            _targetZoom = Mathf.Clamp(_targetZoom + zoomDelta * _zoomSpeed, _minZoom, _maxZoom);
        }

        public void FocusOn(Vector3 targetWorldPosition)
        {
            _targetPosition = new Vector3(
                Mathf.Clamp(targetWorldPosition.x, _panBoundsX.x, _panBoundsX.y),
                0f,
                Mathf.Clamp(targetWorldPosition.z, _panBoundsZ.x, _panBoundsZ.y)
            );
        }
    }
}
