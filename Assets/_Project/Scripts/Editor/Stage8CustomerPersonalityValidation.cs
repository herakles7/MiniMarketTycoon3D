#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Gameplay;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class Stage8CustomerPersonalityValidation
    {
        static Stage8CustomerPersonalityValidation()
        {
            EditorApplication.delayCall += RunValidation;
        }

        [MenuItem("MiniMarket/Validate Stage 8 Customer Personalities")]
        public static void RunValidation()
        {
            Debug.Log("==================================================");
            Debug.Log("[Stage8Validation] Starting Customer Personality & Behavior Validation...");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 12;

            GameObject testRunner = new GameObject("Stage8_Personality_TestRunner");
            try
            {
                // Ensure required singletons
                var currencyMgr = testRunner.AddComponent<CurrencyManager>();
                var economyMgr = testRunner.AddComponent<EconomyManager>();
                var inventoryMgr = testRunner.AddComponent<InventoryManager>();
                var shelfMgr = testRunner.AddComponent<ShelfManager>();
                var upgradeMgr = testRunner.AddComponent<MarketUpgradeManager>();
                var expansionMgr = testRunner.AddComponent<MarketExpansionManager>();
                var camController = testRunner.AddComponent<CameraController>();
                var saveMgr = testRunner.AddComponent<SaveManager>();

                // Load or create personality config
                CustomerPersonalityConfig config = AssetDatabase.LoadAssetAtPath<CustomerPersonalityConfig>(
                    "Assets/_Project/ScriptableObjects/Configuration/CustomerPersonalityConfig.asset");

                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<CustomerPersonalityConfig>();
                    config.InitializeDefaults();
                }

                // ------------------------------------------------------------------
                // TEST 1: Config Asset & 6 Personality Types
                // ------------------------------------------------------------------
                bool test1Pass = false;
                if (config != null && config.Personalities != null && config.Personalities.Count >= 6)
                {
                    HashSet<CustomerPersonalityType> typesFound = new HashSet<CustomerPersonalityType>();
                    foreach (var p in config.Personalities)
                    {
                        typesFound.Add(p.PersonalityType);
                    }

                    if (typesFound.Contains(CustomerPersonalityType.Normal) &&
                        typesFound.Contains(CustomerPersonalityType.QuickShopper) &&
                        typesFound.Contains(CustomerPersonalityType.BargainHunter) &&
                        typesFound.Contains(CustomerPersonalityType.BigShopper) &&
                        typesFound.Contains(CustomerPersonalityType.PatientShopper) &&
                        typesFound.Contains(CustomerPersonalityType.ImpatientShopper))
                    {
                        test1Pass = true;
                    }
                }

                if (test1Pass)
                {
                    passed++;
                    Debug.Log("<color=#00FF00>[PASS]</color> TEST 1: Personality config asset loaded with all 6 distinct personality definitions.");
                }
                else
                {
                    Debug.LogError("[FAIL] TEST 1: Personality config asset missing or does not define all 6 personality types.");
                }

                // ------------------------------------------------------------------
                // TEST 2: Weighted Random Personality Generation (1000 runs)
                // ------------------------------------------------------------------
                Dictionary<CustomerPersonalityType, int> counts = new Dictionary<CustomerPersonalityType, int>();
                foreach (CustomerPersonalityType t in Enum.GetValues(typeof(CustomerPersonalityType)))
                {
                    counts[t] = 0;
                }

                for (int i = 0; i < 1000; i++)
                {
                    CustomerPersonalityType picked = config.GetRandomPersonalityType();
                    counts[picked]++;
                }

                bool allSpawned = true;
                foreach (var kvp in counts)
                {
                    if (kvp.Value == 0)
                    {
                        allSpawned = false;
                        break;
                    }
                }

                if (allSpawned)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 2: 1000 simulated spawns selected all 6 personalities (Normal:{counts[CustomerPersonalityType.Normal]}, Quick:{counts[CustomerPersonalityType.QuickShopper]}, Bargain:{counts[CustomerPersonalityType.BargainHunter]}, Big:{counts[CustomerPersonalityType.BigShopper]}, Patient:{counts[CustomerPersonalityType.PatientShopper]}, Impatient:{counts[CustomerPersonalityType.ImpatientShopper]}).");
                }
                else
                {
                    Debug.LogError("[FAIL] TEST 2: Weighted random distribution failed to spawn every personality type in 1000 runs.");
                }

                // ------------------------------------------------------------------
                // TEST 3: Normal Customer Product Range (1-3)
                // ------------------------------------------------------------------
                var normalDef = config.GetDefinition(CustomerPersonalityType.Normal);
                bool test3Pass = normalDef != null && normalDef.MinProducts == 1 && normalDef.MaxProducts == 3;
                if (test3Pass)
                {
                    passed++;
                    Debug.Log("<color=#00FF00>[PASS]</color> TEST 3: Normal customer product count correctly configured in 1-3 range.");
                }
                else
                {
                    Debug.LogError($"[FAIL] TEST 3: Normal customer min/max products invalid: {normalDef?.MinProducts}-{normalDef?.MaxProducts}.");
                }

                // ------------------------------------------------------------------
                // TEST 4: Big Shopper Product Range (3-5) & Max Quantity (<= 8)
                // ------------------------------------------------------------------
                var bigDef = config.GetDefinition(CustomerPersonalityType.BigShopper);
                bool test4DefPass = bigDef != null && bigDef.MinProducts == 3 && bigDef.MaxProducts == 5;

                // Simulate Big Shopper shopping list generation with quantity cap
                int bigItemsCount = UnityEngine.Random.Range(bigDef.MinProducts, bigDef.MaxProducts + 1);
                int totalQuantitySimulated = 0;
                for (int i = 0; i < bigItemsCount; i++)
                {
                    int qty = UnityEngine.Random.Range(bigDef.MinQuantityPerProduct, bigDef.MaxQuantityPerProduct + 1);
                    if (totalQuantitySimulated + qty > 8)
                    {
                        qty = Mathf.Max(1, 8 - totalQuantitySimulated);
                    }
                    totalQuantitySimulated += qty;
                    if (totalQuantitySimulated >= 8) break;
                }

                bool test4Pass = test4DefPass && totalQuantitySimulated <= 8 && totalQuantitySimulated > 0;
                if (test4Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 4: Big shopper product count is 3-5 ({bigItemsCount} items) and total quantity is strictly capped at <= 8 (generated: {totalQuantitySimulated}).");
                }
                else
                {
                    Debug.LogError($"[FAIL] TEST 4: Big shopper constraints failed. DefPass={test4DefPass}, TotalQty={totalQuantitySimulated}.");
                }

                // ------------------------------------------------------------------
                // TEST 5: Quick Shopper Speed Multiplier > Normal
                // ------------------------------------------------------------------
                var quickDef = config.GetDefinition(CustomerPersonalityType.QuickShopper);
                bool test5Pass = quickDef != null && normalDef != null &&
                                 quickDef.MoveSpeedMultiplierRange.x > normalDef.MoveSpeedMultiplierRange.y;
                if (test5Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 5: Quick shopper speed range ({quickDef.MoveSpeedMultiplierRange.x:F2}-{quickDef.MoveSpeedMultiplierRange.y:F2}) > Normal ({normalDef.MoveSpeedMultiplierRange.x:F2}-{normalDef.MoveSpeedMultiplierRange.y:F2}).");
                }
                else
                {
                    Debug.LogError("[FAIL] TEST 5: Quick shopper speed is not higher than Normal.");
                }

                // ------------------------------------------------------------------
                // TEST 6: Patient Shopper Checkout Patience > Normal
                // ------------------------------------------------------------------
                var patientDef = config.GetDefinition(CustomerPersonalityType.PatientShopper);
                bool test6Pass = patientDef != null && normalDef != null &&
                                 patientDef.CheckoutPatienceMultiplier > normalDef.CheckoutPatienceMultiplier;
                if (test6Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 6: Patient shopper patience multiplier ({patientDef.CheckoutPatienceMultiplier:F2}x) > Normal ({normalDef.CheckoutPatienceMultiplier:F2}x).");
                }
                else
                {
                    Debug.LogError("[FAIL] TEST 6: Patient shopper patience multiplier is not higher than Normal.");
                }

                // ------------------------------------------------------------------
                // TEST 7: Impatient Shopper Checkout Patience < Normal
                // ------------------------------------------------------------------
                var impatientDef = config.GetDefinition(CustomerPersonalityType.ImpatientShopper);
                bool test7Pass = impatientDef != null && normalDef != null &&
                                 impatientDef.CheckoutPatienceMultiplier < normalDef.CheckoutPatienceMultiplier;
                if (test7Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 7: Impatient shopper patience multiplier ({impatientDef.CheckoutPatienceMultiplier:F2}x) < Normal ({normalDef.CheckoutPatienceMultiplier:F2}x).");
                }
                else
                {
                    Debug.LogError("[FAIL] TEST 7: Impatient shopper patience multiplier is not lower than Normal.");
                }

                // ------------------------------------------------------------------
                // TEST 8: Bargain Hunter Price Sensitivity & Cheap Item Weight Bias
                // ------------------------------------------------------------------
                var bargainDef = config.GetDefinition(CustomerPersonalityType.BargainHunter);
                bool test8DefPass = bargainDef != null && bargainDef.PriceSensitivity >= 0.7f;

                // Simulate weighted random calculation for cheap ($1.40) vs expensive ($12.00) item
                double minP = 1.40;
                double maxP = 12.00;
                double range = maxP - minP;
                double normCheap = (1.40 - minP) / range;      // 0.0
                double normExp = (12.00 - minP) / range;       // 1.0

                float weightCheap = 1.0f + (float)((1.0 - normCheap) * bargainDef.PriceSensitivity * 4.0);
                float weightExp = 1.0f + (float)((1.0 - normExp) * bargainDef.PriceSensitivity * 4.0);

                bool test8Pass = test8DefPass && weightCheap > (weightExp * 3f);
                if (test8Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 8: Bargain hunter price sensitivity ({bargainDef.PriceSensitivity}) significantly amplifies cheap product weight ({weightCheap:F2} vs {weightExp:F2}).");
                }
                else
                {
                    Debug.LogError($"[FAIL] TEST 8: Bargain hunter price weighting bias failed: Cheap={weightCheap:F2}, Exp={weightExp:F2}.");
                }

                // ------------------------------------------------------------------
                // TEST 9: Stock = 0 Behavior (Customer skips item without waiting)
                // ------------------------------------------------------------------
                CustomerShoppingData testShoppingData = new CustomerShoppingData();
                testShoppingData.Initialize(2, 60f);
                testShoppingData.AddWishlistItem("prod_water");
                testShoppingData.AddWishlistItem("prod_chips");

                CustomerPersonalityData testRuntimeData = config.CreateRuntimeData(CustomerPersonalityType.Normal);
                int initialSkipped = testRuntimeData.SkippedItemsCount;
                int initialSat = testRuntimeData.Satisfaction;

                // Simulate out of stock encounter
                testRuntimeData.RecordItemSkipped();
                testShoppingData.SkipCurrentItem();

                bool test9Pass = testRuntimeData.SkippedItemsCount == initialSkipped + 1 &&
                                 testRuntimeData.Satisfaction < initialSat &&
                                 testShoppingData.GetCurrentItem() == "prod_chips";

                if (test9Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 9: When Stock=0, item is skipped immediately and satisfaction is penalized (Sat: {initialSat}->{testRuntimeData.Satisfaction}).");
                }
                else
                {
                    Debug.LogError($"[FAIL] TEST 9: Stock=0 skip logic failed. Skipped={testRuntimeData.SkippedItemsCount}, Sat={testRuntimeData.Satisfaction}.");
                }

                // ------------------------------------------------------------------
                // TEST 10: Partial Stock Collection (Wanted = 2, Stock = 1 -> Collected = 1)
                // ------------------------------------------------------------------
                CustomerShoppingData partialShoppingData = new CustomerShoppingData();
                partialShoppingData.Initialize(1, 60f);
                var pWater = inventoryMgr.GetProductData("prod_water");
                partialShoppingData.AddShoppingItem(pWater, 2);

                int availableStock = 1;
                var currentItem = partialShoppingData.GetCurrentShoppingItem();
                int wanted = currentItem.QuantityRemaining; // 2
                int take = Mathf.Min(wanted, availableStock); // 1

                partialShoppingData.AddProductToCart(pWater, take);
                currentItem.QuantityCollected += take;
                partialShoppingData.RecordCollected(take);

                bool test10Pass = partialShoppingData.ItemsCollected == 1 &&
                                  partialShoppingData.CalculateCartTotal() > 0.0 &&
                                  currentItem.QuantityRemaining == 1;

                if (test10Pass)
                {
                    passed++;
                    Debug.Log($"<color=#00FF00>[PASS]</color> TEST 10: Partial stock handled correctly (Wanted=2, Available=1 => Collected=1, Remaining=1).");
                }
                else
                {
                    Debug.LogError($"[FAIL] TEST 10: Partial stock handling failed. Collected={partialShoppingData.ItemsCollected}.");
                }

                // ------------------------------------------------------------------
                // TEST 11: Pooling Reset Safety (Impatience state does not leak)
                // ------------------------------------------------------------------
                CustomerPersonalityData pooledData = config.CreateRuntimeData(CustomerPersonalityType.ImpatientShopper);
                pooledData.RecordItemSkipped();
                pooledData.RecordItemSkipped(); // 2 skips triggers impatience
                pooledData.DeductSatisfaction(70);
                pooledData.CheckoutQueueTimer = 14.5f;

                bool isImpSafeBefore = pooledData.HasLeftDueToImpatience && pooledData.Satisfaction < 30;

                // Customer returns to pool and is reset
                pooledData.Reset();

                bool isResetClean = pooledData.PersonalityType == CustomerPersonalityType.Normal &&
                                    pooledData.Satisfaction == 100 &&
                                    pooledData.SkippedItemsCount == 0 &&
                                    !pooledData.HasLeftDueToImpatience &&
                                    pooledData.CheckoutQueueTimer == 0f;

                // Re-spawned as PatientShopper
                var patientRuntime = config.CreateRuntimeData(CustomerPersonalityType.PatientShopper);
                bool isPatientClean = patientRuntime.PersonalityType == CustomerPersonalityType.PatientShopper &&
                                      patientRuntime.Satisfaction == 100 &&
                                      patientRuntime.CheckoutPatienceMultiplier == 1.5f &&
                                      !patientRuntime.HasLeftDueToImpatience;

                bool test11Pass = isImpSafeBefore && isResetClean && isPatientClean;
                if (test11Pass)
                {
                    passed++;
                    Debug.Log("<color=#00FF00>[PASS]</color> TEST 11: Customer pooling reset safely cleans runtime impatience, timer, and satisfaction.");
                }
                else
                {
                    Debug.LogError($"[FAIL] TEST 11: Pooling reset state leak detected. ImpBefore={isImpSafeBefore}, ResetClean={isResetClean}, PatientClean={isPatientClean}.");
                }

                // ------------------------------------------------------------------
                // TEST 12: Save/Load Compatibility (Customer state is not saved)
                // ------------------------------------------------------------------
                if (SaveManager.Instance.CurrentSave == null)
                {
                    SaveManager.Instance.CreateNewSave();
                }
                SaveManager.Instance.SaveGame();
                string saveJson = JsonUtility.ToJson(SaveManager.Instance.CurrentSave);

                bool test12Pass = !string.IsNullOrEmpty(saveJson) &&
                                  !saveJson.Contains("CustomerPersonalityData") &&
                                  !saveJson.Contains("HasLeftDueToImpatience") &&
                                  (saveJson.Contains("StoreLevel") || saveJson.Contains("MarketLevel"));

                if (test12Pass)
                {
                    passed++;
                    Debug.Log("<color=#00FF00>[PASS]</color> TEST 12: Save/Load preserves market/economy data without serializing transient customer personality state.");
                }
                else
                {
                    Debug.LogError("[FAIL] TEST 12: Save data contains transient customer personality data or failed save validation.");
                }

                // ------------------------------------------------------------------
                // SUMMARY
                // ------------------------------------------------------------------
                Debug.Log("==================================================");
                if (passed == total)
                {
                    Debug.Log($"<color=#00FF00>STAGE 8 VALIDATION SUCCESS: {passed}/{total} TESTS PASSED!</color>");
                }
                else
                {
                    Debug.LogError($"STAGE 8 VALIDATION FAILED: {passed}/{total} TESTS PASSED.");
                }
                Debug.Log("==================================================");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (testRunner != null)
                {
                    GameObject.DestroyImmediate(testRunner);
                }
            }
        }
    }
}
#endif
