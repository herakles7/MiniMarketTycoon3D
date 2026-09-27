using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Builds and manages the Unity AI NavMesh for the market environment at runtime or edit-time
    /// using UnityEngine.AI.NavMeshBuilder.
    /// Ensures customer NPCs navigate smoothly along store aisles and cannot enter walls,
    /// shelf interiors, checkout employee zones, or storage rooms.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class MarketNavMeshManager : MonoBehaviour
    {
        [Header("Agent Physical Dimensions")]
        [SerializeField] private float _agentRadius = 0.32f;
        [SerializeField] private float _agentHeight = 1.8f;
        [SerializeField] private float _agentSlope = 45f;
        [SerializeField] private float _agentClimb = 0.25f;

        [Header("NavMesh Spatial Bounds")]
        [SerializeField] private Vector3 _boundsCenter = new Vector3(0f, 2.0f, 2.0f);
        [SerializeField] private Vector3 _boundsSize = new Vector3(45f, 10f, 45f);

        private NavMeshData _navMeshData;
        private NavMeshDataInstance _navMeshInstance;

        public static MarketNavMeshManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            BuildRuntimeNavMesh();
        }

        [ContextMenu("Rebuild Runtime NavMesh")]
        public void BuildRuntimeNavMesh()
        {
            if (_navMeshInstance.valid)
            {
                NavMesh.RemoveNavMeshData(_navMeshInstance);
            }

            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            settings.agentRadius = _agentRadius;
            settings.agentHeight = _agentHeight;
            settings.agentSlope = _agentSlope;
            settings.agentClimb = _agentClimb;

            Bounds bounds = new Bounds(_boundsCenter, _boundsSize);
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

            // Collect all physics colliders across the store environment
            NavMeshBuilder.CollectSources(bounds, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, markups, sources);

            _navMeshData = NavMeshBuilder.BuildNavMeshData(settings, sources, bounds, Vector3.zero, Quaternion.identity);

            if (_navMeshData != null)
            {
                _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData);
                Debug.Log("[MarketNavMeshManager] Retail NavMesh successfully built and activated via NavMeshBuilder!");
            }
            else
            {
                Debug.LogWarning("[MarketNavMeshManager] NavMeshData build returned null.");
            }
        }

        private void OnDestroy()
        {
            if (_navMeshInstance.valid)
            {
                NavMesh.RemoveNavMeshData(_navMeshInstance);
            }
        }
    }
}
