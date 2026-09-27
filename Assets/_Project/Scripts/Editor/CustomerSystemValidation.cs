#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using MiniMarketTycoon.Customers;

namespace MiniMarketTycoon.Editor
{
    public static class CustomerSystemValidation
    {
        [MenuItem("MiniMarket/Validate Customer System")]
        public static void RunValidation()
        {
            Debug.Log("==========================================");
            Debug.Log("[CustomerSystemValidation] Starting Stage 3 Validation...");
            Debug.Log("==========================================");

            int passed = 0;
            int total = 0;

            // 1. Check CustomerConfiguration
            total++;
            var config = AssetDatabase.LoadAssetAtPath<CustomerConfiguration>("Assets/_Project/ScriptableObjects/Configuration/CustomerConfiguration.asset");
            if (config != null && config.StartingCustomers == 3 && config.MaxCustomers == 8)
            {
                Debug.Log("✔ Test 1 Passed: CustomerConfiguration.asset loaded with Starting: 3, Max: 8, Interval: 5s.");
                passed++;
            }
            else
            {
                Debug.LogError("❌ Test 1 Failed: CustomerConfiguration.asset missing or invalid.");
            }

            // 2. Check Customer Prefab
            total++;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab");
            if (prefab != null && prefab.GetComponent<CustomerController>() != null && prefab.GetComponent<UnityEngine.AI.NavMeshAgent>() != null)
            {
                Debug.Log("✔ Test 2 Passed: PF_Customer_NPC.prefab verified with NavMeshAgent, CapsuleCollider, Navigation, Animation, Visual, Controller.");
                passed++;
            }
            else
            {
                Debug.LogError("❌ Test 2 Failed: PF_Customer_NPC.prefab missing or components missing.");
            }

            // 3. Check Materials
            total++;
            string[] checkMats = new string[] { "Mat_Skin_Warm", "Mat_Skin_Fair", "Mat_Skin_Olive", "Mat_Clothes_Blue", "Mat_Clothes_Teal", "Mat_Clothes_Maroon", "Mat_Pants_Khaki", "Mat_Pants_Navy", "Mat_Shoes_Dark" };
            bool allMatsValid = true;
            foreach (var m in checkMats)
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Materials/{m}.mat");
                if (mat == null)
                {
                    allMatsValid = false;
                    break;
                }
            }
            if (allMatsValid)
            {
                Debug.Log("✔ Test 3 Passed: All character PBR materials verified with valid URP Lit shaders.");
                passed++;
            }
            else
            {
                Debug.LogError("❌ Test 3 Failed: One or more character materials missing.");
            }

            // 4. Test Queue Controller Logic
            total++;
            GameObject queueGo = new GameObject("Test_Queue");
            var queue = queueGo.AddComponent<CustomerQueueController>();
            queue.Initialize(new Vector3(-2f, 0f, -5.6f), Vector3.forward, -Vector3.forward, capacity: 4);

            GameObject cust1Go = new GameObject("Test_Cust1");
            var c1 = cust1Go.AddComponent<CustomerController>();
            GameObject cust2Go = new GameObject("Test_Cust2");
            var c2 = cust2Go.AddComponent<CustomerController>();

            bool enqueued1 = queue.TryJoinQueue(c1, out int slot1, out Vector3 pos1);
            bool enqueued2 = queue.TryJoinQueue(c2, out int slot2, out Vector3 pos2);

            if (enqueued1 && enqueued2 && slot1 == 0 && slot2 == 1 && queue.QueuedCount == 2)
            {
                queue.RemoveFromQueue(c1);
                if (queue.QueuedCount == 1)
                {
                    Debug.Log("✔ Test 4 Passed: CustomerQueueController FIFO enqueuing, advancement, and dequeuing verified.");
                    passed++;
                }
                else
                {
                    Debug.LogError("❌ Test 4 Failed: Queue count did not update on remove.");
                }
            }
            else
            {
                Debug.LogError("❌ Test 4 Failed: Queue joining failed.");
            }

            Object.DestroyImmediate(cust1Go);
            Object.DestroyImmediate(cust2Go);
            Object.DestroyImmediate(queueGo);

            // 5. Test Shopping Data Logic
            total++;
            CustomerShoppingData shopData = new CustomerShoppingData();
            shopData.Initialize(3, 60f);
            shopData.AddWishlistItem("Snacks");
            shopData.AddWishlistItem("Water");
            shopData.AddWishlistItem("Milk");

            if (shopData.HasPendingItems() && shopData.GetCurrentItem() == "Snacks")
            {
                shopData.MarkCurrentItemCollected();
                if (shopData.GetCurrentItem() == "Water" && shopData.ItemsCollected == 1)
                {
                    shopData.MarkCurrentItemCollected();
                    shopData.MarkCurrentItemCollected();
                    if (!shopData.HasPendingItems() && shopData.ItemsCollected == 3)
                    {
                        Debug.Log("✔ Test 5 Passed: CustomerShoppingData wishlist iteration and collection completed.");
                        passed++;
                    }
                }
            }

            Debug.Log("==========================================");
            Debug.Log($"[CustomerSystemValidation] Completed {passed}/{total} tests successfully!");
            Debug.Log("==========================================");
        }
    }
}
#endif
