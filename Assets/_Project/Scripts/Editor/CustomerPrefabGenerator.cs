#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using MiniMarketTycoon.Customers;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class CustomerPrefabGenerator
    {
        static CustomerPrefabGenerator()
        {
            EditorApplication.delayCall += CheckAndGeneratePrefab;
        }

        private static void CheckAndGeneratePrefab()
        {
            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";
            if (!System.IO.File.Exists(prefabPath))
            {
                GeneratePrefab();
            }
        }

        [MenuItem("MiniMarket/Generate Customer Prefab")]
        public static void GeneratePrefab()
        {
            // Build root GameObject
            GameObject root = new GameObject("PF_Customer_NPC");

            // 1. Physics & Navigation
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.3f;
            agent.height = 1.75f;
            agent.speed = 1.25f;
            agent.angularSpeed = 480f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.2f;

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.9f, 0f);
            col.radius = 0.3f;
            col.height = 1.8f;

            var nav = root.AddComponent<CustomerNavigation>();
            var anim = root.AddComponent<CustomerAnimationController>();
            var visual = root.AddComponent<CustomerVisual>();
            var controller = root.AddComponent<CustomerController>();

            // Load materials
            Material skinMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Skin_Warm.mat");
            Material hairMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Hair_Brown.mat");
            Material clothesMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Clothes_Blue.mat");
            Material pantsMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Pants_Khaki.mat");
            Material shoeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Shoes_Dark.mat");
            Material basketMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Basket_Red.mat");

            visual.BuildProceduralHumanoidRig(skinMat, hairMat, clothesMat, pantsMat, shoeMat, basketMat);
            anim.BindProceduralBones(visual.TorsoTransform, visual.HeadTransform, visual.LeftArmTransform, visual.RightArmTransform, visual.LeftLegTransform, visual.RightLegTransform);

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Customers"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Customers");
            }

            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[CustomerPrefabGenerator] Successfully saved customer prefab to {prefabPath}");
        }
    }
}
#endif
