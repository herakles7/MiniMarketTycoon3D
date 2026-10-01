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
        [SerializeField] private float _pitchAngle = 45f;
        [SerializeField] private float _yawAngle = 0f;

        [Header("Camera Modes")]
        [SerializeField] private CameraMode _mode = CameraMode.FollowPlayer;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _playerFollowZoom = 8.5f;
        [SerializeField] private float _playerFollowPitch = 38f;
        [SerializeField] private float _tycoonPitch = 50f;

        public enum CameraMode { FollowPlayer, FreeTycoon }
        public CameraMode CurrentMode => _mode;

        [Header("Pan Settings")]
        [SerializeField] private float _panSpeed = 0.035f;
        [SerializeField] private float _panDamping = 10f;
        [SerializeField] private Vector2 _panBoundsX = new Vector2(-10f, 10f);
        [SerializeField] private Vector2 _panBoundsZ = new Vector2(-12f, 12f);

        [Header("Zoom Settings")]
        [SerializeField] private float _zoomSpeed = 1.2f;
        [SerializeField] private float _zoomDamping = 8f;
        [SerializeField] private float _minZoom = 6f;
        [SerializeField] private float _maxZoom = 20f;
        [SerializeField] private float _currentZoom = 11.31f;

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

            if (_targetCamera != null)
            {
                _targetCamera.allowMSAA = false; // Prevents "Disabling TAA because MSAA is on" console spam
            }

            _currentPivotPosition = new Vector3(0f, 0f, 2f);
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
            // Toggle Camera Mode via [C] key
            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                ToggleCameraMode();
            }

            if (_mode == CameraMode.FollowPlayer)
            {
                if (_playerTransform == null && PlayerManagerController.HasInstance)
                {
                    _playerTransform = PlayerManagerController.Instance.transform;
                }

                if (_playerTransform != null)
                {
                    _targetPosition = _playerTransform.position + Vector3.up * 0.9f;
                    _pitchAngle = Mathf.Lerp(_pitchAngle, _playerFollowPitch, Time.unscaledDeltaTime * 6f);
                    _targetZoom = _playerFollowZoom;
                }
            }
            else
            {
                _pitchAngle = Mathf.Lerp(_pitchAngle, _tycoonPitch, Time.unscaledDeltaTime * 6f);
            }

            // Smoothly interpolate pivot position
            _currentPivotPosition = Vector3.Lerp(_currentPivotPosition, _targetPosition, Time.unscaledDeltaTime * _panDamping);

            // Smoothly interpolate camera zoom distance
            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.unscaledDeltaTime * _zoomDamping);

            ApplyCameraTransform(false);
        }

        public void ToggleCameraMode()
        {
            SetCameraMode(_mode == CameraMode.FollowPlayer ? CameraMode.FreeTycoon : CameraMode.FollowPlayer);
        }

        public void SetCameraMode(CameraMode newMode)
        {
            _mode = newMode;
            if (_mode == CameraMode.FollowPlayer)
            {
                _targetZoom = _playerFollowZoom;
            }
            else
            {
                _targetZoom = 14f;
            }
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
            // If following player, dragging switches to Free Tycoon mode
            if (_mode == CameraMode.FollowPlayer)
            {
                if (screenDelta.sqrMagnitude > 25f)
                {
                    SetCameraMode(CameraMode.FreeTycoon);
                }
                return;
            }

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

        public Vector2 PanBoundsX => _panBoundsX;
        public Vector2 PanBoundsZ => _panBoundsZ;
        public float MaxZoom => _maxZoom;
        public float MinZoom => _minZoom;

        public void SetPanBounds(Vector2 boundsX, Vector2 boundsZ, float minZoom = 10f, float maxZoom = 28f)
        {
            _panBoundsX = boundsX;
            _panBoundsZ = boundsZ;
            _minZoom = minZoom;
            _maxZoom = maxZoom;
            _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _panBoundsX.x, _panBoundsX.y);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, _panBoundsZ.x, _panBoundsZ.y);
        }

        public void UpdateBoundsForLevel(int level)
        {
            switch (level)
            {
                case 1:
                    SetPanBounds(new Vector2(-6f, 6f), new Vector2(-8f, 7f), 10f, 22f);
                    break;
                case 2:
                    SetPanBounds(new Vector2(-6f, 12f), new Vector2(-8f, 7f), 10f, 25f);
                    break;
                case 3:
                    SetPanBounds(new Vector2(-6f, 13f), new Vector2(-8f, 11f), 10f, 27f);
                    break;
                case 4:
                    SetPanBounds(new Vector2(-13f, 13f), new Vector2(-8f, 11f), 10f, 30f);
                    break;
                case 5:
                default:
                    SetPanBounds(new Vector2(-14f, 14f), new Vector2(-8f, 16f), 10f, 34f);
                    break;
            }
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
