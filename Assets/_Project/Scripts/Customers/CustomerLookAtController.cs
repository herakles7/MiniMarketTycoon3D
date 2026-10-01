using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Lightweight mobile-optimized LookAt controller for customer NPCs.
    /// Smoothly rotates the head towards interest points (shelves when browsing/picking,
    /// checkout register when queuing, or movement direction when walking).
    /// Enforces anatomical range limits (yaw +/- 40 deg, pitch +/- 20 deg) and strictly avoids
    /// heavy FindObjectsOfType, raycasts, or per-frame allocations.
    /// </summary>
    public class CustomerLookAtController : MonoBehaviour
    {
        [Header("Bone References")]
        [SerializeField] private Transform _headBone;

        [Header("Settings")]
        [SerializeField] private float _turnSpeed = 4.5f;
        [SerializeField] private float _maxAngleDegrees = 45f;

        private Vector3? _targetPosition = null;
        private Quaternion _currentOffset = Quaternion.identity;

        public void BindHeadBone(Transform head)
        {
            _headBone = head;
        }

        public void SetTarget(Vector3 worldPos)
        {
            _targetPosition = worldPos;
        }

        public void ClearTarget()
        {
            _targetPosition = null;
        }

        public void ResetLookAt()
        {
            _targetPosition = null;
            _currentOffset = Quaternion.identity;
        }

        private void LateUpdate()
        {
            if (_headBone == null) return;

            Quaternion targetOffset = Quaternion.identity;

            if (_targetPosition.HasValue)
            {
                Vector3 toTarget = _targetPosition.Value - _headBone.position;
                if (toTarget.sqrMagnitude > 0.04f)
                {
                    Vector3 localDir = transform.InverseTransformDirection(toTarget.normalized);
                    // Only look if target is in front of customer (within 85 degrees)
                    if (localDir.z > 0.05f)
                    {
                        float yaw = Mathf.Clamp(Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg, -_maxAngleDegrees, _maxAngleDegrees);
                        float pitch = Mathf.Clamp(-Mathf.Atan2(localDir.y, Mathf.Sqrt(localDir.x * localDir.x + localDir.z * localDir.z)) * Mathf.Rad2Deg, -20f, 20f);
                        targetOffset = Quaternion.Euler(pitch, yaw, 0f);
                    }
                }
            }

            _currentOffset = Quaternion.Slerp(_currentOffset, targetOffset, Time.deltaTime * _turnSpeed);
            _headBone.localRotation = _headBone.localRotation * _currentOffset;
        }
    }
}
