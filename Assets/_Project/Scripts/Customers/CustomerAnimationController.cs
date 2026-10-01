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

        [SerializeField] private CustomerNavigation _navigation;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
        private static readonly int WalkHash = Animator.StringToHash("Walk");
        private static readonly int WalkFastHash = Animator.StringToHash("WalkFast");
        private static readonly int IsShoppingHash = Animator.StringToHash("IsShopping");
        private static readonly int PickupHash = Animator.StringToHash("Pickup");
        private static readonly int IsWaitingHash = Animator.StringToHash("IsWaiting");
        private static readonly int QueueIdleHash = Animator.StringToHash("QueueIdle");
        private static readonly int IsCheckingOutHash = Animator.StringToHash("IsCheckingOut");
        private static readonly int PayHash = Animator.StringToHash("Pay");
        private static readonly int LeaveHash = Animator.StringToHash("Leave");

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
            if (_navigation == null)
            {
                _navigation = GetComponent<CustomerNavigation>();
            }
            CacheRestingRotations();
        }

        private Quaternion _naturalLeftArmRot = Quaternion.identity;
        private Quaternion _naturalRightArmRot = Quaternion.identity;

        private void CacheRestingRotations()
        {
            if (_torsoBone != null) _initTorsoRot = _torsoBone.localRotation;
            if (_headBone != null) _initHeadRot = _headBone.localRotation;
            if (_leftArmBone != null)
            {
                _initLeftArmRot = _leftArmBone.localRotation;
                // Kenney FBX rests in T-pose (arms horizontal). Rotate down ~68 deg along torso for natural human rest pose.
                _naturalLeftArmRot = _initLeftArmRot * Quaternion.Euler(0f, 0f, -68f);
            }
            if (_rightArmBone != null)
            {
                _initRightArmRot = _rightArmBone.localRotation;
                _naturalRightArmRot = _initRightArmRot * Quaternion.Euler(0f, 0f, 68f);
            }
            if (_leftLegBone != null) _initLeftLegRot = _leftLegBone.localRotation;
            if (_rightLegBone != null) _initRightLegRot = _rightLegBone.localRotation;
        }

        // Safe set helpers — no crash if parameter doesn't exist in current controller
        private void SafeSetBool(int hash, bool value)
        {
            if (_animator == null) return;
            foreach (var p in _animator.parameters)
                if (p.nameHash == hash) { _animator.SetBool(hash, value); return; }
        }
        private void SafeSetFloat(int hash, float value)
        {
            if (_animator == null) return;
            foreach (var p in _animator.parameters)
                if (p.nameHash == hash) { _animator.SetFloat(hash, value); return; }
        }
        private void SafeSetTrigger(int hash)
        {
            if (_animator == null) return;
            foreach (var p in _animator.parameters)
                if (p.nameHash == hash) { _animator.SetTrigger(hash); return; }
        }

        public void SetState(CustomerState state)
        {
            _currentState = state;
            _actionCycleTime = 0f;

            if (_animator != null)
            {
                bool isWalking = state == CustomerState.Entering || state == CustomerState.GoingToShelf || state == CustomerState.GoingToCheckout || state == CustomerState.Leaving;
                bool isFast = isWalking && (_personalityType == CustomerPersonalityType.QuickShopper || _personalityType == CustomerPersonalityType.ImpatientShopper);

                SafeSetBool(IsWalkingHash, isWalking);
                SafeSetBool(WalkHash, isWalking);
                SafeSetBool(WalkFastHash, isFast);
                SafeSetBool(IsShoppingHash, state == CustomerState.Shopping);
                SafeSetBool(IsWaitingHash, state == CustomerState.WaitingInQueue);
                SafeSetBool(QueueIdleHash, state == CustomerState.WaitingInQueue);
                SafeSetBool(IsCheckingOutHash, state == CustomerState.CheckingOut);

                if (state == CustomerState.Shopping) SafeSetTrigger(PickupHash);
                if (state == CustomerState.CheckingOut) SafeSetTrigger(PayHash);
                if (state == CustomerState.Leaving) SafeSetTrigger(LeaveHash);
            }
        }

        public void UpdateMovement(float speed)
        {
            _currentSpeed = speed;
            if (_animator != null)
            {
                bool isMoving = speed > 0.08f;
                SafeSetFloat(SpeedHash, speed);
                SafeSetBool(IsWalkingHash, isMoving);
                SafeSetBool(WalkHash, isMoving);
            }
        }


        private void Update()
        {
            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
            if (_navigation == null)
            {
                _navigation = GetComponent<CustomerNavigation>();
            }

            float currentSpd = _navigation != null ? _navigation.CurrentSpeed : _currentSpeed;
            bool isMoving = _navigation != null ? (_navigation.IsMoving || currentSpd > 0.08f) : currentSpd > 0.08f;

            if (_animator != null)
            {
                SafeSetFloat(SpeedHash, currentSpd);
                SafeSetBool(IsWalkingHash, isMoving);
                SafeSetBool(WalkHash, isMoving);

                // If animator is actively controlling mesh with a controller, avoid overriding transforms
                if (_animator.runtimeAnimatorController != null && _animator.isActiveAndEnabled)
                {
                    return;
                }
            }


            // Procedural articulated animation fallback
            AnimateProceduralRig();
        }

        private float _heightMultiplier = 1.0f;

        public void SetHeightMultiplier(float height)
        {
            _heightMultiplier = Mathf.Clamp(height, 0.85f, 1.15f);
        }

        public void ResetToNeutral()
        {
            _currentState = CustomerState.Idle;
            _walkCycleTime = 0f;
            _actionCycleTime = 0f;
            _currentSpeed = 0f;
            _currentHeadYaw = 0f;
            _headGlanceTargetYaw = 0f;
            _weightShiftPhase = 0f;
            _heightMultiplier = 1.0f;

            if (_leftLegBone != null) _leftLegBone.localRotation = _initLeftLegRot;
            if (_rightLegBone != null) _rightLegBone.localRotation = _initRightLegRot;
            if (_leftArmBone != null) _leftArmBone.localRotation = _initLeftArmRot;
            if (_rightArmBone != null) _rightArmBone.localRotation = _initRightArmRot;
            if (_torsoBone != null) _torsoBone.localRotation = _initTorsoRot;
            if (_headBone != null) _headBone.localRotation = _initHeadRot;

            if (_animator != null)
            {
                _animator.SetBool(IsWalkingHash, false);
                _animator.SetBool(WalkFastHash, false);
                _animator.SetBool(IsShoppingHash, false);
                _animator.SetBool(IsWaitingHash, false);
                _animator.SetBool(QueueIdleHash, false);
                _animator.SetBool(IsCheckingOutHash, false);
                _animator.SetFloat(SpeedHash, 0f);
            }
        }

        private void AnimateProceduralRig()
        {
            float dt = Time.deltaTime;
            bool isMoving = _currentSpeed > 0.1f;

            // Natural glance timer update
            UpdateHeadGlance(dt);

            if (isMoving)
            {
                // Synchronized stride kinematics (calibrated stride to avoid foot sliding - Section 9 & 11)
                float strideLength = 0.65f * _heightMultiplier;
                float strideFreq = (_currentSpeed / Mathf.Max(0.1f, strideLength)) * Mathf.PI;

                float personalityFreqMult = 1.0f;
                if (_personalityType == CustomerPersonalityType.QuickShopper) personalityFreqMult = 1.15f;
                else if (_personalityType == CustomerPersonalityType.ImpatientShopper) personalityFreqMult = 1.10f;
                else if (_personalityType == CustomerPersonalityType.PatientShopper) personalityFreqMult = 0.95f;

                _walkCycleTime += dt * strideFreq * personalityFreqMult;
                float legAngle = Mathf.Sin(_walkCycleTime) * 26f;
                float armAngle = -legAngle * (_isFemale ? 0.75f : 0.85f);

                // Female subtle hip sway vs Male firmer shoulder sway
                float torsoRoll = _isFemale ? Mathf.Sin(_walkCycleTime) * 4f : 0f;
                float torsoYaw = _isFemale ? Mathf.Sin(_walkCycleTime) * 2f : Mathf.Sin(_walkCycleTime) * 3.5f;

                if (_leftLegBone != null) _leftLegBone.localRotation = _initLeftLegRot * Quaternion.Euler(legAngle, 0f, 0f);
                if (_rightLegBone != null) _rightLegBone.localRotation = _initRightLegRot * Quaternion.Euler(-legAngle, 0f, 0f);
                if (_leftArmBone != null) _leftArmBone.localRotation = _naturalLeftArmRot * Quaternion.Euler(armAngle, 0f, _isFemale ? -4f : 0f);
                if (_rightArmBone != null) _rightArmBone.localRotation = _naturalRightArmRot * Quaternion.Euler(-armAngle, 0f, _isFemale ? 4f : 0f);
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
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _naturalRightArmRot * Quaternion.Euler(-reachAngle, 15f, 0f), dt * 6f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _naturalLeftArmRot * Quaternion.Euler(15f, -10f, 0f), dt * 6f);
                        if (_torsoBone != null) _torsoBone.localRotation = Quaternion.Slerp(_torsoBone.localRotation, _initTorsoRot * Quaternion.Euler(10f, 5f, 0f), dt * 4f);
                        if (_headBone != null) _headBone.localRotation = Quaternion.Slerp(_headBone.localRotation, _initHeadRot * Quaternion.Euler(-12f, _currentHeadYaw * 0.5f, 0f), dt * 4f);
                        break;

                    case CustomerState.CheckingOut:
                        // Arm gesture towards POS / cash payment
                        _actionCycleTime += dt * 3f;
                        float payAngle = Mathf.PingPong(_actionCycleTime * 30f, 45f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _naturalRightArmRot * Quaternion.Euler(-payAngle - 25f, -15f, 0f), dt * 8f);
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _naturalLeftArmRot, dt * 6f);
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
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _naturalLeftArmRot * Quaternion.Euler(0f, 0f, 2f), dt * 4f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _naturalRightArmRot * Quaternion.Euler(0f, 0f, -2f), dt * 4f);
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
                        if (_leftArmBone != null) _leftArmBone.localRotation = Quaternion.Slerp(_leftArmBone.localRotation, _naturalLeftArmRot * Quaternion.Euler(0f, 0f, 2f), dt * 4f);
                        if (_rightArmBone != null) _rightArmBone.localRotation = Quaternion.Slerp(_rightArmBone.localRotation, _naturalRightArmRot * Quaternion.Euler(0f, 0f, -2f), dt * 4f);
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
