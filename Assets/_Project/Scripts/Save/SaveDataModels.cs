using System;
using System.Collections.Generic;

namespace MiniMarketTycoon.Save
{
    [Serializable]
    public class CurrencyData
    {
        public double Cash = 1000.0;
        public double Gems = 0.0;
    }

    [Serializable]
    public class StoreData
    {
        public string StoreName = "My Mini Market";
        public int StoreLevel = 1;
        public List<string> UnlockedShelfIds = new List<string>();
        public int CapacityLevel = 1;
        public int CheckoutSpeedLevel = 1;
    }

    [Serializable]
    public class UpgradeSaveData
    {
        public string UpgradeId;
        public int CurrentLevel;

        public UpgradeSaveData() { }

        public UpgradeSaveData(string upgradeId, int currentLevel)
        {
            UpgradeId = upgradeId;
            CurrentLevel = currentLevel;
        }
    }

    [Serializable]
    public class ProductSaveData
    {
        public string ProductId;
        public bool IsUnlocked;
        public int CurrentStock;
        public int ItemsSold;
        public double Revenue;
        public double Profit;

        public ProductSaveData() { }

        public ProductSaveData(string productId, bool isUnlocked, int currentStock = 0)
        {
            ProductId = productId;
            IsUnlocked = isUnlocked;
            CurrentStock = currentStock;
            ItemsSold = 0;
            Revenue = 0.0;
            Profit = 0.0;
        }
    }

    [Serializable]
    public class EconomySaveData
    {
        public double TodayRevenue;
        public double TodayExpenses;
        public double TodayProfit;
        public int ItemsSold;
        public int CustomersServed;
        public double LifetimeRevenue;
        public double LifetimeProfit;
    }

    [Serializable]
    public class SettingsData
    {
        public float MusicVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public int QualityLevel = 1; // 0=Low, 1=Medium, 2=High, 3=Ultra
        public bool HapticsEnabled = true;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public string PlayerId;
        public string PlayerName = "Shopkeeper";
        public int TotalPlayTimeSeconds = 0;
    }

    /// <summary>
    /// Master root save object containing versioning, timestamps, and subsystem data.
    /// Serialized directly to local JSON storage.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int SaveVersion = 1;
        public string LastSaveTimeUtc;
        public PlayerSaveData Player = new PlayerSaveData();
        public CurrencyData Currency = new CurrencyData();
        public StoreData Store = new StoreData();
        public List<UpgradeSaveData> Upgrades = new List<UpgradeSaveData>();
        public List<ProductSaveData> Products = new List<ProductSaveData>();
        public EconomySaveData Economy = new EconomySaveData();
        public SettingsData Settings = new SettingsData();

        public GameSaveData()
        {
            LastSaveTimeUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
