#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class Stage6AssetSetup
    {
        static Stage6AssetSetup()
        {
            EditorApplication.delayCall += EnsureAllAssetsExist;
        }

        [MenuItem("MiniMarket/Setup Stage 6 Assets")]
        public static void EnsureAllAssetsExist()
        {
            EnsureMaterialsExist();
            EnsureProductAssetsAndPrefabsExist();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureMaterialsExist()
        {
            string matDir = "Assets/_Project/Art/Materials";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Water.mat", new Color(0.13f, 0.83f, 0.93f), urpLit, 0.85f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Milk.mat", new Color(0.96f, 0.97f, 1.0f), urpLit, 0.35f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Soda.mat", new Color(0.92f, 0.20f, 0.20f), urpLit, 0.9f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Chips.mat", new Color(0.96f, 0.62f, 0.07f), urpLit, 0.4f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Chocolate.mat", new Color(0.36f, 0.17f, 0.08f), urpLit, 0.5f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Canned.mat", new Color(0.85f, 0.45f, 0.05f), urpLit, 0.7f);

            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Juice.mat", new Color(0.98f, 0.45f, 0.09f), urpLit, 0.6f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Yogurt.mat", new Color(0.66f, 0.33f, 0.97f), urpLit, 0.4f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Cookies.mat", new Color(0.71f, 0.33f, 0.04f), urpLit, 0.3f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Candy.mat", new Color(0.93f, 0.28f, 0.60f), urpLit, 0.5f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Sauce.mat", new Color(0.73f, 0.11f, 0.11f), urpLit, 0.7f);
            CreateOrUpdateMaterial($"{matDir}/Mat_Prod_Rice.mat", new Color(0.95f, 0.90f, 0.75f), urpLit, 0.2f);
        }

        private static Material CreateOrUpdateMaterial(string path, Color color, Shader shader, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static void EnsureProductAssetsAndPrefabsExist()
        {
            string prodAssetDir = "Assets/_Project/ScriptableObjects/Products";
            string prefabDir = "Assets/_Project/Prefabs/Products";
            if (!Directory.Exists(prodAssetDir)) Directory.CreateDirectory(prodAssetDir);
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            // 1. Water
            SetupProduct("Product_Water.asset", "PF_Product_Water.prefab", "prod_water", "Pure Water",
                ProductCategory.Beverages, 1.00, 1.50, 20, 50, 8, ShelfType.StandardShelf,
                PrimitiveType.Cylinder, new Vector3(0.10f, 0.28f, 0.10f), "Mat_Prod_Water.mat");

            // 2. Milk
            SetupProduct("Product_Milk.asset", "PF_Product_Milk.prefab", "prod_milk", "Daily Milk",
                ProductCategory.Dairy, 1.50, 2.20, 15, 40, 8, ShelfType.RefrigeratedShelf,
                PrimitiveType.Cube, new Vector3(0.13f, 0.26f, 0.13f), "Mat_Prod_Milk.mat");

            // 3. Chips
            SetupProduct("Product_Chips.asset", "PF_Product_Chips.prefab", "prod_chips", "Crunch Chips",
                ProductCategory.Snacks, 1.20, 2.00, 20, 50, 8, ShelfType.StandardShelf,
                PrimitiveType.Cube, new Vector3(0.18f, 0.28f, 0.08f), "Mat_Prod_Chips.mat");

            // 4. Chocolate
            SetupProduct("Product_Chocolate.asset", "PF_Product_Chocolate.prefab", "prod_chocolate", "Sweet Chocolate",
                ProductCategory.Sweets, 1.00, 1.80, 15, 40, 8, ShelfType.StandardShelf,
                PrimitiveType.Cube, new Vector3(0.14f, 0.04f, 0.22f), "Mat_Prod_Chocolate.mat");

            // 5. Soda
            SetupProduct("Product_Soda.asset", "PF_Product_Soda.prefab", "prod_soda", "Fresh Soda",
                ProductCategory.Beverages, 1.10, 1.90, 20, 50, 8, ShelfType.RefrigeratedShelf,
                PrimitiveType.Cylinder, new Vector3(0.11f, 0.19f, 0.11f), "Mat_Prod_Soda.mat");

            // 6. Canned Food
            SetupProduct("Product_CannedFood.asset", "PF_Product_CannedFood.prefab", "prod_canned", "Daily Canned Food",
                ProductCategory.CannedFood, 1.80, 3.00, 10, 30, 8, ShelfType.StandardShelf,
                PrimitiveType.Cylinder, new Vector3(0.14f, 0.16f, 0.14f), "Mat_Prod_Canned.mat");

            // 7. Orange Juice
            SetupProduct("Product_OrangeJuice.asset", "PF_Product_OrangeJuice.prefab", "prod_juice", "Orange Juice",
                ProductCategory.Beverages, 1.40, 2.40, 15, 40, 8, ShelfType.RefrigeratedShelf,
                PrimitiveType.Cube, new Vector3(0.12f, 0.28f, 0.12f), "Mat_Prod_Juice.mat");

            // 8. Yogurt
            SetupProduct("Product_Yogurt.asset", "PF_Product_Yogurt.prefab", "prod_yogurt", "Yogurt",
                ProductCategory.Dairy, 1.20, 2.00, 15, 40, 8, ShelfType.RefrigeratedShelf,
                PrimitiveType.Cylinder, new Vector3(0.16f, 0.10f, 0.16f), "Mat_Prod_Yogurt.mat");

            // 9. Cookies
            SetupProduct("Product_Cookies.asset", "PF_Product_Cookies.prefab", "prod_cookies", "Cookies",
                ProductCategory.Snacks, 1.30, 2.20, 15, 45, 8, ShelfType.StandardShelf,
                PrimitiveType.Cube, new Vector3(0.24f, 0.09f, 0.09f), "Mat_Prod_Cookies.mat");

            // 10. Candy
            SetupProduct("Product_Candy.asset", "PF_Product_Candy.prefab", "prod_candy", "Candy",
                ProductCategory.Sweets, 0.80, 1.50, 25, 60, 8, ShelfType.StandardShelf,
                PrimitiveType.Cube, new Vector3(0.12f, 0.15f, 0.06f), "Mat_Prod_Candy.mat");

            // 11. Tomato Sauce
            SetupProduct("Product_TomatoSauce.asset", "PF_Product_TomatoSauce.prefab", "prod_sauce", "Tomato Sauce",
                ProductCategory.CannedFood, 1.60, 2.70, 12, 35, 8, ShelfType.StandardShelf,
                PrimitiveType.Cylinder, new Vector3(0.13f, 0.20f, 0.13f), "Mat_Prod_Sauce.mat");

            // 12. Rice
            SetupProduct("Product_Rice.asset", "PF_Product_Rice.prefab", "prod_rice", "Rice",
                ProductCategory.CannedFood, 2.00, 3.50, 10, 30, 8, ShelfType.StandardShelf,
                PrimitiveType.Cube, new Vector3(0.17f, 0.22f, 0.12f), "Mat_Prod_Rice.mat");
        }

        private static void SetupProduct(
            string assetFileName,
            string prefabFileName,
            string id,
            string displayName,
            ProductCategory category,
            double purchase,
            double sell,
            int startingStock,
            int maxStock,
            int shelfCap,
            ShelfType shelfType,
            PrimitiveType silhouettePrimitive,
            Vector3 visualScale,
            string materialName)
        {
            string assetPath = $"Assets/_Project/ScriptableObjects/Products/{assetFileName}";
            string prefabPath = $"Assets/_Project/Prefabs/Products/{prefabFileName}";
            string matPath = $"Assets/_Project/Art/Materials/{materialName}";

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            // 1. Create or load ProductData ScriptableObject
            ProductData data = AssetDatabase.LoadAssetAtPath<ProductData>(assetPath);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<ProductData>();
                data.Configure(id, displayName, category, purchase, sell, startingStock, maxStock, shelfCap, shelfType);
                AssetDatabase.CreateAsset(data, assetPath);
            }
            else
            {
                data.Configure(id, displayName, category, purchase, sell, startingStock, maxStock, shelfCap, shelfType);
                EditorUtility.SetDirty(data);
            }

            // 2. Create or load Product Prefab
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab == null)
            {
                GameObject go = GameObject.CreatePrimitive(silhouettePrimitive);
                go.name = Path.GetFileNameWithoutExtension(prefabFileName);
                go.transform.localScale = visualScale;

                var col = go.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);

                var rend = go.GetComponent<Renderer>();
                if (rend != null && mat != null)
                {
                    rend.sharedMaterial = mat;
                }

                var vis = go.AddComponent<ProductVisual>();
                vis.Initialize(data);

                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
