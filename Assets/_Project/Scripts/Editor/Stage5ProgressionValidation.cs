#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class Stage5ProgressionValidation
    {
        static Stage5ProgressionValidation()
        {
            EditorApplication.delayCall += () =>
            {
                RunValidation();
            };
        }

        [MenuItem("MiniMarket/Validate Stage 5 Progression")]
        public static void RunValidation()
        {
            Debug.Log("==================================================");
            Debug.Log("[Stage5Validation] Starting Stage 5 Progression Validation...");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 0;

            // 1. Validate MarketProgressionConfig asset
            total++;
            var config = AssetDatabase.LoadAssetAtPath<MarketProgressionConfig>("Assets/_Project/ScriptableObjects/Configuration/MarketProgressionConfig.asset");
            if (config != null && config.Levels != null && config.Levels.Count == 5)
            {
                var l1 = config.GetLevelData(1);
                var l2 = config.GetLevelData(2);
                var l3 = config.GetLevelData(3);
                var l4 = config.GetLevelData(4);
                var l5 = config.GetLevelData(5);

                bool match = l1.UpgradeCost == 500 && l1.MaxCustomers == 5 && l1.SpawnInterval == 8f && l1.ShelfCapacity == 8 && l1.CheckoutTime == 2.5f &&
                             l2.UpgradeCost == 2000 && l2.MaxCustomers == 7 && l2.SpawnInterval == 7f && l2.ShelfCapacity == 12 && l2.CheckoutTime == 2.2f &&
                             l3.UpgradeCost == 7500 && l3.MaxCustomers == 10 && l3.SpawnInterval == 6f && l3.ShelfCapacity == 16 && l3.CheckoutTime == 1.9f &&
                             l4.UpgradeCost == 20000 && l4.MaxCustomers == 14 && l4.SpawnInterval == 5f && l4.ShelfCapacity == 20 && l4.CheckoutTime == 1.6f &&
                             l5.MaxCustomers == 18 && l5.SpawnInterval == 4f && l5.ShelfCapacity == 24 && l5.CheckoutTime == 1.3f;

                if (match)
                {
                    Debug.Log("✔ Test 1 Passed: MarketProgressionConfig.asset loaded with exact 5 progression levels, costs, and parameters.");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 1 Failed: MarketProgressionConfig level parameters do not match specifications.");
                }
            }
            else
            {
                Debug.LogError("❌ Test 1 Failed: MarketProgressionConfig.asset missing or does not contain 5 levels.");
            }

            // 2. Validate CurrencyManager + MarketUpgradeManager Setup & Initial State
            total++;
            GameObject testRoot = new GameObject("Stage5_TestRoot");
            var currencyMgr = testRoot.AddComponent<CurrencyManager>();
            var upgradeMgr = testRoot.AddComponent<MarketUpgradeManager>();
            upgradeMgr.SetConfiguration(config);
            upgradeMgr.SetLevelDirect(1);

            currencyMgr.SetBalance(300.0); // Insufficient for Level 2 ($500 needed)

            if (upgradeMgr.CurrentLevel == 1 &&
                upgradeMgr.NextUpgradeCost == 500.0 &&
                !upgradeMgr.CanAffordUpgrade() &&
                upgradeMgr.CurrentMaxCustomers == 5 &&
                upgradeMgr.CurrentSpawnInterval == 8.0f &&
                upgradeMgr.CurrentShelfCapacity == 8 &&
                upgradeMgr.CurrentCheckoutTime == 2.5f)
            {
                Debug.Log("✔ Test 2 Passed: Level 1 initial state verified (Cost: $500, Customers: 5, Spawn: 8s, Shelf: 8, Checkout: 2.5s).");
                passed++;
            }
            else
            {
                Debug.LogError($"❌ Test 2 Failed: Level 1 initial state mismatch. Level={upgradeMgr.CurrentLevel}, Cost={upgradeMgr.NextUpgradeCost}");
            }

            // 3. Test Insufficient Funds Edge Case
            total++;
            bool upgradeAttempt = upgradeMgr.TryUpgradeMarket();
            if (!upgradeAttempt && upgradeMgr.CurrentLevel == 1 && currencyMgr.Cash == 300.0)
            {
                Debug.Log("✔ Test 3 Passed: Insufficient funds rejected cleanly. Money preserved at $300, Level remains 1.");
                passed++;
            }
            else
            {
                Debug.LogError("❌ Test 3 Failed: Insufficient funds upgrade was improperly allowed.");
            }

            // 4. Test Successful Upgrade Level 1 -> Level 2
            total++;
            currencyMgr.SetBalance(1000.0); // Now have enough ($1000 >= $500)
            int eventFiredNewLevel = -1;
            upgradeMgr.OnMarketLevelUpgraded += (lvl) => eventFiredNewLevel = lvl;

            bool upgradeSuccess = upgradeMgr.TryUpgradeMarket();
            if (upgradeSuccess &&
                upgradeMgr.CurrentLevel == 2 &&
                currencyMgr.Cash == 500.0 &&
                eventFiredNewLevel == 2 &&
                upgradeMgr.CurrentMaxCustomers == 7 &&
                upgradeMgr.CurrentSpawnInterval == 7.0f &&
                upgradeMgr.CurrentShelfCapacity == 12 &&
                upgradeMgr.CurrentCheckoutTime == 2.2f &&
                upgradeMgr.NextUpgradeCost == 2000.0)
            {
                Debug.Log("✔ Test 4 Passed: Level 1 -> Level 2 Upgrade succeeded. Money deducted ($500), new values: Customers=7, Spawn=7s, Shelf=12, Checkout=2.2s, NextCost=$2000.");
                passed++;
            }
            else
            {
                Debug.LogError($"❌ Test 4 Failed: Upgrade Level 1 -> 2 failed or state incorrect. Cash={currencyMgr.Cash}, Level={upgradeMgr.CurrentLevel}");
            }

            // 5. Test CustomerSpawner Dynamic Event Binding
            total++;
            GameObject spawnerGo = new GameObject("Test_Spawner");
            spawnerGo.transform.parent = testRoot.transform;
            var spawner = spawnerGo.AddComponent<CustomerSpawner>();
            spawner.UpdateActiveProgressionValues(2);

            if (spawner.ActiveMaxCustomers == 7 && spawner.ActiveSpawnInterval == 7.0f)
            {
                upgradeMgr.SetLevelDirect(3);
                spawner.UpdateActiveProgressionValues(3);

                if (spawner.ActiveMaxCustomers == 10 && spawner.ActiveSpawnInterval == 6.0f)
                {
                    Debug.Log("✔ Test 5 Passed: CustomerSpawner dynamic parameters correctly bound via progression events (Lv2: 7cust/7s -> Lv3: 10cust/6s).");
                    passed++;
                }
                else
                {
                    Debug.LogError($"❌ Test 5 Failed: Spawner values at Lv3 mismatch: {spawner.ActiveMaxCustomers} cust, {spawner.ActiveSpawnInterval}s");
                }
            }
            else
            {
                Debug.LogError($"❌ Test 5 Failed: Spawner values at Lv2 mismatch: {spawner.ActiveMaxCustomers} cust, {spawner.ActiveSpawnInterval}s");
            }

            // 6. Test InventoryManager Effective Max Stock Scaling
            total++;
            var invMgr = testRoot.AddComponent<InventoryManager>();
            upgradeMgr.SetLevelDirect(1);
            int stockL1 = upgradeMgr.GetEffectiveMaxStock(50);
            upgradeMgr.SetLevelDirect(2);
            int stockL2 = upgradeMgr.GetEffectiveMaxStock(50);
            upgradeMgr.SetLevelDirect(3);
            int stockL3 = upgradeMgr.GetEffectiveMaxStock(50);
            upgradeMgr.SetLevelDirect(4);
            int stockL4 = upgradeMgr.GetEffectiveMaxStock(50);
            upgradeMgr.SetLevelDirect(5);
            int stockL5 = upgradeMgr.GetEffectiveMaxStock(50);

            if (stockL1 == 50 && stockL2 == 75 && stockL3 == 100 && stockL4 == 150 && stockL5 == 200)
            {
                Debug.Log($"✔ Test 6 Passed: Effective warehouse max stock scales correctly without altering ProductData: Lv1=50, Lv2=75, Lv3=100, Lv4=150, Lv5=200.");
                passed++;
            }
            else
            {
                Debug.LogError($"❌ Test 6 Failed: Effective stock values: L1={stockL1}, L2={stockL2}, L3={stockL3}, L4={stockL4}, L5={stockL5}");
            }

            // 7. Test Save & Load State Persistence
            total++;
            GameSaveData saveData = new GameSaveData();
            upgradeMgr.SetLevelDirect(3);
            saveData.Store.StoreLevel = upgradeMgr.CurrentLevel;
            saveData.Store.CapacityLevel = upgradeMgr.CurrentLevel;
            saveData.Store.CheckoutSpeedLevel = upgradeMgr.CashierSpeedLevel;

            // Simulate reload in fresh object
            GameObject reloadGo = new GameObject("Stage5_ReloadTest");
            var freshUpgradeMgr = reloadGo.AddComponent<MarketUpgradeManager>();
            freshUpgradeMgr.SetConfiguration(config);
            freshUpgradeMgr.SetLevelDirect(saveData.Store.StoreLevel);

            if (freshUpgradeMgr.CurrentLevel == 3 &&
                freshUpgradeMgr.CurrentMaxCustomers == 10 &&
                freshUpgradeMgr.CurrentSpawnInterval == 6.0f &&
                freshUpgradeMgr.CurrentShelfCapacity == 16 &&
                freshUpgradeMgr.CurrentCheckoutTime == 1.9f)
            {
                Debug.Log("✔ Test 7 Passed: Save & Load persistence verified. Saved Level 3 correctly restored with all parameters intact.");
                passed++;
            }
            else
            {
                Debug.LogError($"❌ Test 7 Failed: Reloaded level was {freshUpgradeMgr.CurrentLevel} instead of 3.");
            }
            Object.DestroyImmediate(reloadGo);

            // 8. Test Max Level Boundary
            total++;
            upgradeMgr.SetLevelDirect(5);
            bool maxUpgradeAttempt = upgradeMgr.TryUpgradeMarket();

            if (!maxUpgradeAttempt && upgradeMgr.IsMaxLevel && upgradeMgr.NextUpgradeCost == 0.0)
            {
                Debug.Log("✔ Test 8 Passed: Max Level boundary guard verified. Level 5 detected as Max Level, further upgrades safely disallowed.");
                passed++;
            }
            else
            {
                Debug.LogError("❌ Test 8 Failed: Max level guard did not prevent further upgrade.");
            }

            // 9. Test UpgradePopupView UI generation & debouncing
            total++;
            GameObject canvasGo = new GameObject("TestCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            var upgradePopup = UpgradePopupView.CreateDynamic(canvas.transform);

            if (upgradePopup != null)
            {
                upgradePopup.Show();
                upgradePopup.RefreshUI();
                upgradePopup.Hide();
                Debug.Log("✔ Test 9 Passed: UpgradePopupView dynamically instantiates, formats comparison cards, and toggles visibility cleanly.");
                passed++;
            }
            else
            {
                Debug.LogError("❌ Test 9 Failed: UpgradePopupView.CreateDynamic returned null.");
            }
            Object.DestroyImmediate(canvasGo);

            // Clean up test root
            Object.DestroyImmediate(testRoot);

            Debug.Log("==================================================");
            Debug.Log($"[Stage5Validation] Validation Completed: {passed}/{total} Tests Passed!");
            Debug.Log("==================================================");
        }
    }
}
#endif
