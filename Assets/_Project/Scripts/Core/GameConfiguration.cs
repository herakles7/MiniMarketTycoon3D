using UnityEngine;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Master game configuration container linking sub-configurations.
    /// Prevents hard-coded values spread throughout the codebase.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "MiniMarket/Configuration/Game Configuration")]
    public class GameConfiguration : ScriptableObject
    {
        [Header("Sub Configurations")]
        [SerializeField] private StoreConfiguration _storeConfiguration;
        [SerializeField] private EconomyConfiguration _economyConfiguration;

        [Header("Performance Defaults")]
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private int _defaultQualityLevel = 1; // 0=Low, 1=Medium, 2=High, 3=Ultra

        public StoreConfiguration StoreConfig => _storeConfiguration;
        public EconomyConfiguration EconomyConfig => _economyConfiguration;
        public int TargetFrameRate => _targetFrameRate;
        public int DefaultQualityLevel => _defaultQualityLevel;
    }
}
