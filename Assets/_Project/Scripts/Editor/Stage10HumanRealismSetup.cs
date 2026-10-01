#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using MiniMarketTycoon.Customers;

namespace MiniMarketTycoon.Editor
{
    /// <summary>
    /// Master automation for Stage 10: Realistic Human NPC & Animation Polish.
    /// Creates realistic mobile skin, hair, eye iris, and clothing materials,
    /// constructs the anatomical humanoid character model with proper human proportions,
    /// configures 3-stage LODGroup, wires modular hairstyles, accessories, and controllers.
    /// </summary>
    [InitializeOnLoad]
    public static class Stage10HumanRealismSetup
    {
        static Stage10HumanRealismSetup()
        {
            EditorApplication.delayCall += CheckAndSetupStage10;
        }

        private static void CheckAndSetupStage10()
        {
            // Auto-check materials and prefab on load
            EnsureAllMaterialsExist();
            EnsureVisualConfigExists();
        }

        [MenuItem("MiniMarket/Stage 10: Setup Realistic Human NPCs")]
        public static void ExecuteFullSetup()
        {
            Debug.Log("[Stage10Setup] Executing Realistic Human NPC & Animation Polish Setup...");
            EnsureAllMaterialsExist();
            EnsureVisualConfigExists();
            BuildRealisticHumanCustomerPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Stage10Setup] Stage 10 Setup completed successfully!");
        }

        public static void EnsureAllMaterialsExist()
        {
            string matDir = "Assets/_Project/Art/Materials";
            if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            // 1. Natural Skin Tones (6 Natural Shades)
            CreateMaterial($"{matDir}/Mat_Skin_Fair.mat", new Color(0.96f, 0.84f, 0.76f), urpLit, 0.28f);
            CreateMaterial($"{matDir}/Mat_Skin_Peach.mat", new Color(0.98f, 0.86f, 0.80f), urpLit, 0.28f);
            CreateMaterial($"{matDir}/Mat_Skin_Olive.mat", new Color(0.85f, 0.72f, 0.58f), urpLit, 0.28f);
            CreateMaterial($"{matDir}/Mat_Skin_Warm.mat", new Color(0.91f, 0.77f, 0.65f), urpLit, 0.28f);
            CreateMaterial($"{matDir}/Mat_Skin_Tan.mat", new Color(0.76f, 0.58f, 0.44f), urpLit, 0.28f);
            CreateMaterial($"{matDir}/Mat_Skin_Deep.mat", new Color(0.42f, 0.28f, 0.20f), urpLit, 0.28f);

            // 2. Hair Colors
            CreateMaterial($"{matDir}/Mat_Hair_Black.mat", new Color(0.08f, 0.08f, 0.09f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_DarkBrown.mat", new Color(0.18f, 0.12f, 0.08f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_Brown.mat", new Color(0.32f, 0.20f, 0.12f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_LightBrown.mat", new Color(0.48f, 0.34f, 0.22f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_Blonde.mat", new Color(0.78f, 0.66f, 0.42f), urpLit, 0.35f);
            CreateMaterial($"{matDir}/Mat_Hair_Grey.mat", new Color(0.68f, 0.68f, 0.70f), urpLit, 0.35f);

            // 3. Eye Features (Sclera, Natural Irises, Pupil, Specular Glint)
            CreateMaterial($"{matDir}/Mat_Eyes_Sclera.mat", new Color(0.97f, 0.97f, 0.98f), urpLit, 0.85f);
            CreateMaterial($"{matDir}/Mat_Eyes_Iris_Brown.mat", new Color(0.35f, 0.22f, 0.12f), urpLit, 0.90f);
            CreateMaterial($"{matDir}/Mat_Eyes_Iris_Blue.mat", new Color(0.22f, 0.45f, 0.75f), urpLit, 0.90f);
            CreateMaterial($"{matDir}/Mat_Eyes_Iris_Green.mat", new Color(0.25f, 0.50f, 0.32f), urpLit, 0.90f);
            CreateMaterial($"{matDir}/Mat_Eyes_Pupil.mat", new Color(0.06f, 0.06f, 0.07f), urpLit, 0.95f);
            CreateMaterial($"{matDir}/Mat_Eyes_Spec.mat", new Color(1.0f, 1.0f, 1.0f), urpLit, 0.98f);
            CreateMaterial($"{matDir}/Mat_Lips.mat", new Color(0.82f, 0.48f, 0.48f), urpLit, 0.40f);

            // 4. Wardrobe Tops
            CreateMaterial($"{matDir}/Mat_Clothes_Blue.mat", new Color(0.18f, 0.38f, 0.68f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Teal.mat", new Color(0.12f, 0.55f, 0.58f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Maroon.mat", new Color(0.58f, 0.15f, 0.22f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_White.mat", new Color(0.92f, 0.93f, 0.95f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Grey.mat", new Color(0.42f, 0.44f, 0.46f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Green.mat", new Color(0.18f, 0.50f, 0.30f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Coral.mat", new Color(0.92f, 0.42f, 0.38f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Charcoal.mat", new Color(0.22f, 0.23f, 0.25f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Olive.mat", new Color(0.34f, 0.42f, 0.28f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Beige.mat", new Color(0.82f, 0.76f, 0.68f), urpLit, 0.20f);
            CreateMaterial($"{matDir}/Mat_Clothes_Navy.mat", new Color(0.12f, 0.18f, 0.35f), urpLit, 0.20f);

            // 5. Wardrobe Bottoms
            CreateMaterial($"{matDir}/Mat_Pants_DarkGrey.mat", new Color(0.22f, 0.24f, 0.26f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_Khaki.mat", new Color(0.68f, 0.60f, 0.48f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_Navy.mat", new Color(0.12f, 0.18f, 0.32f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_Black.mat", new Color(0.10f, 0.10f, 0.11f), urpLit, 0.15f);
            CreateMaterial($"{matDir}/Mat_Pants_LightJeans.mat", new Color(0.35f, 0.52f, 0.72f), urpLit, 0.15f);

            // 6. Shoes
            CreateMaterial($"{matDir}/Mat_Shoes_Dark.mat", new Color(0.14f, 0.14f, 0.15f), urpLit, 0.40f);
            CreateMaterial($"{matDir}/Mat_Shoes_White.mat", new Color(0.92f, 0.92f, 0.94f), urpLit, 0.50f);
            CreateMaterial($"{matDir}/Mat_Shoes_Brown.mat", new Color(0.35f, 0.22f, 0.14f), urpLit, 0.35f);

            // 7. Accessories
            CreateMaterial($"{matDir}/Mat_Glasses_Frame.mat", new Color(0.15f, 0.15f, 0.18f), urpLit, 0.70f);
            CreateMaterial($"{matDir}/Mat_Basket_Red.mat", new Color(0.85f, 0.15f, 0.18f), urpLit, 0.40f);
            CreateMaterial($"{matDir}/Mat_Accessory_Cap.mat", new Color(0.15f, 0.20f, 0.28f), urpLit, 0.25f);
            CreateMaterial($"{matDir}/Mat_Accessory_Bag.mat", new Color(0.78f, 0.70f, 0.58f), urpLit, 0.30f);
            CreateMaterial($"{matDir}/Mat_Accessory_Backpack.mat", new Color(0.24f, 0.28f, 0.36f), urpLit, 0.25f);
            CreateMaterial($"{matDir}/Mat_Accessory_Phone.mat", new Color(0.12f, 0.12f, 0.14f), urpLit, 0.85f);
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
                Debug.Log($"[Stage10Setup] Created CustomerVisualConfig at {path}");
            }
            else
            {
                config.InitializeDefaults();
                EditorUtility.SetDirty(config);
            }
        }

        private static void CleanCollider(GameObject obj)
        {
            var col = obj.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }

        public static void BuildRealisticHumanCustomerPrefab()
        {
            Material charMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Mat_Char_HumanMale.mat");
            // Load the animator controller if it exists, otherwise pass null (Stage11 will create/assign it)
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/_Project/Art/Characters/AC_Customer.controller");
            Stage11VisualPolishSetup.BuildKenneyCustomerPrefab(charMat, controller);
        }

        private static void OldBuildRealisticHumanCustomerPrefab_Deprecated()
        {
            // Deprecated: Replaced by Kenney low-poly character model integration in Stage11VisualPolishSetup
        }
    }
}
#endif
