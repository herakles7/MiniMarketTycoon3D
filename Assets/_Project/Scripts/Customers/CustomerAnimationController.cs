using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Dual-mode animation controller for customer NPCs.
    /// Drives standard Animator parameters if an Animator is present,
    /// and provides procedural articulated limb animation (walking gait, reaching, shopping, waiting)
    /// to guarantee realistic visual movement without requiring external humanoid animation assets.
    /// </summary>
    public class CustomerAnimationController : MonoBehaviour
    {
        [Header("Animator Support")]
        [SerializeField] private Animator _animator;

        [Header("Procedural Rig References")]
        [SerializeField] private Transform _torsoBone;
        [SerializeField] private Transform _headBone;
        [SerializeField] private Transform _leftArmBone;
        [SerializeField] private Transform _rightArmBone;
        [SerializeField] private Transform _leftLegBone;
        [SerializeField] private Transform _rightLegBone;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int IsShoppingHash = Animator.StringToHash("IsShopping");
        private static readonly int IsWaitingHash = Animator.StringToHash("IsWaiting");
        private static readonly int IsCheckingOutHash = Animator.StringToHash("IsCheckingOut");

        private CustomerState _currentState = CustomerState.Idle;
        private float _walkCycleTime;
        private float _actionCycleTime;
        private float _currentSpeed;

        // Base resting local rotations for procedural limbs
        private Quaternion _initTorsoRot = Quaternion.identity;
        private Quaternion _initHeadRot = Quaternion.identity;
        private Quaternion _initLeftArmRot = Quaternion.identity;
        private Quaternion _initRightArmRot = Quaternion.identity;
        private Quaternion _initLeftLegRot = Quaternion.identity;
        private Quaternion _initRightLegRot = Quaternion.identity;

        public void BindProceduralBones(Transform torso, Transform head, Transform leftArm, Transform rightArm, Transform leftLeg, Transform rightLeg)
        {
            _torsoBone = torso;
            _headBone = head;
            _leftArmBone = leftArm;
            _rightArmBone = rightArm;
            _leftLegBone = leftLeg;
            _rightLegBone = rightLeg;

            CacheRestingRotations();
        }

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            CacheRestingRotations();
        }

        private void CacheRestingRotations()
        {
            if (_torsoBone != null) _initTorsoRot = _torsoBone.localRotation;
            if (_headBone != null) _initHeadRot = _headBone.localRotation;
            if (_leftArmBone != null) _initLeftArmRot = _leftArmBone.localRotation;
            if (_rightArmBone != null) _initRightArmRot = _rightArmBone.localRotation;
            if (_leftLegBone != null) _initLeftLegRot = _leftLegBone.localRotation;
            if (_rightLegBone != null) _initRightLegRot = _rightLegBone.localRotation;
        }

        public void SetState(CustomerState state)
        {
            _currentState = state;
            _actionCycleTime = 0f;

            if (_animator != null)
            {
                _animator.SetBool(IsWalkingHash, state == CustomerState.Entering || state == CustomerState.GoingToShelf || state == CustomerState.GoingToCheckout || state == CustomerState.Leaving);
                _animator.SetBool(IsShoppingHash, state == CustomerState.Shopping);
                _animator.SetBool(IsWaitingHash, state == CustomerState.WaitingInQueue);
                _animator.SetBool(IsCheckingOutHash, state == CustomerState.CheckingOut);
            }
        }

        public void UpdateMovement(float speed)
        {
            _currentSpeed = speed;
            if (_animator != null)
            {
                _animator.SetFloat(SpeedHash, speed);
            }
        }

        private void Update()
        {
            // If animator is controlling mesh, do not overwrite transforms
            if (_animator != null && _animator.isInitialized && _animator.runtimeAnimatorController != null)
            {
                return;
            }

            // Procedural articulated animation fallback
            AnimateProceduralRig();
        }

        private void AnimateProceduralRig()
        {
            float dt = Time.deltaTime;
            bool isMoving = _currentSpeed > 0.1f;

            if (isMoving)
            {
                // Walking Gait: Alternating leg and arm swings, torso slight bounce
                _walkCycleTime += dt * _currentSpeed * 6.5f;
                float legAngle = Mathf.Sin(_walkCycleTime) * 26f;
                float armAngle = -legAngle * 0.85f;
                float bodyBounce = Mathf.Abs(Mathf.Sin(_walkCycleTime * 2f)) * 0.03f;

                if (_leftLegBone != null) _leftLegBone.localRotation = _initLeftLegRot * Quaternion.Euler(legAngle, 0f, 0f);
                if (_rightLegBone != null) _rightLegBone.localRotation = _initRightLegRot * Quaternion.Euler(-legAngle, 0f, 0f);
                if (_leftArmBone != null) _leftArmBone.localRotation = _initLeftArmRot * Quaternion.Euler(armAngle, 0f, 0f);
                if (_rightArmBone != null) _rightArmBone.localRotation = _initRightArmRot * Quaternion.Euler(-armAngle, 0f, 0f);
                if (_torsoBone != null) _torsoBone.localRotation = _initTorsoRot * Quaternion.Euler(4f, Mathf.Sin(_walkCycleTime) * 3f, 0f);
                if (_headBone != null) _headBone.localRotation = _initHeadRot * Quaternion.Euler(-3f, 0f, 0f);
            }
            else
            {
                // Reset legs smoothly to resting stand
                if (_leftLegBone != null) _leftLegBone.localRotation = Quaternion.Slerp(_leftLegBone.localRotation, _initLeftLegRot, dt * 8f);
                if (_rightLegBone != null) _rightLegBone.localRotation = Quaternion.Slerp(_rightLegBone.localRotation, _initRightLegRot, dt * 8f);

                switch (_currentState)
                {
                    case CustomerState.Shopping:
                        // Reaching right arm forward to inspect/grab items from shelf
                        _actionCycleTime += dt * 2.5f;
                        float reachAngle = Mathf.Clamp(Mathf.Sin(_actionCycleTime) * 60f + 20f, -20f, 75f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _initRightArmRot * Quaternion.Euler(-reachAngle, 15f, 0f), dt * 6f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _initLeftArmRot * Quaternion.Euler(15f, -10f, 0f), dt * 6f);
                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot * Quaternion.Euler(10f, 5f, 0f), dt * 4f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(-12f, 0f, 0f), dt * 4f);
                        break;

                    case CustomerState.CheckingOut:
                        // Arm gesture towards POS / cash payment
                        _actionCycleTime += dt * 3f;
                        float payAngle = Mathf.PingPong(_actionCycleTime * 30f, 45f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _initRightArmRot * Quaternion.Euler(-payAngle - 25f, -15f, 0f), dt * 8f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _initLeftArmRot, dt * 6f);
                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot, dt * 6f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(5f, 0f, 0f), dt * 6f);
                        break;

                    case CustomerState.WaitingInQueue:
                    case CustomerState.Idle:
                    default:
                        // Subtle breathing & natural standing sway
                        _actionCycleTime += dt * 1.5f;
                        float breathe = Mathf.Sin(_actionCycleTime) * 2f;
                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot * Quaternion.Euler(breathe * 0.5f, 0f, 0f), dt * 4f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(-breathe * 0.3f, 0f, 0f), dt * 4f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _initLeftArmRot * Quaternion.Euler(0f, 0f, 2f), dt * 4f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _initRightArmRot * Quaternion.Euler(0f, 0f, -2f), dt * 4f);
                        break;
                }
            }
        }
    }
}
