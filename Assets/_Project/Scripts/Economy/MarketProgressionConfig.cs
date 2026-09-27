using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Economy
{
    [Serializable]
    public class MarketLevelData
    {
        [Tooltip("Market Level number (e.g. 1, 2, 3...)")]
        public int Level = 1;

        [Tooltip("Descriptive tier title displayed in the HUD and Upgrade UI.")]
        public string Title = "Starter Market";

        [Tooltip("Cost in cash to upgrade FROM this level to the next level. Set to 0 for max level.")]
        public double UpgradeCost = 500.0;

        [Tooltip("Maximum concurrent customers permitted inside the store.")]
        public int MaxCustomers = 5;

        [Tooltip("Spawn interval in seconds between customer arrivals.")]
        public float SpawnInterval = 8.0f;

        [Tooltip("Display item capacity per shelf.")]
        public int ShelfCapacity = 8;

        [Tooltip("Multiplier applied to the base max warehouse stock of all products.")]
        public float ProductCapacityMultiplier = 1.0f;

        [Tooltip("Time in seconds required to process checkout per customer.")]
        public float CheckoutTime = 2.5f;

        [Tooltip("Summary of new perks unlocked at this tier.")]
        public string BenefitsSummary = "Starting Market";
    }

    /// <summary>
    /// ScriptableObject defining tycoon progression curves, costs, and gameplay unlocks
    /// across market levels without hardcoding values in scripts.
    /// </summary>
    [CreateAssetMenu(fileName = "MarketProgressionConfig", menuName = "MiniMarket/Configuration/Market Progression Config")]
    public class MarketProgressionConfig : ScriptableObject
    {
        [Header("Progression Levels")]
        [SerializeField] private List<MarketLevelData> _levels = new List<MarketLevelData>();

        public IReadOnlyList<MarketLevelData> Levels => _levels;
        public int MaxLevel => _levels != null && _levels.Count > 0 ? _levels[_levels.Count - 1].Level : 5;

        public MarketLevelData GetLevelData(int level)
        {
            if (_levels == null || _levels.Count == 0)
            {
                return GetDefaultLevelData(level);
            }

            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].Level == level)
                {
                    return _levels[i];
                }
            }

            // Fallback: clamp to nearest available level
            if (level < _levels[0].Level) return _levels[0];
            return _levels[_levels.Count - 1];
        }

        public MarketLevelData GetNextLevelData(int currentLevel)
        {
            return GetLevelData(currentLevel + 1);
        }

        public bool HasNextLevel(int currentLevel)
        {
            return currentLevel < MaxLevel;
        }

        public static MarketLevelData GetDefaultLevelData(int level)
        {
            switch (level)
            {
                case 1:
                    return new MarketLevelData
                    {
                        Level = 1,
                        Title = "Starter Market",
                        UpgradeCost = 500.0,
                        MaxCustomers = 5,
                        SpawnInterval = 8.0f,
                        ShelfCapacity = 8,
                        ProductCapacityMultiplier = 1.0f,
                        CheckoutTime = 2.5f,
                        BenefitsSummary = "Base Market Capacity"
                    };
                case 2:
                    return new MarketLevelData
                    {
                        Level = 2,
                        Title = "Expanded Mini Mart",
                        UpgradeCost = 2000.0,
                        MaxCustomers = 7,
                        SpawnInterval = 7.0f,
                        ShelfCapacity = 12,
                        ProductCapacityMultiplier = 1.5f,
                        CheckoutTime = 2.2f,
                        BenefitsSummary = "+2 Customers, +4 Shelf Cap, 7s Spawn, 2.2s Checkout, +50% Stock"
                    };
                case 3:
                    return new MarketLevelData
                    {
                        Level = 3,
                        Title = "Busy Local Store",
                        UpgradeCost = 7500.0,
                        MaxCustomers = 10,
                        SpawnInterval = 6.0f,
                        ShelfCapacity = 16,
                        ProductCapacityMultiplier = 2.0f,
                        CheckoutTime = 1.9f,
                        BenefitsSummary = "+3 Customers, +4 Shelf Cap, 6s Spawn, 1.9s Checkout, 2x Stock"
                    };
                case 4:
                    return new MarketLevelData
                    {
                        Level = 4,
                        Title = "Popular Superette",
                        UpgradeCost = 20000.0,
                        MaxCustomers = 14,
                        SpawnInterval = 5.0f,
                        ShelfCapacity = 20,
                        ProductCapacityMultiplier = 3.0f,
                        CheckoutTime = 1.6f,
                        BenefitsSummary = "+4 Customers, +4 Shelf Cap, 5s Spawn, 1.6s Checkout, 3x Stock"
                    };
                case 5:
                default:
                    return new MarketLevelData
                    {
                        Level = 5,
                        Title = "Grand Tycoon Mart",
                        UpgradeCost = 0.0, // Max Level
                        MaxCustomers = 18,
                        SpawnInterval = 4.0f,
                        ShelfCapacity = 24,
                        ProductCapacityMultiplier = 4.0f,
                        CheckoutTime = 1.3f,
                        BenefitsSummary = "+4 Customers, +4 Shelf Cap, 4s Spawn, 1.3s Checkout, 4x Stock (MAX)"
                    };
            }
        }
    }
}
