using System;
using UnityEngine;
using UnityEngine.AI;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Encapsulates NavMeshAgent navigation, smooth trajectory interpolation,
    /// dynamic facing orientation, and stuck detection & recovery.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CustomerNavigation : MonoBehaviour
    {
        [Header("Agent Configuration")]
        [SerializeField] private float _stoppingDistance = 0.2f;
        [SerializeField] private float _turnSpeed = 480f;
        [SerializeField] private float _acceleration = 12f;
        [SerializeField] private float _stuckThresholdTime = 3.5f;

        private NavMeshAgent _agent;
        private Action _onDestinationReached;
        private bool _hasActiveDestination;
        private Vector3 _currentDestination;

        // Facing orientation when stopped
        private bool _isAligningFacing;
        private Vector3 _targetFacingDirection;

        // Stuck detection metrics
        private float _stuckTimer;
        private Vector3 _lastSamplePosition;
        private Action _onStuckCallback;

        public bool IsMoving => _agent != null && _agent.enabled && _agent.velocity.sqrMagnitude > 0.04f;
        public float CurrentSpeed => _agent != null && _agent.enabled ? _agent.velocity.magnitude : 0f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            ConfigureAgent();
        }

        private void ConfigureAgent()
        {
            if (_agent == null) return;
            _agent.stoppingDistance = _stoppingDistance;
            _agent.angularSpeed = _turnSpeed;
            _agent.acceleration = _acceleration;
            _agent.autoBraking = true;
            _agent.radius = 0.3f;
            _agent.height = 1.75f;
            _agent.avoidancePriority = UnityEngine.Random.Range(30, 70);
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        }

        public void SetSpeed(float speed)
        {
            if (_agent != null)
            {
                _agent.speed = speed;
            }
        }

        public void WarpTo(Vector3 position)
        {
            if (_agent == null) return;
            if (_agent.isOnNavMesh)
            {
                _agent.Warp(position);
            }
            else
            {
                transform.position = position;
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                {
                    _agent.Warp(hit.position);
                }
            }
            _hasActiveDestination = false;
            _isAligningFacing = false;
            _stuckTimer = 0f;
        }

        public void MoveTo(Vector3 destination, Action onReached = null, Action onStuck = null)
        {
            _onDestinationReached = onReached;
            _onStuckCallback = onStuck;
            _currentDestination = destination;
            _hasActiveDestination = true;
            _isAligningFacing = false;
            _stuckTimer = 0f;
            _lastSamplePosition = transform.position;

            if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;

            // Ensure destination is on NavMesh
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
            else
            {
                _agent.SetDestination(destination);
            }

            _agent.isStopped = false;
        }

        public void Stop()
        {
            _hasActiveDestination = false;
            _isAligningFacing = false;
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }
        }

        public void AlignFacing(Vector3 forwardDirection, float duration = 0.5f)
        {
            if (forwardDirection.sqrMagnitude < 0.01f) return;
            forwardDirection.y = 0f;
            _targetFacingDirection = forwardDirection.normalized;
            _isAligningFacing = true;
        }

        private void Update()
        {
            if (!_hasActiveDestination && !_isAligningFacing) return;

            // 1. Destination Arrival Check
            if (_hasActiveDestination && _agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                if (!_agent.pathPending)
                {
                    if (_agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
                    {
                        if (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.02f)
                        {
                            _hasActiveDestination = false;
                            _agent.isStopped = true;
                            _onDestinationReached?.Invoke();
                            _onDestinationReached = null;
                            return;
                        }
                    }
                }

                // 2. Stuck Detection Check
                float movedDistance = Vector3.Distance(transform.position, _lastSamplePosition);
                if (movedDistance < 0.1f)
                {
                    _stuckTimer += Time.deltaTime;
                    if (_stuckTimer >= _stuckThresholdTime)
                    {
                        Debug.LogWarning($"[CustomerNavigation] Customer {gameObject.name} appears stuck at {transform.position}. Triggering recovery.");
                        _hasActiveDestination = false;
                        _stuckTimer = 0f;
                        _onStuckCallback?.Invoke();
                        _onStuckCallback = null;
                        return;
                    }
                }
                else
                {
                    _stuckTimer = 0f;
                    _lastSamplePosition = transform.position;
                }
            }

            // 3. Smooth Alignment to Target Facing Direction
            if (_isAligningFacing)
            {
                Quaternion targetRot = Quaternion.LookRotation(_targetFacingDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.rotation, targetRot) < 2f)
                {
                    _isAligningFacing = false;
                }
            }
        }
    }
}
