using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Proactively scans the area around the player for IInteractable objects.
    /// Selects the best candidate based on distance and facing angle, then coordinates
    /// visual highlight and 3D floating interaction prompt display.
    /// </summary>
    public class PlayerInteractionDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float _detectRadius = 2.4f;
        [SerializeField] private float _detectionAngle = 100f; // Max angle in front of player
        [SerializeField] private LayerMask _interactionLayers = ~0; // All layers by default
        [SerializeField] private float _scanInterval = 0.1f;

        [Header("UI Prompt Reference")]
        [SerializeField] private InteractionPromptUI _promptUI;

        private PlayerManagerController _player;
        private IInteractable _currentTarget;
        private float _scanTimer;
        private readonly Collider[] _hitBuffer = new Collider[16];

        public IInteractable CurrentTarget => _currentTarget;

        private void Awake()
        {
            _player = GetComponent<PlayerManagerController>();
            if (_promptUI == null)
            {
                _promptUI = FindFirstObjectByType<InteractionPromptUI>();
            }
        }

        private void Update()
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer >= _scanInterval)
            {
                _scanTimer = 0f;
                ScanForInteractables();
            }

            // Update UI position smoothly every frame if active
            if (_currentTarget != null && _promptUI != null && _currentTarget.CanInteract(_player))
            {
                _promptUI.UpdatePosition(_currentTarget.GetPromptWorldPosition());
            }
        }

        private void ScanForInteractables()
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            int count = Physics.OverlapSphereNonAlloc(origin, _detectRadius, _hitBuffer, _interactionLayers, QueryTriggerInteraction.Collide);

            IInteractable bestCandidate = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider col = _hitBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;

                // Check interactable on collider or parent
                IInteractable interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(_player)) continue;

                Vector3 targetPos = interactable.GetPromptWorldPosition();
                Vector3 toTarget = (targetPos - transform.position);
                toTarget.y = 0f;
                float dist = toTarget.magnitude;

                if (dist > _detectRadius) continue;

                // Angle check
                float angle = Vector3.Angle(transform.forward, toTarget);
                if (angle > _detectionAngle * 0.5f && dist > 1.0f) continue;

                // Score combines distance and angle alignment
                float score = dist + (angle / 180f) * 1.5f;

                if (score < bestScore)
                {
                    bestScore = score;
                    bestCandidate = interactable;
                }
            }

            // Handle target change
            if (_currentTarget != bestCandidate)
            {
                if (_currentTarget != null)
                {
                    _currentTarget.OnFocusExit(_player);
                }

                _currentTarget = bestCandidate;

                if (_currentTarget != null)
                {
                    _currentTarget.OnFocusEnter(_player);
                    if (_promptUI != null)
                    {
                        _promptUI.ShowPrompt(_currentTarget.GetInteractionPrompt(), _currentTarget.GetPromptWorldPosition());
                    }
                }
                else
                {
                    if (_promptUI != null)
                    {
                        _promptUI.HidePrompt();
                    }
                }
            }
            else if (_currentTarget != null)
            {
                // Refresh prompt text if changed dynamically
                if (_promptUI != null)
                {
                    _promptUI.SetPromptText(_currentTarget.GetInteractionPrompt());
                }
            }
        }

        private void OnDisable()
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnFocusExit(_player);
                _currentTarget = null;
            }

            if (_promptUI != null)
            {
                _promptUI.HidePrompt();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, _detectRadius);
        }
    }
}
