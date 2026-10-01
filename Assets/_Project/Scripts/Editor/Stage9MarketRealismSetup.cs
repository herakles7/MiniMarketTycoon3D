#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using MiniMarketTycoon.Customers;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Editor
{
    [InitializeOnLoad]
    public static class Stage9MarketRealismSetup
    {
        static Stage9MarketRealismSetup()
        {
            EditorApplication.delayCall += ExecuteSetupIfNeeded;
        }

        private static void ExecuteSetupIfNeeded()
        {
            string configPath = "Assets/_Project/ScriptableObjects/Configuration/CustomerVisualConfig.asset";
            if (!File.Exists(configPath))
            {
                ExecuteFullSetup();
            }
        }

        [MenuItem("MiniMarket/Setup Stage 9 Market & NPC Realism")]
        public static void ExecuteFullSetup()
        {
            Debug.Log("[Stage9Setup] Starting Stage 9 Realistic 3D Market + Human NPC Visual Upgrade...");

            EnsureAllMaterialsExist();
            EnsureVisualConfigExists();
            UpgradeProductPrefabs();
            BuildRealisticModularCustomerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Stage9Setup] Stage 9 Market & NPC Realism Setup Completed Successfully!");
        }

        public static void EnsureAllMaterialsExist()
        {
            string matDir = "Assets/_Project/Art/Materials";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            // 1. Skin Tones
            CreateMaterial($"{matDir}/Mat_Skin_Fair.mat", new Color(0.96f, 0.84f, 0.76f), urpLit, 0.25f);
            CreateMaterial($"{matDir}/Mat_Skin_Olive.mat", new Color(0.85f, 0.72f, 0.58f), urpLit, 0.25f);
            CreateMaterial($"{matDir}/Mat_Skin_Warm.mat", new Color(0.91f, 0.77f, 0.65f), urpLit, 0.25f);
            CreateMaterial($"{matDir}/Mat_Skin_Tan.mat", new Color(0.76f, 0.58f, 0.44f), urpLit, 0.25f);

            // 2. Hair Colors
            CreateMaterial($"{matDir}/Mat_Hair_Black.mat", new Color(0.08f, 0.08f, 0.09f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_DarkBrown.mat", new Color(0.18f, 0.12f, 0.08f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_Brown.mat", new Color(0.32f, 0.20f, 0.12f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_LightBrown.mat", new Color(0.48f, 0.34f, 0.22f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_Blonde.mat", new Color(0.78f, 0.66f, 0.42f), urpLit, 0.35f);

            // 3. Facial Feature Details
            CreateMaterial($"{matDir}/Mat_Eyes_Pupil.mat", new Color(0.10f, 0.12f, 0.16f), urpLit, 0.90f);
            CreateMaterial($"{matDir}/Mat_Eyes_Sclera.mat", new Color(0.97f, 0.97f, 0.98f), urpLit, 0.85f);
            CreateMaterial($"{matDir}/Mat_Lips.mat", new Color(0.82f, 0.48f, 0.48f), urpLit, 0.40f);
            CreateMaterial($"{matDir}/Mat_Glasses_Frame.mat", new Color(0.15f, 0.15f, 0.18f), urpLit, 0.70f);

            // 4. Tops / Shirts / Blouses
            CreateMaterial($"{matDir}/Mat_Clothes_Blue.mat", new Color(0.18f, 0.38f, 0.68f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Teal.mat", new Color(0.12f, 0.55f, 0.58f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Maroon.mat", new Color(0.58f, 0.15f, 0.22f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_White.mat", new Color(0.92f, 0.93f, 0.95f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Grey.mat", new Color(0.42f, 0.44f, 0.46f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Green.mat", new Color(0.18f, 0.50f, 0.30f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Coral.mat", new Color(0.92f, 0.42f, 0.38f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Yellow.mat", new Color(0.92f, 0.75f, 0.22f), urpLit, 0.20f);

            // 5. Pants / Bottoms
            CreateMaterial($"{matDir}/Mat_Pants_DarkGrey.mat", new Color(0.22f, 0.24f, 0.26f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_Khaki.mat", new Color(0.68f, 0.60f, 0.48f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_Navy.mat", new Color(0.12f, 0.18f, 0.32f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_Black.mat", new Color(0.10f, 0.10f, 0.11f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_LightJeans.mat", new Color(0.35f, 0.52f, 0.72f), urpLit, 0.15f);

            // 6. Shoes
            CreateMaterial($"{matDir}/Mat_Shoes_Dark.mat", new Color(0.14f, 0.14f, 0.15f), urpLit, 0.40f);
            CreateMaterial($"{matDir}/Mat_Shoes_White.mat", new Color(0.92f, 0.92f, 0.94f), urpLit, 0.50f);
            CreateMaterial($"{matDir}/Mat_Shoes_Brown.mat", new Color(0.35f, 0.22f, 0.14f), urpLit, 0.35f);

            // 7. Packaging & Product Accents
            CreateMaterial($"{matDir}/Mat_Product_Cap.mat", new Color(0.10f, 0.45f, 0.85f), urpLit, 0.80f);
            CreateMaterial($"{matDir}/Mat_Can_Lid.mat", new Color(0.85f, 0.86f, 0.88f), urpLit, 0.90f);
            CreateMaterial($"{matDir}/Mat_Foil_Seal.mat", new Color(0.88f, 0.89f, 0.90f), urpLit, 0.92f);
        }

        private static Material CreateMaterial(string path, Color color, Shader shader, float smoothness)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                mat.enableInstancing = true;
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        public static void EnsureVisualConfigExists()
        {
            string dir = "Assets/_Project/ScriptableObjects/Configuration";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string path = $"{dir}/CustomerVisualConfig.asset";
            CustomerVisualConfig config = AssetDatabase.LoadAssetAtPath<CustomerVisualConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CustomerVisualConfig>();
                config.InitializeDefaults();
                AssetDatabase.CreateAsset(config, path);
                Debug.Log($"[Stage9Setup] Created CustomerVisualConfig at {path}");
            }
        }

        public static void UpgradeProductPrefabs()
        {
            string prefabDir = "Assets/_Project/Prefabs/Products";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

            // Material helpers
            Material matCap = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Product_Cap.mat");
            Material matCanLid = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Can_Lid.mat");
            Material matFoil = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Foil_Seal.mat");

            // 1. Water: AquaPure (Clear/Blue ribbed bottle + screw cap)
            BuildProductPrefab($"{prefabDir}/PF_Product_Water.prefab", "Mat_Prod_Water.mat", (root) =>
            {
                // Bottle body
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                body.transform.parent = root.transform;
                body.transform.localPosition = new Vector3(0f, 0.10f, 0f);
                body.transform.localScale = new Vector3(0.09f, 0.10f, 0.09f);
                CleanCollider(body);

                // Bottle neck & cap
                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = "Cap";
                cap.transform.parent = root.transform;
                cap.transform.localPosition = new Vector3(0f, 0.22f, 0f);
                cap.transform.localScale = new Vector3(0.045f, 0.025f, 0.045f);
                CleanCollider(cap);
                if (matCap != null) cap.GetComponent<MeshRenderer>().sharedMaterial = matCap;
            });

            // 2. Milk: FarmFresh (Gable-top milk carton)
            BuildProductPrefab($"{prefabDir}/PF_Product_Milk.prefab", "Mat_Prod_Milk.mat", (root) =>
            {
                // Main box body
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.transform.parent = root.transform;
                body.transform.localPosition = new Vector3(0f, 0.11f, 0f);
                body.transform.localScale = new Vector3(0.12f, 0.20f, 0.12f);
                CleanCollider(body);

                // Gable top fold
                GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                top.name = "GableTop";
                top.transform.parent = root.transform;
                top.transform.localPosition = new Vector3(0f, 0.23f, 0f);
                top.transform.localScale = new Vector3(0.10f, 0.04f, 0.03f);
                CleanCollider(top);
            });

            // 3. Chips: CrunchMax (Puffed snack pillow pouch)
            BuildProductPrefab($"{prefabDir}/PF_Product_Chips.prefab", "Mat_Prod_Chips.mat", (root) =>
            {
                GameObject bag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bag.transform.parent = root.transform;
                bag.transform.localPosition = new Vector3(0f, 0.14f, 0f);
                bag.transform.localScale = new Vector3(0.18f, 0.26f, 0.08f);
                CleanCollider(bag);

                GameObject topCrimp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                topCrimp.name = "TopSeal";
                topCrimp.transform.parent = root.transform;
                topCrimp.transform.localPosition = new Vector3(0f, 0.28f, 0f);
                topCrimp.transform.localScale = new Vector3(0.19f, 0.015f, 0.02f);
                CleanCollider(topCrimp);
            });

            // 4. Chocolate: ChocoDelight (Wrapped bar)
            BuildProductPrefab($"{prefabDir}/PF_Product_Chocolate.prefab", "Mat_Prod_Chocolate.mat", (root) =>
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.transform.parent = root.transform;
                bar.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                bar.transform.localScale = new Vector3(0.15f, 0.035f, 0.24f);
                CleanCollider(bar);
            });

            // 5. Soda: FizzPop (Aluminum beverage can with metallic lid)
            BuildProductPrefab($"{prefabDir}/PF_Product_Soda.prefab", "Mat_Prod_Soda.mat", (root) =>
            {
                GameObject can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                can.transform.parent = root.transform;
                can.transform.localPosition = new Vector3(0f, 0.09f, 0f);
                can.transform.localScale = new Vector3(0.10f, 0.09f, 0.10f);
                CleanCollider(can);

                GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lid.name = "Lid";
                lid.transform.parent = root.transform;
                lid.transform.localPosition = new Vector3(0f, 0.182f, 0f);
                lid.transform.localScale = new Vector3(0.095f, 0.005f, 0.095f);
                CleanCollider(lid);
                if (matCanLid != null) lid.GetComponent<MeshRenderer>().sharedMaterial = matCanLid;
            });

            // 6. Canned Food: ChefChoice (Ribbed metal tin can)
            BuildProductPrefab($"{prefabDir}/PF_Product_CannedFood.prefab", "Mat_Prod_CannedFood.mat", (root) =>
            {
                GameObject can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                can.transform.parent = root.transform;
                can.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                can.transform.localScale = new Vector3(0.13f, 0.08f, 0.13f);
                CleanCollider(can);

                GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lid.name = "TinLid";
                lid.transform.parent = root.transform;
                lid.transform.localPosition = new Vector3(0f, 0.162f, 0f);
                lid.transform.localScale = new Vector3(0.125f, 0.005f, 0.125f);
                CleanCollider(lid);
                if (matCanLid != null) lid.GetComponent<MeshRenderer>().sharedMaterial = matCanLid;
            });

            // 7. Juice: SunJuice (Tetra-pack carton)
            BuildProductPrefab($"{prefabDir}/PF_Product_OrangeJuice.prefab", "Mat_Prod_OrangeJuice.mat", (root) =>
            {
                GameObject carton = GameObject.CreatePrimitive(PrimitiveType.Cube);
                carton.transform.parent = root.transform;
                carton.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                carton.transform.localScale = new Vector3(0.11f, 0.24f, 0.11f);
                CleanCollider(carton);

                GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cap.name = "Cap";
                cap.transform.parent = root.transform;
                cap.transform.localPosition = new Vector3(0.02f, 0.245f, 0.02f);
                cap.transform.localScale = new Vector3(0.035f, 0.015f, 0.035f);
                CleanCollider(cap);
                if (matCap != null) cap.GetComponent<MeshRenderer>().sharedMaterial = matCap;
            });

            // 8. Yogurt: CreamyGo (Tapered pot with foil seal lid)
            BuildProductPrefab($"{prefabDir}/PF_Product_Yogurt.prefab", "Mat_Prod_Yogurt.mat", (root) =>
            {
                GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pot.transform.parent = root.transform;
                pot.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                pot.transform.localScale = new Vector3(0.14f, 0.05f, 0.14f);
                CleanCollider(pot);

                GameObject foil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                foil.name = "FoilLid";
                foil.transform.parent = root.transform;
                foil.transform.localPosition = new Vector3(0f, 0.102f, 0f);
                foil.transform.localScale = new Vector3(0.15f, 0.003f, 0.15f);
                CleanCollider(foil);
                if (matFoil != null) foil.GetComponent<MeshRenderer>().sharedMaterial = matFoil;
            });

            // 9. Cookies: SweetBite (Cylindrical roll pack)
            BuildProductPrefab($"{prefabDir}/PF_Product_Cookies.prefab", "Mat_Prod_Cookies.mat", (root) =>
            {
                GameObject roll = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                roll.transform.parent = root.transform;
                roll.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                roll.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                roll.transform.localScale = new Vector3(0.09f, 0.12f, 0.09f);
                CleanCollider(roll);
            });

            // 10. Candy: FruitChew (Confectionery bag)
            BuildProductPrefab($"{prefabDir}/PF_Product_Candy.prefab", "Mat_Prod_Candy.mat", (root) =>
            {
                GameObject pouch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pouch.transform.parent = root.transform;
                pouch.transform.localPosition = new Vector3(0f, 0.09f, 0f);
                pouch.transform.localScale = new Vector3(0.13f, 0.16f, 0.06f);
                CleanCollider(pouch);
            });

            // 11. Sauce: GrandmaSauce (Glass jar with lid)
            BuildProductPrefab($"{prefabDir}/PF_Product_TomatoSauce.prefab", "Mat_Prod_TomatoSauce.mat", (root) =>
            {
                GameObject jar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                jar.transform.parent = root.transform;
                jar.transform.localPosition = new Vector3(0f, 0.09f, 0f);
                jar.transform.localScale = new Vector3(0.12f, 0.09f, 0.12f);
                CleanCollider(jar);

                GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lid.name = "JarLid";
                lid.transform.parent = root.transform;
                lid.transform.localPosition = new Vector3(0f, 0.185f, 0f);
                lid.transform.localScale = new Vector3(0.115f, 0.015f, 0.115f);
                CleanCollider(lid);
                if (matCanLid != null) lid.GetComponent<MeshRenderer>().sharedMaterial = matCanLid;
            });

            // 12. Rice: GoldenGrain (Grain sack)
            BuildProductPrefab($"{prefabDir}/PF_Product_Rice.prefab", "Mat_Prod_Rice.mat", (root) =>
            {
                GameObject sack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sack.transform.parent = root.transform;
                sack.transform.localPosition = new Vector3(0f, 0.11f, 0f);
                sack.transform.localScale = new Vector3(0.16f, 0.22f, 0.10f);
                CleanCollider(sack);
            });
        }

        private static void BuildProductPrefab(string prefabPath, string matName, System.Action<GameObject> buildMeshAction)
        {
            GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            Material mainMat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/_Project/Art/Materials/{matName}");

            buildMeshAction?.Invoke(root);

            // Apply main material to all renderers that don't have a specialized material
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].sharedMaterial == null && mainMat != null)
                {
                    renderers[i].sharedMaterial = mainMat;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static void CleanCollider(GameObject go)
        {
            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }

        public static void BuildRealisticModularCustomerPrefab()
        {
            Material charMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Char_HumanMale.mat");
            // Load the animator controller if it exists, otherwise pass null (Stage11 will create/assign it)
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Project/Art/Characters/AC_Customer.controller");
            Stage11VisualPolishSetup.BuildKenneyCustomerPrefab(charMat, controller);
        }

        private static void OldBuildRealisticModularCustomerPrefab_Deprecated()
        {
            // Deprecated: Replaced by Kenney low-poly character model integration in Stage11VisualPolishSetup
        }
    }
}
#endif
