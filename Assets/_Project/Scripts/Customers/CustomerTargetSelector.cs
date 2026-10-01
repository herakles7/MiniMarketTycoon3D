using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Central store directory of customer navigation targets, shelf interaction points,
    /// entrance portals, and exit waypoints.
    /// </summary>
    public class CustomerTargetSelector : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _entrancePoint;
        [SerializeField] private Transform _browsingPoint;
        [SerializeField] private Transform _exitPoint;

        [Header("Interaction Points")]
        [SerializeField] private List<CustomerInteractionPoint> _interactionPoints = new List<CustomerInteractionPoint>();

        public Transform SpawnPoint => _spawnPoint;
        public Transform EntrancePoint => _entrancePoint;
        public Transform BrowsingPoint => _browsingPoint;
        public Transform ExitPoint => _exitPoint;

        private void Awake()
        {
            EnsureDefaultWaypoints();

            if (_interactionPoints == null || _interactionPoints.Count == 0)
            {
                DiscoverInteractionPoints();
            }
        }

        public void EnsureDefaultWaypoints()
        {
            if (_spawnPoint == null)
            {
                var sp = new GameObject("CustomerSpawnPoint");
                sp.transform.parent = transform;
                sp.transform.position = new Vector3(0f, 0f, -13f);
                _spawnPoint = sp.transform;
            }
            if (_entrancePoint == null)
            {
                var ep = new GameObject("CustomerEntrancePoint");
                ep.transform.parent = transform;
                ep.transform.position = new Vector3(0f, 0f, -9.5f);
                _entrancePoint = ep.transform;
            }
            if (_browsingPoint == null)
            {
                var bp = new GameObject("CustomerBrowsingPoint");
                bp.transform.parent = transform;
                bp.transform.position = new Vector3(0f, 0f, -5.5f);
                _browsingPoint = bp.transform;
            }
            if (_exitPoint == null)
            {
                var xp = new GameObject("CustomerExitPoint");
                xp.transform.parent = transform;
                xp.transform.position = new Vector3(-2.0f, 0f, -14f);
                _exitPoint = xp.transform;
            }
        }

        public void SetupWaypoints(Transform spawn, Transform entrance, Transform browsing, Transform exit)
        {
            _spawnPoint = spawn;
            _entrancePoint = entrance;
            _browsingPoint = browsing;
            _exitPoint = exit;
        }

        /// <summary>
        /// Returns a scattered browsing destination dispersed among different store aisles
        /// so entering customers do not bunch up or block each other.
        /// </summary>
        public Vector3 GetBrowsingTarget()
        {
            // Disperse between Left aisle (-2.8m), Center main aisle (0m), and Right aisle (+2.8m)
            float[] aisleX = new float[] { -2.8f, 0f, 2.8f };
            float chosenX = aisleX[UnityEngine.Random.Range(0, aisleX.Length)];
            float zJitter = UnityEngine.Random.Range(-1.2f, 1.2f);
            float baseY = _browsingPoint != null ? _browsingPoint.position.y : 0f;
            float baseZ = _browsingPoint != null ? _browsingPoint.position.z : -5.5f;

            return new Vector3(chosenX, baseY, baseZ + zJitter);
        }

        public void RegisterPoint(CustomerInteractionPoint point)
        {
            if (point != null && !_interactionPoints.Contains(point))
            {
                _interactionPoints.Add(point);
            }
        }

        public void DiscoverInteractionPoints()
        {
            _interactionPoints.Clear();
            var points = FindObjectsByType<CustomerInteractionPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            _interactionPoints.AddRange(points);
        }

        public CustomerInteractionPoint GetPointForCategory(string category)
        {
            if (_interactionPoints.Count == 0)
            {
                DiscoverInteractionPoints();
            }

            // 1. Try to find an unoccupied point matching the category
            for (int i = 0; i < _interactionPoints.Count; i++)
            {
                var pt = _interactionPoints[i];
                if (pt != null && !pt.IsOccupied && string.Equals(pt.Category, category, System.StringComparison.OrdinalIgnoreCase))
                {
                    return pt;
                }
            }

            // 2. Fallback: Any unoccupied shelf
            for (int i = 0; i < _interactionPoints.Count; i++)
            {
                var pt = _interactionPoints[i];
                if (pt != null && !pt.IsOccupied)
                {
                    return pt;
                }
            }

            // 3. Fallback: Random shelf
            if (_interactionPoints.Count > 0)
            {
                return _interactionPoints[Random.Range(0, _interactionPoints.Count)];
            }

            return null;
        }

        public CustomerInteractionPoint GetRandomShelfPoint()
        {
            if (_interactionPoints.Count == 0)
            {
                DiscoverInteractionPoints();
            }

            if (_interactionPoints.Count == 0) return null;
            return _interactionPoints[Random.Range(0, _interactionPoints.Count)];
        }
    }
}
