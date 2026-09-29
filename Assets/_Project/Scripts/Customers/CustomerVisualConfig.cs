using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    [CreateAssetMenu(fileName = "CustomerVisualConfig", menuName = "MiniMarket/Customer Visual Config")]
    public class CustomerVisualConfig : ScriptableObject
    {
        [Header("Demographic Spawn Weights")]
        [Range(0f, 1f)] public float MaleProbability = 0.50f;
        [Range(0f, 1f)] public float FemaleProbability = 0.50f;

        [Header("Age Distribution Weights")]
        [Range(0f, 1f)] public float YoungAdultWeight = 0.35f;
        [Range(0f, 1f)] public float AdultWeight = 0.45f;
        [Range(0f, 1f)] public float MiddleAdultWeight = 0.20f;

        [Header("LOD Distance Thresholds (Meters)")]
        [Tooltip("Distance under which LOD0 (Full high-fidelity humanoid mesh) is rendered")]
        public float LOD0Distance = 8.0f;
        [Tooltip("Distance under which LOD1 (Medium fidelity humanoid mesh) is rendered")]
        public float LOD1Distance = 18.0f;

        [Header("Skin Tone Materials")]
        public string[] SkinFairMaterials = new string[] { "Mat_Skin_Fair" };
        public string[] SkinOliveMaterials = new string[] { "Mat_Skin_Olive" };
        public string[] SkinWarmMaterials = new string[] { "Mat_Skin_Warm" };
        public string[] SkinTanMaterials = new string[] { "Mat_Skin_Tan", "Mat_Skin_Warm" };

        [Header("Hair Materials")]
        public string[] HairBlackMaterials = new string[] { "Mat_Hair_Black" };
        public string[] HairDarkBrownMaterials = new string[] { "Mat_Hair_DarkBrown", "Mat_Hair_Black" };
        public string[] HairBrownMaterials = new string[] { "Mat_Hair_Brown" };
        public string[] HairLightBrownMaterials = new string[] { "Mat_Hair_LightBrown", "Mat_Hair_Brown" };
        public string[] HairBlondeMaterials = new string[] { "Mat_Hair_Blonde" };

        [Header("Top Clothing Materials")]
        public string[] MaleTopMaterials = new string[]
        {
            "Mat_Clothes_Blue",
            "Mat_Clothes_Teal",
            "Mat_Clothes_Maroon",
            "Mat_Clothes_Grey",
            "Mat_Clothes_Green"
        };

        public string[] FemaleTopMaterials = new string[]
        {
            "Mat_Clothes_Teal",
            "Mat_Clothes_Maroon",
            "Mat_Clothes_Blue",
            "Mat_Clothes_Coral",
            "Mat_Clothes_White"
        };

        [Header("Bottom Clothing Materials")]
        public string[] BottomMaterials = new string[]
        {
            "Mat_Pants_DarkGrey",
            "Mat_Pants_Khaki",
            "Mat_Pants_Navy",
            "Mat_Pants_Black",
            "Mat_Pants_LightJeans"
        };

        [Header("Shoe Materials")]
        public string[] ShoeMaterials = new string[]
        {
            "Mat_Shoes_Dark",
            "Mat_Shoes_White",
            "Mat_Shoes_Brown"
        };

        public string GetSkinMaterialName(CustomerSkinTone tone)
        {
            switch (tone)
            {
                case CustomerSkinTone.Fair:
                    return GetRandomString(SkinFairMaterials, "Mat_Skin_Fair");
                case CustomerSkinTone.Olive:
                    return GetRandomString(SkinOliveMaterials, "Mat_Skin_Olive");
                case CustomerSkinTone.Warm:
                    return GetRandomString(SkinWarmMaterials, "Mat_Skin_Warm");
                case CustomerSkinTone.Tan:
                    return GetRandomString(SkinTanMaterials, "Mat_Skin_Olive");
                default:
                    return "Mat_Skin_Warm";
            }
        }

        public string GetHairMaterialName(CustomerHairColor color)
        {
            switch (color)
            {
                case CustomerHairColor.Black:
                    return GetRandomString(HairBlackMaterials, "Mat_Hair_Black");
                case CustomerHairColor.DarkBrown:
                    return GetRandomString(HairDarkBrownMaterials, "Mat_Hair_Brown");
                case CustomerHairColor.Brown:
                    return GetRandomString(HairBrownMaterials, "Mat_Hair_Brown");
                case CustomerHairColor.LightBrown:
                    return GetRandomString(HairLightBrownMaterials, "Mat_Hair_Brown");
                case CustomerHairColor.DarkBlonde:
                    return GetRandomString(HairBlondeMaterials, "Mat_Hair_Blonde");
                default:
                    return "Mat_Hair_Brown";
            }
        }

        public string GetTopMaterialName(CustomerGender gender)
        {
            var arr = gender == CustomerGender.Male ? MaleTopMaterials : FemaleTopMaterials;
            return GetRandomString(arr, "Mat_Clothes_Blue");
        }

        public string GetBottomMaterialName()
        {
            return GetRandomString(BottomMaterials, "Mat_Pants_Khaki");
        }

        public string GetShoeMaterialName()
        {
            return GetRandomString(ShoeMaterials, "Mat_Shoes_Dark");
        }

        private string GetRandomString(string[] options, string fallback)
        {
            if (options != null && options.Length > 0)
            {
                return options[UnityEngine.Random.Range(0, options.Length)];
            }
            return fallback;
        }

        public void InitializeDefaults()
        {
            MaleProbability = 0.50f;
            FemaleProbability = 0.50f;
            YoungAdultWeight = 0.35f;
            AdultWeight = 0.45f;
            MiddleAdultWeight = 0.20f;
            LOD0Distance = 8.0f;
            LOD1Distance = 18.0f;
        }
    }
}
