using System;
using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// First/Third-person direct controller for the supermarket manager (player avatar).
    /// Uses Unity's CharacterController for responsive, physical movement without jitter.
    /// Handles walking, sprinting, carrying delivery boxes, and interacting with shelves,
    /// cash registers, and the wholesale office computer.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerManagerController : MonoBehaviourSingleton<PlayerManagerController>
    {
        [Header("Movement Speeds")]
        [SerializeField] private float _walkSpeed = 3.6f;
        [SerializeField] private float _sprintSpeed = 5.8f;
        [SerializeField] private float _rotationSpeed = 12f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _gravity = -18f;

        [Header("Components & Transforms")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _carrySocket;
        [SerializeField] private ParticleSystem _footstepDust;

        [Header("Current State")]
        [SerializeField] private bool _isCarryingBox = false;
        [SerializeField] private bool _canMove = true;

        // Interaction
        private PlayerInteractionDetector _interactionDetector;
        private Vector3 _velocity;
        private float _currentSpeed;
        private float _verticalVelocity;
        private bool _isGrounded;

        // Held Carriable Object
        private ICarriable _carriedItem;

        public bool IsCarryingBox => _isCarryingBox;
        public ICarriable CarriedItem => _carriedItem;
        public Transform CarrySocket => _carrySocket;
        public bool CanMove
        {
            get => _canMove;
            set
            {
                _canMove = value;
                if (!_canMove)
                {
                    _currentSpeed = 0f;
                    if (_animator != null)
                    {
                        _animator.SetFloat("Speed", 0f);
                        _animator.SetBool("IsWalking", false);
                    }
                }
            }
        }

        public event Action<bool> OnSprintToggled;
        public event Action<ICarriable> OnCarriedItemChanged;

        private void Reset()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }

        protected override void OnInitialized()
        {
            if (_characterController == null) _characterController = GetComponent<CharacterController>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            _interactionDetector = GetComponent<PlayerInteractionDetector>();

            // Setup CharacterController dimensions tailored for Kenney characters
            if (_characterController != null)
            {
                _characterController.height = 1.4f;
                _characterController.radius = 0.3f;
                _characterController.center = new Vector3(0f, 0.7f, 0f);
                _characterController.stepOffset = 0.25f;
                _characterController.slopeLimit = 45f;
            }

            EnsureCarrySocket();
        }

        private void EnsureCarrySocket()
        {
            if (_carrySocket == null)
            {
                Transform existing = transform.Find("CarrySocket");
                if (existing != null)
                {
                    _carrySocket = existing;
                }
                else
                {
                    GameObject go = new GameObject("CarrySocket");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = new Vector3(0f, 0.65f, 0.45f);
                    _carrySocket = go.transform;
                }
            }
        }

        private void Update()
        {
            HandleGroundAndGravity();

            if (_canMove)
            {
                HandleMovement();
            }

            HandleInteractionInput();
        }

        private void HandleGroundAndGravity()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f; // Slight downward push to maintain ground contact
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
        }

        private void HandleMovement()
        {
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

            // Camera-relative input direction
            Vector3 moveDirection = Vector3.zero;
            Camera mainCam = Camera.main;
            if (mainCam != null && inputDir.sqrMagnitude > 0.001f)
            {
                Vector3 camForward = mainCam.transform.forward;
                Vector3 camRight = mainCam.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDirection = (camForward * inputDir.z + camRight * inputDir.x).normalized;
            }

            bool isSprinting = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
            float targetSpeed = inputDir.sqrMagnitude > 0.01f ? (isSprinting ? _sprintSpeed : _walkSpeed) : 0f;

            // Smooth acceleration
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _acceleration * Time.deltaTime);

            // Rotate smoothly towards movement direction
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
            }

            // Move character
            Vector3 finalMove = moveDirection * _currentSpeed;
            finalMove.y = _verticalVelocity;
            _characterController.Move(finalMove * Time.deltaTime);

            // Animator parameter sync
            if (_animator != null)
            {
                float normalizedSpeed = _currentSpeed / _sprintSpeed;
                _animator.SetFloat("Speed", normalizedSpeed);
                _animator.SetBool("IsWalking", _currentSpeed > 0.1f);
                _animator.SetBool("WalkFast", isSprinting && _currentSpeed > _walkSpeed);
            }

            // Footstep dust
            if (_footstepDust != null)
            {
                if (_currentSpeed > 0.2f && !_footstepDust.isPlaying)
                {
                    _footstepDust.Play();
                }
                else if (_currentSpeed <= 0.2f && _footstepDust.isPlaying)
                {
                    _footstepDust.Stop();
                }
            }
        }

        private void HandleInteractionInput()
        {
            if (_interactionDetector == null) return;

            var currentTarget = _interactionDetector.CurrentTarget;
            if (currentTarget == null || !currentTarget.CanInteract(this)) return;

            // Single Press: [E] or Left Click
            if (UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                currentTarget.OnInteract(this);
            }

            // Continuous Hold: [E] or Space
            if (UnityEngine.Input.GetKey(KeyCode.E) || UnityEngine.Input.GetKey(KeyCode.Space))
            {
                currentTarget.OnHoldInteract(this, Time.deltaTime);
            }
        }

        public void PickUpItem(ICarriable item)
        {
            if (item == null) return;

            _carriedItem = item;
            _isCarryingBox = true;
            EnsureCarrySocket();
            item.AttachToSocket(_carrySocket);

            OnCarriedItemChanged?.Invoke(item);
        }

        public ICarriable DropCarriedItem()
        {
            if (_carriedItem == null) return null;

            var item = _carriedItem;
            _carriedItem = null;
            _isCarryingBox = false;
            item.DetachFromSocket();

            OnCarriedItemChanged?.Invoke(null);
            return item;
        }

        public void ClearCarriedItem()
        {
            _carriedItem = null;
            _isCarryingBox = false;
            OnCarriedItemChanged?.Invoke(null);
        }
    }

    /// <summary>
    /// Interface for physical items that can be picked up and held by the player (like boxes, trash, tools).
    /// </summary>
    public interface ICarriable
    {
        string ItemName { get; }
        void AttachToSocket(Transform socket);
        void DetachFromSocket();
    }
}
