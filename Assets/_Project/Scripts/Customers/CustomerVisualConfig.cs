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
        [Range(0f, 1f)] public float YoungAdultWeight = 0.30f;
        [Range(0f, 1f)] public float AdultWeight = 0.40f;
        [Range(0f, 1f)] public float MiddleAdultWeight = 0.18f;
        [Range(0f, 1f)] public float SeniorWeight = 0.12f;
        public float OlderAdultWeight => SeniorWeight;

        [Header("Body Type Weights (Aşama 11)")]
        [Range(0f, 1f)] public float SlimBodyWeight = 0.20f;
        [Range(0f, 1f)] public float AverageBodyWeight = 0.50f;
        [Range(0f, 1f)] public float AthleticBodyWeight = 0.18f;
        [Range(0f, 1f)] public float HeavyBodyWeight = 0.12f;

        [Header("Clothing Style Weights (Aşama 11)")]
        [Range(0f, 1f)] public float CasualStyleWeight = 0.45f;
        [Range(0f, 1f)] public float SmartCasualStyleWeight = 0.25f;
        [Range(0f, 1f)] public float EverydayStyleWeight = 0.20f;
        [Range(0f, 1f)] public float SportyStyleWeight = 0.10f;

        [Header("Human Anatomy & Morphology Ranges")]
        [Tooltip("Realistic height multiplier range (~0.95 - 1.05)")]
        [Range(0.85f, 1.15f)] public float MinHeightMultiplier = 0.95f;
        [Range(0.85f, 1.15f)] public float MaxHeightMultiplier = 1.05f;

        [Tooltip("Realistic body width multiplier range (0.92 - 1.08)")]
        [Range(0.85f, 1.15f)] public float MinBodyWidthMultiplier = 0.92f;
        [Range(0.85f, 1.15f)] public float MaxBodyWidthMultiplier = 1.08f;

        [Tooltip("Anatomical head scale range (0.97 - 1.03)")]
        [Range(0.90f, 1.10f)] public float MinHeadScale = 0.97f;
        [Range(0.90f, 1.10f)] public float MaxHeadScale = 1.03f;

        [Header("Accessory Probabilities")]
        [Range(0f, 1f)] public float OverallAccessoryChance = 0.18f;
        [Range(0f, 1f)] public float CapChance = 0.15f;
        [Range(0f, 1f)] public float BackpackChance = 0.12f;
        [Range(0f, 1f)] public float HandbagChance = 0.15f;
        [Range(0f, 1f)] public float ShoppingBagChance = 0.15f;

        [Header("Shopping Basket Probabilities by Personality")]
        [Range(0f, 1f)] public float BasketChanceNormal = 0.50f;
        [Range(0f, 1f)] public float BasketChanceBigShopper = 0.80f;
        [Range(0f, 1f)] public float BasketChanceQuickShopper = 0.25f;
        [Range(0f, 1f)] public float BasketChancePatientShopper = 0.50f;
        [Range(0f, 1f)] public float BasketChanceImpatientShopper = 0.40f;

        [Header("LOD Distance Thresholds (Meters)")]
        [Tooltip("Distance under which LOD0 (Full high-fidelity humanoid mesh) is rendered")]
        public float LOD0Distance = 8.0f;
        [Tooltip("Distance under which LOD1 (Medium fidelity humanoid mesh) is rendered")]
        public float LOD1Distance = 18.0f;

        [Header("Skin Tone Materials (6 Natural Tones)")]
        public string[] SkinFairMaterials = new string[] { "Mat_Skin_Fair" };
        public string[] SkinPeachMaterials = new string[] { "Mat_Skin_Peach" };
        public string[] SkinOliveMaterials = new string[] { "Mat_Skin_Olive" };
        public string[] SkinWarmMaterials = new string[] { "Mat_Skin_Warm" };
        public string[] SkinTanMaterials = new string[] { "Mat_Skin_Tan" };
        public string[] SkinDeepMaterials = new string[] { "Mat_Skin_Deep" };

        [Header("Hair Materials")]
        public string[] HairBlackMaterials = new string[] { "Mat_Hair_Black" };
        public string[] HairDarkBrownMaterials = new string[] { "Mat_Hair_DarkBrown", "Mat_Hair_Black" };
        public string[] HairBrownMaterials = new string[] { "Mat_Hair_Brown" };
        public string[] HairLightBrownMaterials = new string[] { "Mat_Hair_LightBrown", "Mat_Hair_Brown" };
        public string[] HairBlondeMaterials = new string[] { "Mat_Hair_Blonde" };
        public string[] HairGreyMaterials = new string[] { "Mat_Hair_Grey" };

        [Header("Eye Iris Materials")]
        public string IrisBrownMaterial = "Mat_Eyes_Iris_Brown";
        public string IrisBlueMaterial = "Mat_Eyes_Iris_Blue";
        public string IrisGreenMaterial = "Mat_Eyes_Iris_Green";

        [Header("Top Clothing Materials")]
        public string[] MaleTopMaterials = new string[]
        {
            "Mat_Clothes_Blue",
            "Mat_Clothes_Teal",
            "Mat_Clothes_Maroon",
            "Mat_Clothes_Grey",
            "Mat_Clothes_Green",
            "Mat_Clothes_Charcoal",
            "Mat_Clothes_Olive",
            "Mat_Clothes_Navy"
        };

        public string[] FemaleTopMaterials = new string[]
        {
            "Mat_Clothes_Teal",
            "Mat_Clothes_Maroon",
            "Mat_Clothes_Blue",
            "Mat_Clothes_Coral",
            "Mat_Clothes_White",
            "Mat_Clothes_Beige",
            "Mat_Clothes_Navy"
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
                case CustomerSkinTone.Peach:
                    return GetRandomString(SkinPeachMaterials, "Mat_Skin_Peach");
                case CustomerSkinTone.Olive:
                    return GetRandomString(SkinOliveMaterials, "Mat_Skin_Olive");
                case CustomerSkinTone.Warm:
                    return GetRandomString(SkinWarmMaterials, "Mat_Skin_Warm");
                case CustomerSkinTone.Tan:
                    return GetRandomString(SkinTanMaterials, "Mat_Skin_Tan");
                case CustomerSkinTone.Deep:
                    return GetRandomString(SkinDeepMaterials, "Mat_Skin_Deep");
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
                    return GetRandomString(HairDarkBrownMaterials, "Mat_Hair_DarkBrown");
                case CustomerHairColor.Brown:
                    return GetRandomString(HairBrownMaterials, "Mat_Hair_Brown");
                case CustomerHairColor.LightBrown:
                    return GetRandomString(HairLightBrownMaterials, "Mat_Hair_LightBrown");
                case CustomerHairColor.DarkBlonde:
                case CustomerHairColor.Blonde:
                    return GetRandomString(HairBlondeMaterials, "Mat_Hair_Blonde");
                case CustomerHairColor.Grey:
                    return GetRandomString(HairGreyMaterials, "Mat_Hair_Grey");
                default:
                    return "Mat_Hair_Brown";
            }
        }

        public string GetIrisMaterialName(CustomerIrisColor iris)
        {
            switch (iris)
            {
                case CustomerIrisColor.Blue: return IrisBlueMaterial;
                case CustomerIrisColor.Green: return IrisGreenMaterial;
                case CustomerIrisColor.Brown:
                default:
                    return IrisBrownMaterial;
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

        public float GetBasketChanceForPersonality(CustomerPersonalityType personality)
        {
            switch (personality)
            {
                case CustomerPersonalityType.BigShopper: return BasketChanceBigShopper;
                case CustomerPersonalityType.QuickShopper: return BasketChanceQuickShopper;
                case CustomerPersonalityType.PatientShopper: return BasketChancePatientShopper;
                case CustomerPersonalityType.ImpatientShopper: return BasketChanceImpatientShopper;
                case CustomerPersonalityType.Normal:
                case CustomerPersonalityType.BargainHunter:
                default:
                    return BasketChanceNormal;
            }
        }

        public CustomerBodyType GetRandomBodyType()
        {
            float total = SlimBodyWeight + AverageBodyWeight + AthleticBodyWeight + HeavyBodyWeight;
            if (total <= 0f) return CustomerBodyType.Average;

            float roll = UnityEngine.Random.value * total;
            if (roll < SlimBodyWeight) return CustomerBodyType.Slim;
            if (roll < SlimBodyWeight + AverageBodyWeight) return CustomerBodyType.Average;
            if (roll < SlimBodyWeight + AverageBodyWeight + AthleticBodyWeight) return CustomerBodyType.Athletic;
            return CustomerBodyType.Heavy;
        }

        public CustomerClothingStyle GetRandomClothingStyle()
        {
            float total = CasualStyleWeight + SmartCasualStyleWeight + EverydayStyleWeight + SportyStyleWeight;
            if (total <= 0f) return CustomerClothingStyle.Casual;

            float roll = UnityEngine.Random.value * total;
            if (roll < CasualStyleWeight) return CustomerClothingStyle.Casual;
            if (roll < CasualStyleWeight + SmartCasualStyleWeight) return CustomerClothingStyle.SmartCasual;
            if (roll < CasualStyleWeight + SmartCasualStyleWeight + EverydayStyleWeight) return CustomerClothingStyle.Everyday;
            return CustomerClothingStyle.Sporty;
        }

        public CustomerAgeGroup GetRandomAgeGroup()
        {
            float total = YoungAdultWeight + AdultWeight + MiddleAdultWeight + SeniorWeight;
            if (total <= 0f) return CustomerAgeGroup.Adult;

            float roll = UnityEngine.Random.value * total;
            if (roll < YoungAdultWeight) return CustomerAgeGroup.YoungAdult;
            if (roll < YoungAdultWeight + AdultWeight) return CustomerAgeGroup.Adult;
            if (roll < YoungAdultWeight + AdultWeight + MiddleAdultWeight) return CustomerAgeGroup.MiddleAged;
            return CustomerAgeGroup.Senior;
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
            MinHeightMultiplier = 0.92f;
            MaxHeightMultiplier = 1.08f;
            MinBodyWidthMultiplier = 0.92f;
            MaxBodyWidthMultiplier = 1.08f;
            MinHeadScale = 0.97f;
            MaxHeadScale = 1.03f;
            OverallAccessoryChance = 0.18f;
            CapChance = 0.15f;
            BackpackChance = 0.12f;
            HandbagChance = 0.15f;
            ShoppingBagChance = 0.15f;
            BasketChanceNormal = 0.50f;
            BasketChanceBigShopper = 0.80f;
            BasketChanceQuickShopper = 0.25f;
            BasketChancePatientShopper = 0.50f;
            BasketChanceImpatientShopper = 0.40f;
            LOD0Distance = 8.0f;
            LOD1Distance = 18.0f;
        }
    }
}
