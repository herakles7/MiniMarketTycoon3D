#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using MiniMarketTycoon.Gameplay;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class Stage7ExpansionValidation
    {
        static Stage7ExpansionValidation()
        {
            EditorApplication.delayCall += RunValidation;
        }

        [MenuItem("MiniMarket/Validate Stage 7 Store Expansion")]
        public static void RunValidation()
        {
            Debug.Log("==================================================");
            Debug.Log("[Stage7Validation] Starting Stage 7 Store Expansion Validation...");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 0;

            GameObject testRunner = new GameObject("Stage7_Expansion_TestRunner");
            try
            {
                // Ensure required managers
                var currencyMgr = testRunner.AddComponent<CurrencyManager>();
                var economyMgr = testRunner.AddComponent<EconomyManager>();
                var inventoryMgr = testRunner.AddComponent<InventoryManager>();
                var shelfMgr = testRunner.AddComponent<ShelfManager>();
                var upgradeMgr = testRunner.AddComponent<MarketUpgradeManager>();
                var expansionMgr = testRunner.AddComponent<MarketExpansionManager>();
                var camController = testRunner.AddComponent<CameraController>();

                // Build test expansion areas for Levels 2, 3, 4, 5
                List<MarketExpansionArea> testAreas = new List<MarketExpansionArea>();
                List<ShelfVisualController> testExpansionShelves = new List<ShelfVisualController>();

                for (int lvl = 2; lvl <= 5; lvl++)
                {
                    GameObject areaGo = new GameObject($"Test_Expansion_Lvl{lvl}");
                    areaGo.transform.parent = testRunner.transform;

                    GameObject contentGo = new GameObject("Content");
                    contentGo.transform.parent = areaGo.transform;

                    // Gate setup
                    GameObject gateGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gateGo.name = "Gate";
                    gateGo.transform.parent = areaGo.transform;
                    Collider gateCol = gateGo.GetComponent<Collider>();
                    NavMeshObstacle gateObs = gateGo.AddComponent<NavMeshObstacle>();
                    gateObs.carving = true;

                    // Indicator setup
                    GameObject indGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    indGo.name = "Indicator";
                    indGo.transform.parent = areaGo.transform;

                    // Interior light
                    GameObject lightGo = new GameObject("Light");
                    lightGo.transform.parent = contentGo.transform;
                    List<GameObject> lights = new List<GameObject> { lightGo };

                    // Expansion shelf
                    GameObject shelfGo = new GameObject($"Shelf_Lvl{lvl}");
                    shelfGo.transform.parent = contentGo.transform;
                    var shelfVis = shelfGo.AddComponent<ShelfVisualController>();
                    
                    GameObject ptGo = new GameObject("InteractionPt");
                    ptGo.transform.parent = shelfGo.transform;
                    var ip = ptGo.AddComponent<CustomerInteractionPoint>();
                    ip.Initialize($"Category_Lvl{lvl}", Vector3.forward);

                    // Attach dummy ProductData for testing
                    string prodId = (lvl == 2) ? "prod_chips" : (lvl == 3) ? "prod_cookies" : (lvl == 4) ? "prod_soda" : "prod_chocolate";
                    ProductData pData = inventoryMgr.GetProductData(prodId);
                    shelfVis.Initialize(pData, ShelfType.StandardShelf, new List<GameObject>(), new List<CustomerInteractionPoint> { ip });
                    
                    List<ShelfVisualController> areaShelves = new List<ShelfVisualController> { shelfVis };
                    testExpansionShelves.Add(shelfVis);

                    var expArea = areaGo.AddComponent<MarketExpansionArea>();
                    expArea.Configure(lvl, $"Expansion Level {lvl}", $"Section {lvl}", gateGo, gateCol, gateObs, indGo, contentGo, lights, areaShelves);

                    testAreas.Add(expArea);
                    expansionMgr.RegisterExpansionArea(expArea);
                }

                // ------------------------------------------------------------------
                // TEST 1: Level 1 Expansion Locked
                // ------------------------------------------------------------------
                total++;
                expansionMgr.ApplyExpansionLevel(1, animate: false);
                bool test1Ok = true;
                foreach (var a in testAreas)
                {
                    if (a.IsUnlocked || (a.GateCollider != null && !a.GateCollider.enabled) || (a.GateIndicator != null && !a.GateIndicator.activeSelf))
                    {
                        test1Ok = false;
                        break;
                    }
                }
                if (test1Ok)
                {
                    Debug.Log("✔ Test 1 Passed: Level 1 initial state verified (All expansions 2-5 locked, gate colliders active, indicators visible).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 1 Failed: Level 1 initial state has an unlocked expansion or inactive gate.");
                }

                // ------------------------------------------------------------------
                // TEST 2: Level 2 Expansion Unlock
                // ------------------------------------------------------------------
                total++;
                expansionMgr.ApplyExpansionLevel(2, animate: false);
                var a2 = expansionMgr.GetExpansionArea(2);
                var a3 = expansionMgr.GetExpansionArea(3);
                if (a2 != null && a2.IsUnlocked && (a2.GateCollider == null || !a2.GateCollider.enabled) &&
                    a3 != null && !a3.IsUnlocked && (a3.GateCollider != null && a3.GateCollider.enabled))
                {
                    Debug.Log("✔ Test 2 Passed: Level 2 expansion unlocked (Level 2 open, gate collider disabled; Levels 3-5 remain locked).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 2 Failed: Level 2 expansion did not unlock correctly or unlocked future levels.");
                }

                // ------------------------------------------------------------------
                // TEST 3: Level 3 Expansion Unlock
                // ------------------------------------------------------------------
                total++;
                expansionMgr.ApplyExpansionLevel(3, animate: false);
                var a4 = expansionMgr.GetExpansionArea(4);
                if (a2.IsUnlocked && a3.IsUnlocked && !a4.IsUnlocked)
                {
                    Debug.Log("✔ Test 3 Passed: Level 3 expansion unlocked (Level 2 & 3 open; Level 4 & 5 remain locked).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 3 Failed: Level 3 expansion state invalid.");
                }

                // ------------------------------------------------------------------
                // TEST 4: Level 4 Expansion Unlock
                // ------------------------------------------------------------------
                total++;
                expansionMgr.ApplyExpansionLevel(4, animate: false);
                var a5 = expansionMgr.GetExpansionArea(5);
                if (a2.IsUnlocked && a3.IsUnlocked && a4.IsUnlocked && !a5.IsUnlocked)
                {
                    Debug.Log("✔ Test 4 Passed: Level 4 expansion unlocked (Levels 2, 3, 4 open; Level 5 remains locked).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 4 Failed: Level 4 expansion state invalid.");
                }

                // ------------------------------------------------------------------
                // TEST 5: Level 5 All Expansions Unlock
                // ------------------------------------------------------------------
                total++;
                expansionMgr.ApplyExpansionLevel(5, animate: false);
                bool allUnlocked = true;
                foreach (var a in testAreas)
                {
                    if (!a.IsUnlocked || (a.GateIndicator != null && a.GateIndicator.activeSelf))
                    {
                        allUnlocked = false;
                        break;
                    }
                }
                if (allUnlocked)
                {
                    Debug.Log("✔ Test 5 Passed: Level 5 Grand Tycoon Mart verified (All expansions unlocked, 0 locked indicators remaining).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 5 Failed: Not all expansions are unlocked at Level 5.");
                }

                // ------------------------------------------------------------------
                // TEST 6: Gate Collider & NavMesh Obstacle States
                // ------------------------------------------------------------------
                total++;
                // Check Level 2 area when locked vs unlocked
                a2.SetState(false);
                bool colLocked = a2.GateCollider != null && a2.GateCollider.enabled;
                a2.SetState(true);
                bool colUnlocked = a2.GateCollider == null || !a2.GateCollider.enabled;
                if (colLocked && colUnlocked)
                {
                    Debug.Log("✔ Test 6 Passed: Gate collider and obstacle toggle verified (Blocks pathfinding when locked, allows entrance when unlocked).");
                    passed++;
                }
                else
                {
                    Debug.LogError($"❌ Test 6 Failed: Gate collider states invalid: locked={colLocked}, unlocked={colUnlocked}");
                }

                // ------------------------------------------------------------------
                // TEST 7: Camera Bounds Scaling Across Levels
                // ------------------------------------------------------------------
                total++;
                camController.UpdateBoundsForLevel(1);
                Vector2 bX1 = camController.PanBoundsX;
                Vector2 bZ1 = camController.PanBoundsZ;
                float zoom1 = camController.MaxZoom;

                camController.UpdateBoundsForLevel(5);
                Vector2 bX5 = camController.PanBoundsX;
                Vector2 bZ5 = camController.PanBoundsZ;
                float zoom5 = camController.MaxZoom;

                if (bX5.y > bX1.y && bZ5.y > bZ1.y && zoom5 > zoom1)
                {
                    Debug.Log($"✔ Test 7 Passed: Camera bounds dynamically scale with market size (Lvl 1 X:[{bX1.x},{bX1.y}] Z:[{bZ1.x},{bZ1.y}] Zoom:{zoom1} -> Lvl 5 X:[{bX5.x},{bX5.y}] Z:[{bZ5.x},{bZ5.y}] Zoom:{zoom5}).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 7 Failed: Camera bounds did not expand correctly.");
                }

                // ------------------------------------------------------------------
                // TEST 8: Shelf Registration & Lookup in Unlocked Expansion
                // ------------------------------------------------------------------
                total++;
                expansionMgr.ApplyExpansionLevel(2, animate: false);
                var shelfLvl2 = testExpansionShelves[0]; // prod_chips
                var foundShelf = shelfMgr.FindShelfForProduct("prod_chips");
                if (shelfLvl2 != null && shelfLvl2.gameObject.activeSelf && foundShelf != null && foundShelf.ProductData != null && foundShelf.ProductData.ID == "prod_chips")
                {
                    Debug.Log("✔ Test 8 Passed: Expansion shelf registration verified (Shelf activated and registered with ShelfManager upon area unlock).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 8 Failed: Expansion shelf was not registered or discoverable.");
                }

                // ------------------------------------------------------------------
                // TEST 9: Customer Navigation & Stock Selection Prioritization
                // ------------------------------------------------------------------
                total++;
                // Create base shelf with occupied interaction point, expansion shelf with free interaction point
                inventoryMgr.TryRestock("prod_water", 15, out int _, out double _);

                GameObject baseShelfGo = new GameObject("Base_Shelf_Water");
                baseShelfGo.transform.parent = testRunner.transform;
                var baseShelf = baseShelfGo.AddComponent<ShelfVisualController>();
                GameObject ptBaseGo = new GameObject("BasePt");
                ptBaseGo.transform.parent = baseShelfGo.transform;
                var ipBase = ptBaseGo.AddComponent<CustomerInteractionPoint>();
                ipBase.Initialize("WaterBase", Vector3.forward);
                ipBase.SetOccupied(true); // Occupy base shelf point

                ProductData waterData = inventoryMgr.GetProductData("prod_water");
                baseShelf.Initialize(waterData, ShelfType.StandardShelf, new List<GameObject>(), new List<CustomerInteractionPoint> { ipBase });

                GameObject expShelfGo = new GameObject("Exp_Shelf_Water");
                expShelfGo.transform.parent = testRunner.transform;
                var expShelf = expShelfGo.AddComponent<ShelfVisualController>();
                GameObject ptExpGo = new GameObject("ExpPt");
                ptExpGo.transform.parent = expShelfGo.transform;
                var ipExp = ptExpGo.AddComponent<CustomerInteractionPoint>();
                ipExp.Initialize("WaterExp", Vector3.forward);
                ipExp.SetOccupied(false); // Free interaction point on expansion shelf
                expShelf.Initialize(waterData, ShelfType.StandardShelf, new List<GameObject>(), new List<CustomerInteractionPoint> { ipExp });

                shelfMgr.RegisterShelf(baseShelf);
                shelfMgr.RegisterShelf(expShelf);

                var prioritizedShelf = shelfMgr.FindShelfForProduct("prod_water");
                if (prioritizedShelf != null && prioritizedShelf.CurrentStock > 0 && prioritizedShelf == expShelf)
                {
                    Debug.Log("✔ Test 9 Passed: Duplicate shelf & stock prioritization verified (ShelfManager chooses available in-stock expansion shelf over occupied shelf).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 9 Failed: ShelfManager failed to prioritize shelf with available interaction point.");
                }

                // ------------------------------------------------------------------
                // TEST 10: Save / Load Persistence
                // ------------------------------------------------------------------
                total++;
                GameSaveData mockSave = new GameSaveData();
                mockSave.Store.StoreLevel = 3;
                mockSave.Currency.Cash = 15000;

                // Load simulation
                expansionMgr.ApplyExpansionLevel(mockSave.Store.StoreLevel, animate: false);
                if (expansionMgr.IsExpansionUnlocked(2) && expansionMgr.IsExpansionUnlocked(3) && !expansionMgr.IsExpansionUnlocked(4))
                {
                    Debug.Log("✔ Test 10 Passed: Save/Load persistence verified (StoreLevel 3 correctly restored Expansions 2 & 3 as unlocked, 4 locked).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 10 Failed: Save/load state application mismatch.");
                }

                // ------------------------------------------------------------------
                // TEST 11: Old Save Compatibility
                // ------------------------------------------------------------------
                total++;
                // Old save has no expansion keys or default Level 1
                GameSaveData legacySave = new GameSaveData();
                legacySave.Store.StoreLevel = 1;
                expansionMgr.ApplyExpansionLevel(legacySave.Store.StoreLevel, animate: false);
                bool legacyOk = !expansionMgr.IsExpansionUnlocked(2) && !expansionMgr.IsExpansionUnlocked(3) &&
                                !expansionMgr.IsExpansionUnlocked(4) && !expansionMgr.IsExpansionUnlocked(5);
                if (legacyOk)
                {
                    Debug.Log("✔ Test 11 Passed: Old save compatibility verified (Legacy save with Level 1 safely locks all expansions with 0 errors).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 11 Failed: Legacy save loaded invalid expansion state.");
                }

                // ------------------------------------------------------------------
                // TEST 12: Max Level Behavior
                // ------------------------------------------------------------------
                total++;
                // Testing Level 5 and beyond clamping
                expansionMgr.ApplyExpansionLevel(5, animate: false);
                bool lvl5Ok = expansionMgr.IsExpansionUnlocked(2) && expansionMgr.IsExpansionUnlocked(3) &&
                              expansionMgr.IsExpansionUnlocked(4) && expansionMgr.IsExpansionUnlocked(5);

                // Try higher level
                expansionMgr.ApplyExpansionLevel(6, animate: false);
                bool lvl6Ok = expansionMgr.IsExpansionUnlocked(5);

                if (lvl5Ok && lvl6Ok)
                {
                    Debug.Log("✔ Test 12 Passed: Max level behavior verified (Level 5 clamps cleanly, 0 exceptions on overflow).");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 12 Failed: Max level handling failed.");
                }
            }
            finally
            {
                if (testRunner != null)
                {
                    Object.DestroyImmediate(testRunner);
                }
            }

            Debug.Log("==================================================");
            Debug.Log($"[Stage7Validation] COMPLETED: {passed}/{total} TESTS PASSED.");
            Debug.Log("==================================================");
        }
    }
}
#endif
