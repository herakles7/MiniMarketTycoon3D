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
            string prefabPath = "Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab";

            GameObject root = new GameObject("PF_Customer_NPC");

            // 1. Navigation & Physics
            var agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.30f;
            agent.height = 1.75f;
            agent.speed = 1.25f;
            agent.angularSpeed = 480f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.20f;

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.88f, 0f);
            col.radius = 0.30f;
            col.height = 1.75f;

            var nav = root.AddComponent<CustomerNavigation>();
            var anim = root.AddComponent<CustomerAnimationController>();
            var visualController = root.AddComponent<CustomerVisualController>();
            var visual = root.AddComponent<CustomerVisual>();
            var controller = root.AddComponent<CustomerController>();

            // 2. Load Base Materials
            Material skinMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Skin_Warm.mat");
            Material hairMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Hair_Brown.mat");
            Material eyeWhiteMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Eyes_Sclera.mat");
            Material eyePupilMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Eyes_Pupil.mat");
            Material lipsMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Lips.mat");
            Material clothesMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Clothes_Blue.mat");
            Material pantsMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Pants_Khaki.mat");
            Material shoeMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Shoes_Dark.mat");
            Material basketMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Basket_Red.mat");
            Material glassesMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Glasses_Frame.mat");

            // 3. Assemble Hierarchies for LOD0, LOD1, LOD2
            GameObject modelRoot = new GameObject("Humanoid_Model");
            modelRoot.transform.parent = root.transform;
            modelRoot.transform.localPosition = Vector3.zero;

            // LOD0 Root (Full anatomical detail with facial features, hair geometry, accessories)
            GameObject lod0Root = new GameObject("LOD0");
            lod0Root.transform.parent = modelRoot.transform;
            lod0Root.transform.localPosition = Vector3.zero;

            // Pelvis / Hips
            GameObject hips = new GameObject("Hips");
            hips.transform.parent = lod0Root.transform;
            hips.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            // Torso (Upper body: T-shirt / Polo / Shirt)
            GameObject torsoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torsoObj.name = "Torso";
            torsoObj.transform.parent = hips.transform;
            torsoObj.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            torsoObj.transform.localScale = new Vector3(0.42f, 0.54f, 0.24f);
            CleanCollider(torsoObj);
            var torsoRend = torsoObj.GetComponent<MeshRenderer>();
            torsoRend.sharedMaterial = clothesMat;

            // Neck
            GameObject neckObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neckObj.name = "Neck";
            neckObj.transform.parent = torsoObj.transform;
            neckObj.transform.localPosition = new Vector3(0f, 0.52f, 0.02f);
            neckObj.transform.localScale = new Vector3(0.24f, 0.12f, 0.24f);
            CleanCollider(neckObj);
            neckObj.GetComponent<MeshRenderer>().sharedMaterial = skinMat;

            // Head (Anatomical proportions: skull, chin, jawline)
            GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            headObj.name = "Head";
            headObj.transform.parent = torsoObj.transform;
            headObj.transform.localPosition = new Vector3(0f, 0.65f, 0.02f);
            headObj.transform.localScale = new Vector3(0.50f, 0.52f, 0.54f);
            CleanCollider(headObj);
            var headRend = headObj.GetComponent<MeshRenderer>();
            headRend.sharedMaterial = skinMat;

            // 3D Facial Features
            // Left Eye
            GameObject lEyeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lEyeObj.name = "Left_Eye";
            lEyeObj.transform.parent = headObj.transform;
            lEyeObj.transform.localPosition = new Vector3(-0.16f, 0.08f, 0.42f);
            lEyeObj.transform.localScale = new Vector3(0.12f, 0.10f, 0.08f);
            CleanCollider(lEyeObj);
            var lEyeRend = lEyeObj.GetComponent<MeshRenderer>();
            lEyeRend.sharedMaterial = eyePupilMat;

            // Right Eye
            GameObject rEyeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rEyeObj.name = "Right_Eye";
            rEyeObj.transform.parent = headObj.transform;
            rEyeObj.transform.localPosition = new Vector3(0.16f, 0.08f, 0.42f);
            rEyeObj.transform.localScale = new Vector3(0.12f, 0.10f, 0.08f);
            CleanCollider(rEyeObj);
            var rEyeRend = rEyeObj.GetComponent<MeshRenderer>();
            rEyeRend.sharedMaterial = eyePupilMat;

            // Left Eyebrow
            GameObject lBrowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lBrowObj.name = "Left_Eyebrow";
            lBrowObj.transform.parent = headObj.transform;
            lBrowObj.transform.localPosition = new Vector3(-0.16f, 0.18f, 0.42f);
            lBrowObj.transform.localScale = new Vector3(0.18f, 0.04f, 0.05f);
            CleanCollider(lBrowObj);
            var lBrowRend = lBrowObj.GetComponent<MeshRenderer>();
            lBrowRend.sharedMaterial = hairMat;

            // Right Eyebrow
            GameObject rBrowObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rBrowObj.name = "Right_Eyebrow";
            rBrowObj.transform.parent = headObj.transform;
            rBrowObj.transform.localPosition = new Vector3(0.16f, 0.18f, 0.42f);
            rBrowObj.transform.localScale = new Vector3(0.18f, 0.04f, 0.05f);
            CleanCollider(rBrowObj);
            var rBrowRend = rBrowObj.GetComponent<MeshRenderer>();
            rBrowRend.sharedMaterial = hairMat;

            // Nose
            GameObject noseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            noseObj.name = "Nose";
            noseObj.transform.parent = headObj.transform;
            noseObj.transform.localPosition = new Vector3(0f, -0.02f, 0.48f);
            noseObj.transform.localScale = new Vector3(0.08f, 0.14f, 0.12f);
            CleanCollider(noseObj);
            var noseRend = noseObj.GetComponent<MeshRenderer>();
            noseRend.sharedMaterial = skinMat;

            // Mouth
            GameObject mouthObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouthObj.name = "Mouth";
            mouthObj.transform.parent = headObj.transform;
            mouthObj.transform.localPosition = new Vector3(0f, -0.18f, 0.44f);
            mouthObj.transform.localScale = new Vector3(0.16f, 0.05f, 0.06f);
            CleanCollider(mouthObj);
            var mouthRend = mouthObj.GetComponent<MeshRenderer>();
            mouthRend.sharedMaterial = lipsMat;

            // Left Ear
            GameObject lEarObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lEarObj.name = "Left_Ear";
            lEarObj.transform.parent = headObj.transform;
            lEarObj.transform.localPosition = new Vector3(-0.48f, 0.02f, 0f);
            lEarObj.transform.localScale = new Vector3(0.08f, 0.18f, 0.12f);
            CleanCollider(lEarObj);
            var lEarRend = lEarObj.GetComponent<MeshRenderer>();
            lEarRend.sharedMaterial = skinMat;

            // Right Ear
            GameObject rEarObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rEarObj.name = "Right_Ear";
            rEarObj.transform.parent = headObj.transform;
            rEarObj.transform.localPosition = new Vector3(0.48f, 0.02f, 0f);
            rEarObj.transform.localScale = new Vector3(0.08f, 0.18f, 0.12f);
            CleanCollider(rEarObj);
            var rEarRend = rEarObj.GetComponent<MeshRenderer>();
            rEarRend.sharedMaterial = skinMat;

            // Modular Hairstyles
            GameObject hairRoot = new GameObject("Hair_Modular_Root");
            hairRoot.transform.parent = headObj.transform;
            hairRoot.transform.localPosition = Vector3.zero;

            List<MeshRenderer> allHairRenderers = new List<MeshRenderer>();

            // 1. Hair Short
            GameObject hShort = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hShort.name = "Hair_Short";
            hShort.transform.parent = hairRoot.transform;
            hShort.transform.localPosition = new Vector3(0f, 0.22f, -0.04f);
            hShort.transform.localScale = new Vector3(1.04f, 0.65f, 1.05f);
            CleanCollider(hShort);
            var hShortRend = hShort.GetComponent<MeshRenderer>();
            hShortRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hShortRend);

            // 2. Hair Fade
            GameObject hFade = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hFade.name = "Hair_Fade";
            hFade.transform.parent = hairRoot.transform;
            hFade.transform.localPosition = new Vector3(0f, 0.26f, -0.02f);
            hFade.transform.localScale = new Vector3(0.96f, 0.70f, 1.02f);
            CleanCollider(hFade);
            var hFadeRend = hFade.GetComponent<MeshRenderer>();
            hFadeRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hFadeRend);
            hFade.SetActive(false);

            // 3. Hair Medium
            GameObject hMed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hMed.name = "Hair_Medium";
            hMed.transform.parent = hairRoot.transform;
            hMed.transform.localPosition = new Vector3(0f, 0.20f, -0.06f);
            hMed.transform.localScale = new Vector3(1.10f, 0.75f, 1.12f);
            CleanCollider(hMed);
            var hMedRend = hMed.GetComponent<MeshRenderer>();
            hMedRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hMedRend);
            hMed.SetActive(false);

            // 4. Hair Long
            GameObject hLong = new GameObject("Hair_Long");
            hLong.transform.parent = hairRoot.transform;
            hLong.transform.localPosition = Vector3.zero;

            GameObject hLongCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hLongCap.transform.parent = hLong.transform;
            hLongCap.transform.localPosition = new Vector3(0f, 0.20f, -0.05f);
            hLongCap.transform.localScale = new Vector3(1.08f, 0.70f, 1.10f);
            CleanCollider(hLongCap);
            var hLongCapRend = hLongCap.GetComponent<MeshRenderer>();
            hLongCapRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hLongCapRend);

            GameObject hLongStrandL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hLongStrandL.transform.parent = hLong.transform;
            hLongStrandL.transform.localPosition = new Vector3(-0.46f, -0.15f, -0.05f);
            hLongStrandL.transform.localScale = new Vector3(0.18f, 0.65f, 0.40f);
            CleanCollider(hLongStrandL);
            var hStrandLRend = hLongStrandL.GetComponent<MeshRenderer>();
            hStrandLRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hStrandLRend);

            GameObject hLongStrandR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hLongStrandR.transform.parent = hLong.transform;
            hLongStrandR.transform.localPosition = new Vector3(0.46f, -0.15f, -0.05f);
            hLongStrandR.transform.localScale = new Vector3(0.18f, 0.65f, 0.40f);
            CleanCollider(hLongStrandR);
            var hStrandRRend = hLongStrandR.GetComponent<MeshRenderer>();
            hStrandRRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hStrandRRend);
            hLong.SetActive(false);

            // 5. Hair Ponytail
            GameObject hPony = new GameObject("Hair_Ponytail");
            hPony.transform.parent = hairRoot.transform;
            hPony.transform.localPosition = Vector3.zero;

            GameObject hPonyCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hPonyCap.transform.parent = hPony.transform;
            hPonyCap.transform.localPosition = new Vector3(0f, 0.22f, -0.04f);
            hPonyCap.transform.localScale = new Vector3(1.04f, 0.65f, 1.05f);
            CleanCollider(hPonyCap);
            var hPonyCapRend = hPonyCap.GetComponent<MeshRenderer>();
            hPonyCapRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hPonyCapRend);

            GameObject hTail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hTail.transform.parent = hPony.transform;
            hTail.transform.localPosition = new Vector3(0f, 0.05f, -0.55f);
            hTail.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);
            hTail.transform.localScale = new Vector3(0.15f, 0.28f, 0.15f);
            CleanCollider(hTail);
            var hTailRend = hTail.GetComponent<MeshRenderer>();
            hTailRend.sharedMaterial = hairMat;
            allHairRenderers.Add(hTailRend);
            hPony.SetActive(false);

            // Accessories: Glasses
            GameObject glasses = new GameObject("Glasses");
            glasses.transform.parent = headObj.transform;
            glasses.transform.localPosition = new Vector3(0f, 0.08f, 0.44f);

            GameObject glBridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glBridge.transform.parent = glasses.transform;
            glBridge.transform.localPosition = Vector3.zero;
            glBridge.transform.localScale = new Vector3(0.52f, 0.03f, 0.04f);
            CleanCollider(glBridge);
            glBridge.GetComponent<MeshRenderer>().sharedMaterial = glassesMat;

            GameObject glL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glL.transform.parent = glasses.transform;
            glL.transform.localPosition = new Vector3(-0.16f, 0f, 0.02f);
            glL.transform.localScale = new Vector3(0.16f, 0.12f, 0.03f);
            CleanCollider(glL);
            glL.GetComponent<MeshRenderer>().sharedMaterial = glassesMat;

            GameObject glR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glR.transform.parent = glasses.transform;
            glR.transform.localPosition = new Vector3(0.16f, 0f, 0.02f);
            glR.transform.localScale = new Vector3(0.16f, 0.12f, 0.03f);
            CleanCollider(glR);
            glR.GetComponent<MeshRenderer>().sharedMaterial = glassesMat;
            glasses.SetActive(false);

            // Arms & Hands
            // Left Arm
            GameObject lArmPivot = new GameObject("Left_Arm_Pivot");
            lArmPivot.transform.parent = torsoObj.transform;
            lArmPivot.transform.localPosition = new Vector3(-0.58f, 0.20f, 0f);

            GameObject lArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lArmMesh.name = "Left_Arm_Mesh";
            lArmMesh.transform.parent = lArmPivot.transform;
            lArmMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            lArmMesh.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            CleanCollider(lArmMesh);
            var lArmRend = lArmMesh.GetComponent<MeshRenderer>();
            lArmRend.sharedMaterial = clothesMat;

            GameObject lHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lHand.name = "Left_Hand";
            lHand.transform.parent = lArmPivot.transform;
            lHand.transform.localPosition = new Vector3(0f, -0.52f, 0f);
            lHand.transform.localScale = new Vector3(0.16f, 0.16f, 0.14f);
            CleanCollider(lHand);
            var lHandRend = lHand.GetComponent<MeshRenderer>();
            lHandRend.sharedMaterial = skinMat;

            // Right Arm
            GameObject rArmPivot = new GameObject("Right_Arm_Pivot");
            rArmPivot.transform.parent = torsoObj.transform;
            rArmPivot.transform.localPosition = new Vector3(0.58f, 0.20f, 0f);

            GameObject rArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rArmMesh.name = "Right_Arm_Mesh";
            rArmMesh.transform.parent = rArmPivot.transform;
            rArmMesh.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            rArmMesh.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
            CleanCollider(rArmMesh);
            var rArmRend = rArmMesh.GetComponent<MeshRenderer>();
            rArmRend.sharedMaterial = clothesMat;

            GameObject rHand = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rHand.name = "Right_Hand";
            rHand.transform.parent = rArmPivot.transform;
            rHand.transform.localPosition = new Vector3(0f, -0.52f, 0f);
            rHand.transform.localScale = new Vector3(0.16f, 0.16f, 0.14f);
            CleanCollider(rHand);
            var rHandRend = rHand.GetComponent<MeshRenderer>();
            rHandRend.sharedMaterial = skinMat;

            // Shopping Basket (held in right hand)
            GameObject basket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            basket.name = "ShoppingBasket";
            basket.transform.parent = rArmPivot.transform;
            basket.transform.localPosition = new Vector3(0f, -0.52f, 0.18f);
            basket.transform.localScale = new Vector3(0.35f, 0.24f, 0.44f);
            CleanCollider(basket);
            var basketRend = basket.GetComponent<MeshRenderer>();
            basketRend.sharedMaterial = basketMat;
            basket.SetActive(false);

            // Legs & Shoes
            // Left Leg
            GameObject lLegPivot = new GameObject("Left_Leg_Pivot");
            lLegPivot.transform.parent = hips.transform;
            lLegPivot.transform.localPosition = new Vector3(-0.16f, 0f, 0f);

            GameObject lLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lLegMesh.name = "Left_Leg_Mesh";
            lLegMesh.transform.parent = lLegPivot.transform;
            lLegMesh.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            lLegMesh.transform.localScale = new Vector3(0.25f, 0.42f, 0.25f);
            CleanCollider(lLegMesh);
            var lLegRend = lLegMesh.GetComponent<MeshRenderer>();
            lLegRend.sharedMaterial = pantsMat;

            GameObject lShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lShoe.name = "Left_Shoe";
            lShoe.transform.parent = lLegPivot.transform;
            lShoe.transform.localPosition = new Vector3(0f, -0.82f, 0.05f);
            lShoe.transform.localScale = new Vector3(0.24f, 0.12f, 0.35f);
            CleanCollider(lShoe);
            var lShoeRend = lShoe.GetComponent<MeshRenderer>();
            lShoeRend.sharedMaterial = shoeMat;

            // Right Leg
            GameObject rLegPivot = new GameObject("Right_Leg_Pivot");
            rLegPivot.transform.parent = hips.transform;
            rLegPivot.transform.localPosition = new Vector3(0.16f, 0f, 0f);

            GameObject rLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rLegMesh.name = "Right_Leg_Mesh";
            rLegMesh.transform.parent = rLegPivot.transform;
            rLegMesh.transform.localPosition = new Vector3(0f, -0.42f, 0f);
            rLegMesh.transform.localScale = new Vector3(0.25f, 0.42f, 0.25f);
            CleanCollider(rLegMesh);
            var rLegRend = rLegMesh.GetComponent<MeshRenderer>();
            rLegRend.sharedMaterial = pantsMat;

            GameObject rShoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rShoe.name = "Right_Shoe";
            rShoe.transform.parent = rLegPivot.transform;
            rShoe.transform.localPosition = new Vector3(0f, -0.82f, 0.05f);
            rShoe.transform.localScale = new Vector3(0.24f, 0.12f, 0.35f);
            CleanCollider(rShoe);
            var rShoeRend = rShoe.GetComponent<MeshRenderer>();
            rShoeRend.sharedMaterial = shoeMat;

            // 4. LOD1 Mesh (Medium detail: Head, hair cap, torso, limbs, shoes - facial features culled)
            GameObject lod1Root = new GameObject("LOD1");
            lod1Root.transform.parent = modelRoot.transform;
            lod1Root.transform.localPosition = Vector3.zero;

            GameObject lod1Body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lod1Body.name = "LOD1_Silhouette_Body";
            lod1Body.transform.parent = lod1Root.transform;
            lod1Body.transform.localPosition = new Vector3(0f, 0.90f, 0f);
            lod1Body.transform.localScale = new Vector3(0.48f, 0.88f, 0.32f);
            CleanCollider(lod1Body);
            var lod1Rend = lod1Body.GetComponent<MeshRenderer>();
            lod1Rend.sharedMaterial = clothesMat;
            lod1Root.SetActive(false);

            // 5. LOD2 Mesh (Low silhouette mesh for mobile draw-call optimization)
            GameObject lod2Root = new GameObject("LOD2");
            lod2Root.transform.parent = modelRoot.transform;
            lod2Root.transform.localPosition = Vector3.zero;

            GameObject lod2Body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lod2Body.name = "LOD2_LowPoly_Proxy";
            lod2Body.transform.parent = lod2Root.transform;
            lod2Body.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            lod2Body.transform.localScale = new Vector3(0.46f, 0.86f, 0.30f);
            CleanCollider(lod2Body);
            var lod2Rend = lod2Body.GetComponent<MeshRenderer>();
            lod2Rend.sharedMaterial = clothesMat;
            lod2Root.SetActive(false);

            // 6. LODGroup Setup
            LODGroup lodGroup = root.AddComponent<LODGroup>();
            LOD[] lods = new LOD[3];

            Renderer[] lod0Renderers = lod0Root.GetComponentsInChildren<Renderer>(true);
            Renderer[] lod1Renderers = lod1Root.GetComponentsInChildren<Renderer>(true);
            Renderer[] lod2Renderers = lod2Root.GetComponentsInChildren<Renderer>(true);

            // Transition thresholds: LOD0 (0.45), LOD1 (0.18), LOD2 (0.05), Culling (<0.05)
            lods[0] = new LOD(0.45f, lod0Renderers);
            lods[1] = new LOD(0.18f, lod1Renderers);
            lods[2] = new LOD(0.05f, lod2Renderers);

            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds();

            // 7. Wire CustomerVisualController
            visualController.AssignReferences(
                modelRoot, lod0Root, lod1Root, lod2Root, lodGroup,
                headRend, lEyeRend, rEyeRend, lBrowRend, rBrowRend,
                noseRend, mouthRend, lEarRend, rEarRend,
                hShort, hFade, hMed, hLong, hPony,
                allHairRenderers,
                torsoRend, lArmRend, rArmRend, lHandRend, rHandRend,
                lLegRend, rLegRend, lShoeRend, rShoeRend,
                basket, glasses,
                torsoObj.transform, headObj.transform, lArmPivot.transform, rArmPivot.transform, lLegPivot.transform, rLegPivot.transform
            );

            // 8. Bind Procedural Animation Controller Bones
            anim.BindProceduralBones(torsoObj.transform, headObj.transform, lArmPivot.transform, rArmPivot.transform, lLegPivot.transform, rLegPivot.transform);

            // Ensure destination folder
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Customers"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Customers");
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[Stage9Setup] Saved realistic modular humanoid customer prefab to {prefabPath}");
        }
    }
}
#endif
