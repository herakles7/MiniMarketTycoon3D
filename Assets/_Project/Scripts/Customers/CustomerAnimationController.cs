using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Dual-mode animation controller for customer NPCs.
    /// Drives standard Animator parameters if an Animator is present,
    /// and provides procedural articulated limb animation (walking gait, reaching, shopping, waiting, idle variations)
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

        // Personality & Gender traits for animation
        private bool _isFemale;
        private CustomerPersonalityType _personalityType = CustomerPersonalityType.Normal;
        private float _headGlanceTimer;
        private float _headGlanceTargetYaw;
        private float _currentHeadYaw;
        private float _weightShiftPhase;

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

        public void SetGender(CustomerGender gender)
        {
            _isFemale = gender == CustomerGender.Female;
        }

        public void SetPersonality(CustomerPersonalityType personality)
        {
            _personalityType = personality;
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
            if (_rightArmBone != null) _rightArmBone.localRotation = _initRightArmRot;
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
            // If animator is actively controlling mesh with a controller, avoid overriding transforms
            if (_animator != null && _animator.isInitialized && _animator.runtimeAnimatorController != null)
            {
                return;
            }

            // Procedural articulated animation
            AnimateProceduralRig();
        }

        private void AnimateProceduralRig()
        {
            float dt = Time.deltaTime;
            bool isMoving = _currentSpeed > 0.1f;

            // Natural glance timer update
            UpdateHeadGlance(dt);

            if (isMoving)
            {
                // Personality gait multiplier
                float gaitSpeedMultiplier = 6.5f;
                if (_personalityType == CustomerPersonalityType.QuickShopper) gaitSpeedMultiplier = 7.5f;
                else if (_personalityType == CustomerPersonalityType.ImpatientShopper) gaitSpeedMultiplier = 7.0f;
                else if (_personalityType == CustomerPersonalityType.PatientShopper) gaitSpeedMultiplier = 5.8f;

                _walkCycleTime += dt * _currentSpeed * gaitSpeedMultiplier;
                float legAngle = Mathf.Sin(_walkCycleTime) * 26f;
                float armAngle = -legAngle * (_isFemale ? 0.75f : 0.85f);

                // Female subtle hip sway vs Male firmer shoulder sway
                float torsoRoll = _isFemale ? Mathf.Sin(_walkCycleTime) * 4f : 0f;
                float torsoYaw = _isFemale ? Mathf.Sin(_walkCycleTime) * 2f : Mathf.Sin(_walkCycleTime) * 3.5f;

                if (_leftLegBone != null) _leftLegBone.localRotation = _initLeftLegRot * Quaternion.Euler(legAngle, 0f, 0f);
                if (_rightLegBone != null) _rightLegBone.localRotation = _initRightLegRot * Quaternion.Euler(-legAngle, 0f, 0f);
                if (_leftArmBone != null) _leftArmBone.localRotation = _initLeftArmRot * Quaternion.Euler(armAngle, 0f, _isFemale ? -4f : 0f);
                if (_rightArmBone != null) _rightArmBone.localRotation = _initRightArmRot * Quaternion.Euler(-armAngle, 0f, _isFemale ? 4f : 0f);
                if (_torsoBone != null) _torsoBone.localRotation = _initTorsoRot * Quaternion.Euler(4f, torsoYaw, torsoRoll);
                if (_headBone != null) _headBone.localRotation = _initHeadRot * Quaternion.Euler(-3f, _currentHeadYaw * 0.3f, 0f);
            }
            else
            {
                // Smoothly return legs to resting stand
                if (_leftLegBone != null) _leftLegBone.localRotation = Quaternion.Slerp(_leftLegBone.localRotation, _initLeftLegRot, dt * 8f);
                if (_rightLegBone != null) _rightLegBone.localRotation = Quaternion.Slerp(_rightLegBone.localRotation, _initRightLegRot, dt * 8f);

                switch (_currentState)
                {
                    case CustomerState.Shopping:
                        // Reaching right arm forward to inspect/grab items from shelf
                        float reachSpeed = (_personalityType == CustomerPersonalityType.QuickShopper) ? 3.8f : 2.5f;
                        _actionCycleTime += dt * reachSpeed;
                        float reachAngle = Mathf.Clamp(Mathf.Sin(_actionCycleTime) * 60f + 20f, -20f, 75f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _initRightArmRot * Quaternion.Euler(-reachAngle, 15f, 0f), dt * 6f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _initLeftArmRot * Quaternion.Euler(15f, -10f, 0f), dt * 6f);
                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot * Quaternion.Euler(10f, 5f, 0f), dt * 4f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(-12f, _currentHeadYaw * 0.5f, 0f), dt * 4f);
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
                        // Impatient shoppers fidget and look around more often
                        float fidgetRate = (_personalityType == CustomerPersonalityType.ImpatientShopper) ? 3.0f : 1.2f;
                        _actionCycleTime += dt * fidgetRate;
                        _weightShiftPhase += dt * (_personalityType == CustomerPersonalityType.ImpatientShopper ? 1.5f : 0.6f);

                        float queueShift = Mathf.Sin(_weightShiftPhase) * (_personalityType == CustomerPersonalityType.ImpatientShopper ? 4f : 1.5f);
                        float breatheQ = Mathf.Sin(_actionCycleTime) * 2f;

                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot * Quaternion.Euler(breatheQ * 0.5f, queueShift * 0.5f, queueShift), dt * 4f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(-breatheQ * 0.3f, _currentHeadYaw, 0f), dt * 5f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _initLeftArmRot * Quaternion.Euler(0f, 0f, 2f), dt * 4f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _initRightArmRot * Quaternion.Euler(0f, 0f, -2f), dt * 4f);
                        break;

                    case CustomerState.Idle:
                    default:
                        // Subtle breathing & natural standing weight shift
                        _actionCycleTime += dt * 1.5f;
                        _weightShiftPhase += dt * 0.5f;
                        float shift = Mathf.Sin(_weightShiftPhase) * 2.0f;
                        float breathe = Mathf.Sin(_actionCycleTime) * 2f;

                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot * Quaternion.Euler(breathe * 0.5f, 0f, shift), dt * 3f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(-breathe * 0.3f, _currentHeadYaw, 0f), dt * 4f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _initLeftArmRot * Quaternion.Euler(0f, 0f, 2f), dt * 4f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _initRightArmRot * Quaternion.Euler(0f, 0f, -2f), dt * 4f);
                        break;
                }
            }
        }

        private void UpdateHeadGlance(float dt)
        {
            _headGlanceTimer -= dt;
            if (_headGlanceTimer <= 0f)
            {
                // Reset glance interval: Impatient glances around more often
                float minInterval = _personalityType == CustomerPersonalityType.ImpatientShopper ? 1.5f : 3.5f;
                float maxInterval = _personalityType == CustomerPersonalityType.ImpatientShopper ? 3.5f : 7.0f;
                _headGlanceTimer = Random.Range(minInterval, maxInterval);

                // Chance to look left, right, or straight ahead
                float roll = Random.value;
                if (roll < 0.35f) _headGlanceTargetYaw = Random.Range(-25f, -10f); // Look left
                else if (roll < 0.70f) _headGlanceTargetYaw = Random.Range(10f, 25f); // Look right
                else _headGlanceTargetYaw = 0f; // Look forward
            }

            _currentHeadYaw = Mathf.Lerp(_currentHeadYaw, _headGlanceTargetYaw, dt * 3.5f);
        }
    }
}
