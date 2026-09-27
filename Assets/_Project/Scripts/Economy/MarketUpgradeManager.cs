using System;
using UnityEngine;
using MiniMarketTycoon.Core;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Economy
{
    /// <summary>
    /// Central manager for Store Level progression, upgrades, capacity scaling,
    /// and dynamic tycoon perks. Coordinates with CurrencyManager and SaveManager.
    /// </summary>
    public class MarketUpgradeManager : MonoBehaviourSingleton<MarketUpgradeManager>
    {
        [Header("Configuration")]
        [SerializeField] private MarketProgressionConfig _progressionConfig;

        [Header("Runtime State")]
        [SerializeField] private int _currentLevel = 1;
        [SerializeField] private int _cashierSpeedLevel = 1;

        public int CurrentLevel => _currentLevel;
        public int CashierSpeedLevel => _cashierSpeedLevel;
        public MarketProgressionConfig ProgressionConfig => _progressionConfig;

        public int MaxLevel => _progressionConfig != null ? _progressionConfig.MaxLevel : 5;
        public bool IsMaxLevel => _currentLevel >= MaxLevel;

        public MarketLevelData CurrentLevelData => _progressionConfig != null
            ? _progressionConfig.GetLevelData(_currentLevel)
            : MarketProgressionConfig.GetDefaultLevelData(_currentLevel);

        public MarketLevelData NextLevelData => _progressionConfig != null
            ? _progressionConfig.GetNextLevelData(_currentLevel)
            : MarketProgressionConfig.GetDefaultLevelData(_currentLevel + 1);

        public double NextUpgradeCost => !IsMaxLevel && CurrentLevelData != null ? CurrentLevelData.UpgradeCost : 0.0;
        public int CurrentMaxCustomers => CurrentLevelData != null ? CurrentLevelData.MaxCustomers : 5;
        public float CurrentSpawnInterval => CurrentLevelData != null ? CurrentLevelData.SpawnInterval : 8.0f;
        public int CurrentShelfCapacity => CurrentLevelData != null ? CurrentLevelData.ShelfCapacity : 8;
        public float CurrentProductCapacityMultiplier => CurrentLevelData != null ? CurrentLevelData.ProductCapacityMultiplier : 1.0f;
        public float CurrentCheckoutTime => CurrentLevelData != null ? CurrentLevelData.CheckoutTime : 2.5f;

        /// <summary>
        /// Event fired whenever the market level increases: (newLevel)
        /// </summary>
        public event Action<int> OnMarketLevelUpgraded;

        /// <summary>
        /// Event fired whenever progression or upgrade parameters are altered.
        /// </summary>
        public event Action OnProgressionChanged;

        protected override void OnInitialized()
        {
            LoadConfiguration();

            // Load saved progression state
            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                var store = SaveManager.Instance.CurrentSave.Store;
                if (store != null)
                {
                    _currentLevel = Mathf.Clamp(store.StoreLevel, 1, MaxLevel);
                    _cashierSpeedLevel = Mathf.Clamp(store.CheckoutSpeedLevel > 0 ? store.CheckoutSpeedLevel : _currentLevel, 1, MaxLevel);
                }

                SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
            }

            Debug.Log($"[MarketUpgradeManager] Initialized at Level {_currentLevel}/{MaxLevel}. MaxCustomers: {CurrentMaxCustomers}, SpawnInterval: {CurrentSpawnInterval}s, Checkout: {CurrentCheckoutTime}s");
        }

        private void LoadConfiguration()
        {
            if (_progressionConfig == null)
            {
#if UNITY_EDITOR
                _progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<MarketProgressionConfig>("Assets/_Project/ScriptableObjects/Configuration/MarketProgressionConfig.asset");
#endif
                if (_progressionConfig == null)
                {
                    _progressionConfig = Resources.Load<MarketProgressionConfig>("MarketProgressionConfig");
                }

                if (_progressionConfig == null)
                {
                    _progressionConfig = ScriptableObject.CreateInstance<MarketProgressionConfig>();
                }
            }
        }

        public void SetConfiguration(MarketProgressionConfig config)
        {
            _progressionConfig = config;
            OnProgressionChanged?.Invoke();
        }

        private void HandleBeforeSave(GameSaveData saveData)
        {
            if (saveData == null) return;

            if (saveData.Store == null)
            {
                saveData.Store = new StoreData();
            }

            saveData.Store.StoreLevel = _currentLevel;
            saveData.Store.CapacityLevel = _currentLevel;
            saveData.Store.CheckoutSpeedLevel = _cashierSpeedLevel;
        }

        public bool CanAffordUpgrade()
        {
            if (IsMaxLevel) return false;
            if (!CurrencyManager.HasInstance) return false;

            double cost = NextUpgradeCost;
            return cost > 0.0 && CurrencyManager.Instance.CanAfford(cost);
        }

        /// <summary>
        /// Attempts to purchase the next market level tier using currency.
        /// Returns true if transaction was valid and level was incremented.
        /// </summary>
        public bool TryUpgradeMarket()
        {
            if (IsMaxLevel)
            {
                Debug.LogWarning("[MarketUpgradeManager] Already at maximum market level.");
                return false;
            }

            if (!CurrencyManager.HasInstance)
            {
                Debug.LogError("[MarketUpgradeManager] CurrencyManager is not active.");
                return false;
            }

            double cost = NextUpgradeCost;
            if (!CurrencyManager.Instance.CanAfford(cost))
            {
                Debug.Log($"[MarketUpgradeManager] Insufficient funds: Need ${cost:N0}, have ${CurrencyManager.Instance.Cash:N0}");
                return false;
            }

            // Deduct cash safely
            if (!CurrencyManager.Instance.RemoveCurrency(cost))
            {
                return false;
            }

            // Apply level promotion
            _currentLevel++;
            _cashierSpeedLevel = _currentLevel;

            Debug.Log($"[MarketUpgradeManager] Upgraded market to Level {_currentLevel}! New perks: Customers={CurrentMaxCustomers}, Interval={CurrentSpawnInterval}s, ShelfCap={CurrentShelfCapacity}, CapMult={CurrentProductCapacityMultiplier}x");

            OnMarketLevelUpgraded?.Invoke(_currentLevel);
            OnProgressionChanged?.Invoke();

            // Save immediately on major progression upgrade
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.SaveGame();
            }

            return true;
        }

        /// <summary>
        /// Calculates effective max stock taking market level multiplier into account.
        /// Keeps ProductData static and unmodified.
        /// </summary>
        public int GetEffectiveMaxStock(int baseMaxStock)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseMaxStock * CurrentProductCapacityMultiplier));
        }

        /// <summary>
        /// Returns effective shelf capacity for current market tier.
        /// </summary>
        public int GetEffectiveShelfCapacity(int baseShelfCapacity)
        {
            return Mathf.Max(baseShelfCapacity, CurrentShelfCapacity);
        }

        public void SetLevelDirect(int level)
        {
            _currentLevel = Mathf.Clamp(level, 1, MaxLevel);
            _cashierSpeedLevel = _currentLevel;
            OnMarketLevelUpgraded?.Invoke(_currentLevel);
            OnProgressionChanged?.Invoke();
        }

        protected override void OnDestroy()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnBeforeSave -= HandleBeforeSave;
            }
            base.OnDestroy();
        }
    }
}
