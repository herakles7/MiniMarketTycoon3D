using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Represents an individual physical store expansion area (e.g., Expansion_Level2, Level3, Level4, Level5).
    /// Manages the physical barrier gate, 3D locked indicator, interior lighting, NavMesh obstacle carving,
    /// and registration of expansion shelves.
    /// </summary>
    public class MarketExpansionArea : MonoBehaviour
    {
        [Header("Expansion Identity")]
        [SerializeField] private int _targetLevel = 2;
        [SerializeField] private string _areaTitle = "Expanded Mini Mart";
        [SerializeField] private string _areaSubtitle = "East Wing - Fresh Deli";

        [Header("Physical Barrier & Gate")]
        [SerializeField] private GameObject _gateObject;
        [SerializeField] private Collider _gateCollider;
        [SerializeField] private NavMeshObstacle _gateObstacle;
        [SerializeField] private GameObject _gateIndicator;

        [Header("Interior & Shelving")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private List<GameObject> _interiorLights = new List<GameObject>();
        [SerializeField] private List<ShelfVisualController> _shelves = new List<ShelfVisualController>();

        private bool _isUnlocked = false;
        private Vector3 _gateClosedLocalPos;
        private Coroutine _gateAnimCoroutine;

        public int TargetLevel => _targetLevel;
        public string AreaTitle => _areaTitle;
        public string AreaSubtitle => _areaSubtitle;
        public bool IsUnlocked => _isUnlocked;
        public IReadOnlyList<ShelfVisualController> Shelves => _shelves;
        public Collider GateCollider => _gateCollider;
        public GameObject GateIndicator => _gateIndicator;

        public void Configure(int targetLevel, string title, string subtitle, GameObject gateObj, Collider gateCol, NavMeshObstacle gateObs, GameObject indicator, GameObject contentRoot, List<GameObject> lights, List<ShelfVisualController> shelves)
        {
            _targetLevel = targetLevel;
            _areaTitle = title;
            _areaSubtitle = subtitle;
            _gateObject = gateObj;
            _gateCollider = gateCol;
            _gateObstacle = gateObs;
            _gateIndicator = indicator;
            _contentRoot = contentRoot;
            _interiorLights = lights ?? new List<GameObject>();
            _shelves = shelves ?? new List<ShelfVisualController>();

            if (_gateObject != null)
            {
                _gateClosedLocalPos = _gateObject.transform.localPosition;
            }
        }

        private void Awake()
        {
            if (_gateObject != null && _gateClosedLocalPos == Vector3.zero)
            {
                _gateClosedLocalPos = _gateObject.transform.localPosition;
            }

            if (_shelves.Count == 0)
            {
                var found = GetComponentsInChildren<ShelfVisualController>(true);
                if (found != null && found.Length > 0)
                {
                    _shelves.AddRange(found);
                }
            }
        }

        public void SetState(bool unlocked, bool animate = false)
        {
            if (unlocked)
            {
                Unlock(animate);
            }
            else
            {
                Lock();
            }
        }

        public void Unlock(bool animate = false)
        {
            _isUnlocked = true;

            // 1. Hide 3D indicator
            if (_gateIndicator != null)
            {
                _gateIndicator.SetActive(false);
            }

            // 2. Disable gate collider & NavMesh obstacle so pathfinding and physics flow freely
            if (_gateCollider != null)
            {
                _gateCollider.enabled = false;
            }

            if (_gateObstacle != null)
            {
                _gateObstacle.enabled = false;
            }

            // 3. Open the physical shutter/barrier gate
            if (_gateObject != null)
            {
                if (animate && gameObject.activeInHierarchy)
                {
                    if (_gateAnimCoroutine != null) StopCoroutine(_gateAnimCoroutine);
                    _gateAnimCoroutine = StartCoroutine(AnimateGateOpen());
                }
                else
                {
                    _gateObject.SetActive(false);
                }
            }

            // 4. Activate interior lighting
            for (int i = 0; i < _interiorLights.Count; i++)
            {
                if (_interiorLights[i] != null)
                {
                    _interiorLights[i].SetActive(true);
                }
            }

            // 5. Activate and register shelves with ShelfManager
            for (int i = 0; i < _shelves.Count; i++)
            {
                var s = _shelves[i];
                if (s != null)
                {
                    s.gameObject.SetActive(true);
                    if (ShelfManager.HasInstance)
                    {
                        ShelfManager.Instance.RegisterShelf(s);
                    }
                }
            }

            Debug.Log($"[MarketExpansionArea] Level {_targetLevel} area '{_areaTitle}' unlocked!");
        }

        public void Lock()
        {
            _isUnlocked = false;

            if (_gateAnimCoroutine != null)
            {
                StopCoroutine(_gateAnimCoroutine);
                _gateAnimCoroutine = null;
            }

            // 1. Activate gate barrier & reset position
            if (_gateObject != null)
            {
                _gateObject.SetActive(true);
                if (_gateClosedLocalPos != Vector3.zero)
                {
                    _gateObject.transform.localPosition = _gateClosedLocalPos;
                }
            }

            // 2. Enable gate collision & NavMesh obstacle carving
            if (_gateCollider != null)
            {
                _gateCollider.enabled = true;
            }

            if (_gateObstacle != null)
            {
                _gateObstacle.enabled = true;
                _gateObstacle.carving = true;
            }

            // 3. Activate 3D lock indicator
            if (_gateIndicator != null)
            {
                _gateIndicator.SetActive(true);
            }

            // 4. Keep lights dimmed/off
            for (int i = 0; i < _interiorLights.Count; i++)
            {
                if (_interiorLights[i] != null)
                {
                    _interiorLights[i].SetActive(false);
                }
            }

            // 5. Deactivate shelves while locked so customers don't select them
            for (int i = 0; i < _shelves.Count; i++)
            {
                var s = _shelves[i];
                if (s != null)
                {
                    if (ShelfManager.HasInstance)
                    {
                        ShelfManager.Instance.UnregisterShelf(s);
                    }
                    s.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator AnimateGateOpen()
        {
            float duration = 0.65f;
            float elapsed = 0f;
            Vector3 startPos = _gateObject.transform.localPosition;
            Vector3 targetPos = startPos + new Vector3(0f, 3.2f, 0f); // Slide shutter up into ceiling

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                _gateObject.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            _gateObject.SetActive(false);
            _gateAnimCoroutine = null;
        }
    }
}
