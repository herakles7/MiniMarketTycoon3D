using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MiniMarketTycoon.Core;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Editor
{
    public static class Stage4VerificationRunner
    {
        [MenuItem("MiniMarket/Run Stage 4 Verification Test")]
        public static void RunVerification()
        {
            Debug.Log("==================================================");
            Debug.Log("[STAGE 4 VERIFICATION] Starting Real Economy & Sales Verification...");
            Debug.Log("==================================================");

            int passed = 0;
            int failed = 0;

            // ---------------------------------------------------------
            // TEST 1: Product Data Assets Integrity
            // ---------------------------------------------------------
            string[] expectedProducts = { "prod_water", "prod_milk", "prod_chips", "prod_chocolate", "prod_soda", "prod_canned" };
            double[] expectedBuy = { 1.00, 1.50, 1.20, 1.00, 1.10, 1.80 };
            double[] expectedSell = { 1.50, 2.20, 2.00, 1.80, 1.90, 3.00 };

            var catalogGO = new GameObject("Test_InventoryManager");
            var invMgr = catalogGO.AddComponent<InventoryManager>();
            
            // Allow Awake & OnInitialized
            invMgr.SendMessage("OnInitialized", SendMessageOptions.DontRequireReceiver);

            var catalog = invMgr.ProductCatalog;
            if (catalog != null && catalog.Count >= 6)
            {
                Debug.Log($"[TEST 1 PASS] Product Catalog loaded with {catalog.Count} products.");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 1 FAIL] Expected >=6 products, found: {catalog?.Count ?? 0}");
                failed++;
            }

            for (int i = 0; i < expectedProducts.Length; i++)
            {
                var prod = invMgr.GetProductData(expectedProducts[i]);
                if (prod != null && Math.Abs(prod.PurchasePrice - expectedBuy[i]) < 0.001 && Math.Abs(prod.SellPrice - expectedSell[i]) < 0.001)
                {
                    Debug.Log($"[TEST 1.{i+1} PASS] Product {expectedProducts[i]}: Buy=${prod.PurchasePrice:F2}, Sell=${prod.SellPrice:F2}, Profit/Unit=${prod.ProfitPerUnit:F2}");
                    passed++;
                }
                else
                {
                    Debug.LogError($"[TEST 1.{i+1} FAIL] Mismatch for {expectedProducts[i]}. Found: Buy=${prod?.PurchasePrice ?? 0:F2}, Sell=${prod?.SellPrice ?? 0:F2}");
                    failed++;
                }
            }

            // ---------------------------------------------------------
            // TEST 2: Currency Manager Initialization & Restock Cash Deduction
            // ---------------------------------------------------------
            var currGO = new GameObject("Test_CurrencyManager");
            var currMgr = currGO.AddComponent<CurrencyManager>();
            currMgr.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            currMgr.SetBalance(1000.0);

            if (Math.Abs(currMgr.Cash - 1000.0) < 0.001)
            {
                Debug.Log($"[TEST 2 PASS] Starting player cash initialized: ${currMgr.Cash:F2}");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 2 FAIL] Expected $1000.00, got ${currMgr.Cash:F2}");
                failed++;
            }

            // Test Restock Water: 10 units @ $1.00
            int initialWaterStock = invMgr.GetStock("prod_water");
            bool restocked = invMgr.TryRestock("prod_water", 10, out int actualRestocked, out double totalCost);

            if (restocked && actualRestocked == 10 && Math.Abs(totalCost - 10.0) < 0.001)
            {
                currMgr.RemoveCurrency(totalCost);
                int newWaterStock = invMgr.GetStock("prod_water");
                Debug.Log($"[TEST 3 PASS] Restock Water +10: Cost=${totalCost:F2}, Stock {initialWaterStock}->{newWaterStock}, Player Cash=${currMgr.Cash:F2}");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 3 FAIL] Restock Water failed. ActualRestocked={actualRestocked}, Cost={totalCost}");
                failed++;
            }

            // ---------------------------------------------------------
            // TEST 4: MaxStock Boundary Protection
            // ---------------------------------------------------------
            int maxStock = invMgr.GetMaxStock("prod_water");
            int curStock = invMgr.GetStock("prod_water");
            int overBuy = 100;
            bool overRestock = invMgr.TryRestock("prod_water", overBuy, out int overActual, out double overCost);
            int cappedStock = invMgr.GetStock("prod_water");

            if (cappedStock == maxStock)
            {
                Debug.Log($"[TEST 4 PASS] MaxStock enforced: stock capped at {cappedStock}/{maxStock}. Over-purchased items rejected.");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 4 FAIL] Stock exceeded MaxStock! Current={cappedStock}, Max={maxStock}");
                failed++;
            }

            // Attempting to buy when stock is full
            bool fullRestock = invMgr.TryRestock("prod_water", 10, out int fullActual, out double fullCost);
            if (!fullRestock && fullActual == 0)
            {
                Debug.Log("[TEST 5 PASS] Restock blocked when stock is 100% full.");
                passed++;
            }
            else
            {
                Debug.LogError("[TEST 5 FAIL] Restock succeeded when stock was already full!");
                failed++;
            }

            // ---------------------------------------------------------
            // TEST 6: Customer Shopping, Cart Accumulation & Stock Consumption
            // ---------------------------------------------------------
            var custData = new CustomerShoppingData();
            custData.Initialize(3, 60f);

            var waterData = invMgr.GetProductData("prod_water");
            var milkData = invMgr.GetProductData("prod_milk");

            // Customer takes 2 Water ($1.50 ea) and 1 Milk ($2.20 ea)
            bool c1 = invMgr.TryConsumeStock("prod_water", 1);
            custData.AddProductToCart(waterData, 1);
            custData.MarkCurrentItemCollected();

            bool c2 = invMgr.TryConsumeStock("prod_water", 1);
            custData.AddProductToCart(waterData, 1);
            custData.MarkCurrentItemCollected();

            bool c3 = invMgr.TryConsumeStock("prod_milk", 1);
            custData.AddProductToCart(milkData, 1);
            custData.MarkCurrentItemCollected();

            double expectedRevenue = (2 * 1.50) + (1 * 2.20); // $5.20
            double expectedCost = (2 * 1.00) + (1 * 1.50);    // $3.50
            double expectedProfit = expectedRevenue - expectedCost; // $1.70

            double cartTotal = custData.CalculateCartTotal();
            double cartCost = custData.CalculateCartCost();
            double cartProfit = cartTotal - cartCost;

            if (Math.Abs(cartTotal - expectedRevenue) < 0.001 && Math.Abs(cartCost - expectedCost) < 0.001)
            {
                Debug.Log($"[TEST 6 PASS] Customer cart totals calculated correctly: Revenue=${cartTotal:F2}, Cost=${cartCost:F2}, Profit=${cartProfit:F2}");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 6 FAIL] Cart total mismatch. Expected=${expectedRevenue:F2}/${expectedCost:F2}, Got=${cartTotal:F2}/${cartCost:F2}");
                failed++;
            }

            // ---------------------------------------------------------
            // TEST 7: Checkout Payment & Economy Accounting
            // ---------------------------------------------------------
            var ecoGO = new GameObject("Test_EconomyManager");
            var ecoMgr = ecoGO.AddComponent<EconomyManager>();
            ecoMgr.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            double cashBeforeCheckout = currMgr.Cash;
            currMgr.AddCurrency(cartTotal);
            ecoMgr.RecordExpense(10.0); // The restock from earlier
            ecoMgr.RecordSale(cartTotal, cartCost, custData.ItemsCollected);

            double cashAfterCheckout = currMgr.Cash;
            double netDailyProfit = ecoMgr.TodayProfit;

            if (Math.Abs(cashAfterCheckout - (cashBeforeCheckout + cartTotal)) < 0.001 &&
                Math.Abs(ecoMgr.TodayRevenue - cartTotal) < 0.001 &&
                Math.Abs(ecoMgr.TodayExpenses - 10.0) < 0.001 &&
                ecoMgr.ItemsSold == 3 && ecoMgr.CustomersServed == 1)
            {
                Debug.Log($"[TEST 7 PASS] Checkout payment executed: Cash=${cashAfterCheckout:F2}, Revenue=${ecoMgr.TodayRevenue:F2}, Expenses=${ecoMgr.TodayExpenses:F2}, Profit=${netDailyProfit:F2}, ItemsSold={ecoMgr.ItemsSold}");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 7 FAIL] Checkout accounting mismatch!");
                failed++;
            }

            // ---------------------------------------------------------
            // TEST 8: Out-of-Stock Fallback (No infinite stalling)
            // ---------------------------------------------------------
            // Drain milk to 0
            while (invMgr.GetStock("prod_milk") > 0)
            {
                invMgr.TryConsumeStock("prod_milk", 1);
            }

            bool milkStockCheck = invMgr.HasStock("prod_milk");
            bool canConsumeMilk = invMgr.TryConsumeStock("prod_milk", 1);

            var emptyCustData = new CustomerShoppingData();
            emptyCustData.Initialize(1, 60f);
            emptyCustData.AddWishlistItem("prod_milk");

            if (!milkStockCheck && !canConsumeMilk)
            {
                // Customer skips depleted item
                emptyCustData.SkipCurrentItem();
                bool hasPending = emptyCustData.HasPendingItems();
                if (!hasPending && emptyCustData.ItemsCollected == 0)
                {
                    Debug.Log("[TEST 8 PASS] Out-of-stock customer gracefully skips item and exits without hanging.");
                    passed++;
                }
                else
                {
                    Debug.LogError("[TEST 8 FAIL] Customer did not handle empty item correctly.");
                    failed++;
                }
            }
            else
            {
                Debug.LogError("[TEST 8 FAIL] Stock draining failed.");
                failed++;
            }

            // ---------------------------------------------------------
            // TEST 9: Shelf Visual Controller Stock Density Sync
            // ---------------------------------------------------------
            var shelfGO = new GameObject("Test_Shelf");
            var shelfVis = shelfGO.AddComponent<ShelfVisualController>();
            var visualItems = new List<GameObject>();
            for (int i = 0; i < 8; i++)
            {
                var item = new GameObject($"Item_{i}");
                item.transform.parent = shelfGO.transform;
                visualItems.Add(item);
            }
            shelfVis.Initialize("prod_water", visualItems);

            // Full stock (50/50) -> 8 items visible
            shelfVis.UpdateVisualDisplay(50, 50);
            int activeCountFull = visualItems.FindAll(x => x.activeSelf).Count;

            // Half stock (25/50) -> 4 items visible
            shelfVis.UpdateVisualDisplay(25, 50);
            int activeCountHalf = visualItems.FindAll(x => x.activeSelf).Count;

            // Empty stock (0/50) -> 0 items visible
            shelfVis.UpdateVisualDisplay(0, 50);
            int activeCountEmpty = visualItems.FindAll(x => x.activeSelf).Count;

            if (activeCountFull == 8 && activeCountHalf == 4 && activeCountEmpty == 0)
            {
                Debug.Log($"[TEST 9 PASS] Shelf visuals synchronize accurately with stock density: Full={activeCountFull}/8, Half={activeCountHalf}/8, Empty={activeCountEmpty}/8");
                passed++;
            }
            else
            {
                Debug.LogError($"[TEST 9 FAIL] Shelf visual density incorrect. Full={activeCountFull}, Half={activeCountHalf}, Empty={activeCountEmpty}");
                failed++;
            }

            // ---------------------------------------------------------
            // TEST 10: Save & Load Persistence Integration
            // ---------------------------------------------------------
            var saveGO = new GameObject("Test_SaveManager");
            var saveMgr = saveGO.AddComponent<SaveManager>();
            saveMgr.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);

            var saveData = saveMgr.CurrentSave;
            if (saveData != null)
            {
                // Trigger save sync
                saveData.Currency.Cash = currMgr.Cash;
                saveData.Economy = new EconomySaveData
                {
                    TodayRevenue = ecoMgr.TodayRevenue,
                    TodayExpenses = ecoMgr.TodayExpenses,
                    TodayProfit = ecoMgr.TodayProfit,
                    ItemsSold = ecoMgr.ItemsSold,
                    CustomersServed = ecoMgr.CustomersServed,
                    LifetimeRevenue = ecoMgr.LifetimeRevenue,
                    LifetimeProfit = ecoMgr.LifetimeProfit
                };
                saveData.Products = new List<ProductSaveData>
                {
                    new ProductSaveData("prod_water", true, invMgr.GetStock("prod_water")),
                    new ProductSaveData("prod_milk", true, invMgr.GetStock("prod_milk"))
                };

                saveMgr.SaveGame();
                saveMgr.LoadGame();
                var loaded = saveMgr.CurrentSave;

                if (loaded != null &&
                    Math.Abs(loaded.Currency.Cash - currMgr.Cash) < 0.001 &&
                    Math.Abs(loaded.Economy.TodayRevenue - ecoMgr.TodayRevenue) < 0.001 &&
                    loaded.Products.Find(p => p.ProductId == "prod_water").CurrentStock == invMgr.GetStock("prod_water"))
                {
                    Debug.Log("[TEST 10 PASS] Save & Load cycle verified. Currency, Economy and Stock persisted seamlessly.");
                    passed++;
                }
                else
                {
                    Debug.LogError("[TEST 10 FAIL] Load data verification failed.");
                    failed++;
                }
            }

            // Clean up temporary test game objects
            GameObject.DestroyImmediate(catalogGO);
            GameObject.DestroyImmediate(currGO);
            GameObject.DestroyImmediate(ecoGO);
            GameObject.DestroyImmediate(shelfGO);
            GameObject.DestroyImmediate(saveGO);

            Debug.Log("==================================================");
            Debug.Log($"[STAGE 4 VERIFICATION COMPLETE] Passed: {passed}, Failed: {failed}");
            Debug.Log("==================================================");
        }
    }
}
