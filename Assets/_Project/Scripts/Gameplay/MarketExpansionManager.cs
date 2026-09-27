using System;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.UI;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Central manager for store expansions and physical market upgrades.
    /// Synchronizes with MarketUpgradeManager to unlock expansion areas, open gates,
    /// adjust camera limits, update NavMesh pathfinding, and register newly accessible shelves.
    /// </summary>
    public class MarketExpansionManager : MonoBehaviourSingleton<MarketExpansionManager>
    {
        [Header("Expansion Areas")]
        [SerializeField] private List<MarketExpansionArea> _expansionAreas = new List<MarketExpansionArea>();

        [Header("Camera Rig")]
        [SerializeField] private CameraController _cameraController;

        public IReadOnlyList<MarketExpansionArea> ExpansionAreas => _expansionAreas;
        public int CurrentExpansionLevel => MarketUpgradeManager.HasInstance ? MarketUpgradeManager.Instance.CurrentLevel : 1;

        /// <summary>
        /// Event fired when a new store expansion area is unlocked: (level, expansionArea)
        /// </summary>
        public event Action<int, MarketExpansionArea> OnExpansionUnlocked;

        protected override void OnInitialized()
        {
            if (_expansionAreas.Count == 0)
            {
                DiscoverExpansionAreas();
            }

            if (_cameraController == null)
            {
                _cameraController = FindFirstObjectByType<CameraController>();
            }

            // Hook up with MarketUpgradeManager
            if (MarketUpgradeManager.HasInstance)
            {
                MarketUpgradeManager.Instance.OnMarketLevelUpgraded -= HandleMarketLevelUpgraded;
                MarketUpgradeManager.Instance.OnMarketLevelUpgraded += HandleMarketLevelUpgraded;
            }

            // Hook up with SaveManager
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded -= HandleSaveLoaded;
                SaveManager.Instance.OnSaveLoaded += HandleSaveLoaded;
            }
        }

        private void Start()
        {
            // Sync with current market level on startup
            int initialLevel = MarketUpgradeManager.HasInstance ? MarketUpgradeManager.Instance.CurrentLevel : 1;
            ApplyExpansionLevel(initialLevel, animate: false);
        }

        public void DiscoverExpansionAreas()
        {
            _expansionAreas.Clear();
            var areas = FindObjectsByType<MarketExpansionArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (areas != null && areas.Length > 0)
            {
                _expansionAreas.AddRange(areas);
                // Sort by target level ascending
                _expansionAreas.Sort((a, b) => a.TargetLevel.CompareTo(b.TargetLevel));
            }
        }

        public void RegisterExpansionArea(MarketExpansionArea area)
        {
            if (area != null && !_expansionAreas.Contains(area))
            {
                _expansionAreas.Add(area);
                _expansionAreas.Sort((a, b) => a.TargetLevel.CompareTo(b.TargetLevel));
            }
        }

        private void HandleSaveLoaded(GameSaveData saveData)
        {
            int level = (saveData != null && saveData.Store != null) ? saveData.Store.StoreLevel : 1;
            ApplyExpansionLevel(level, animate: false);
        }

        private void HandleMarketLevelUpgraded(int newLevel)
        {
            ApplyExpansionLevel(newLevel, animate: true);
        }

        /// <summary>
        /// Applies the unlocked / locked status to all expansion areas matching the current level.
        /// </summary>
        public void ApplyExpansionLevel(int level, bool animate = false)
        {
            if (_expansionAreas.Count == 0)
            {
                DiscoverExpansionAreas();
            }

            MarketExpansionArea newlyUnlockedArea = null;

            for (int i = 0; i < _expansionAreas.Count; i++)
            {
                var area = _expansionAreas[i];
                if (area == null) continue;

                bool shouldBeUnlocked = level >= area.TargetLevel;
                bool wasUnlocked = area.IsUnlocked;

                area.SetState(shouldBeUnlocked, animate && shouldBeUnlocked && !wasUnlocked);

                if (shouldBeUnlocked && !wasUnlocked)
                {
                    newlyUnlockedArea = area;
                }
            }

            // Update camera bounds to encompass the new store footprint
            if (_cameraController == null)
            {
                _cameraController = FindFirstObjectByType<CameraController>();
            }

            if (_cameraController != null)
            {
                _cameraController.UpdateBoundsForLevel(level);
            }

            // Rebuild NavMesh if a new area was just unlocked
            if (newlyUnlockedArea != null)
            {
                if (MarketNavMeshManager.HasInstance)
                {
                    MarketNavMeshManager.Instance.BuildRuntimeNavMesh();
                }

                if (ShelfManager.HasInstance)
                {
                    ShelfManager.Instance.DiscoverShelves();
                }

                if (FloatingFeedbackManager.HasInstance)
                {
                    FloatingFeedbackManager.Instance.ShowExpansionUnlockedFeedback(level, newlyUnlockedArea.AreaTitle);
                }

                OnExpansionUnlocked?.Invoke(level, newlyUnlockedArea);
            }
        }

        public bool IsExpansionUnlocked(int level)
        {
            var area = GetExpansionArea(level);
            return area != null && area.IsUnlocked;
        }

        public MarketExpansionArea GetExpansionArea(int level)
        {
            for (int i = 0; i < _expansionAreas.Count; i++)
            {
                if (_expansionAreas[i] != null && _expansionAreas[i].TargetLevel == level)
                {
                    return _expansionAreas[i];
                }
            }
            return null;
        }

        protected override void OnDestroy()
        {
            if (MarketUpgradeManager.HasInstance)
            {
                MarketUpgradeManager.Instance.OnMarketLevelUpgraded -= HandleMarketLevelUpgraded;
            }

            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnSaveLoaded -= HandleSaveLoaded;
            }

            base.OnDestroy();
        }
    }
}
