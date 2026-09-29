using System;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Generates distinct, natural customer appearance profiles with demographic weighting
    /// and duplicate prevention to avoid cloned-looking NPC swarms in the market.
    /// Uses high-performance allocation-free random generation.
    /// </summary>
    public static class CustomerAppearanceRandomizer
    {
        private const int RecentHistoryCapacity = 8;
        private static readonly string[] RecentSignatures = new string[RecentHistoryCapacity];
        private static int _historyIndex = 0;
        private static readonly System.Random Rng = new System.Random();

        private static float NextFloat()
        {
            return (float)Rng.NextDouble();
        }

        private static int NextInt(int min, int max)
        {
            return Rng.Next(min, max);
        }

        /// <summary>
        /// Generates a randomized, demographic-weighted CustomerVisualProfile.
        /// </summary>
        public static CustomerVisualProfile GenerateProfile(CustomerVisualConfig config = null)
        {
            CustomerVisualProfile profile = null;
            int maxAttempts = 3;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                profile = CreateRandomProfile(config);
                string sig = profile.GetSignature();

                if (!IsRecentDuplicate(sig) || attempt == maxAttempts - 1)
                {
                    RecordSignature(sig);
                    break;
                }
            }

            return profile;
        }

        private static CustomerVisualProfile CreateRandomProfile(CustomerVisualConfig config)
        {
            var profile = new CustomerVisualProfile();

            // 1. Gender Selection (Weighted)
            float maleProb = config != null ? config.MaleProbability : 0.50f;
            profile.Gender = NextFloat() < maleProb ? CustomerGender.Male : CustomerGender.Female;

            // 2. Age Group Selection (Weighted)
            float youngWeight = config != null ? config.YoungAdultWeight : 0.35f;
            float adultWeight = config != null ? config.AdultWeight : 0.45f;
            float middleWeight = config != null ? config.MiddleAdultWeight : 0.20f;
            float totalAgeWeight = youngWeight + adultWeight + middleWeight;
            float ageRoll = NextFloat() * (totalAgeWeight > 0f ? totalAgeWeight : 1f);

            if (ageRoll < youngWeight)
            {
                profile.AgeGroup = CustomerAgeGroup.YoungAdult;
            }
            else if (ageRoll < youngWeight + adultWeight)
            {
                profile.AgeGroup = CustomerAgeGroup.Adult;
            }
            else
            {
                profile.AgeGroup = CustomerAgeGroup.MiddleAdult;
            }

            // 3. Skin Tone Selection
            int skinCount = Enum.GetValues(typeof(CustomerSkinTone)).Length;
            profile.SkinTone = (CustomerSkinTone)NextInt(0, skinCount);

            // 4. Hair Style Selection
            if (profile.Gender == CustomerGender.Male)
            {
                CustomerHairStyle[] maleStyles = { CustomerHairStyle.Short, CustomerHairStyle.Fade, CustomerHairStyle.Medium };
                profile.HairStyle = maleStyles[NextInt(0, maleStyles.Length)];
            }
            else
            {
                CustomerHairStyle[] femaleStyles = { CustomerHairStyle.Short, CustomerHairStyle.Medium, CustomerHairStyle.Long, CustomerHairStyle.Ponytail };
                profile.HairStyle = femaleStyles[NextInt(0, femaleStyles.Length)];
            }

            // 5. Hair Color Selection
            int hairColorCount = Enum.GetValues(typeof(CustomerHairColor)).Length;
            profile.HairColor = (CustomerHairColor)NextInt(0, hairColorCount);

            // Middle adult has higher chance of darker brown or greying/lighter tones
            if (profile.AgeGroup == CustomerAgeGroup.MiddleAdult && NextFloat() < 0.35f)
            {
                profile.HairColor = CustomerHairColor.DarkBrown;
            }

            // 6. Clothing Selection
            if (profile.Gender == CustomerGender.Male)
            {
                CustomerTopType[] maleTops = { CustomerTopType.TShirt, CustomerTopType.Polo, CustomerTopType.Sweatshirt, CustomerTopType.Shirt };
                profile.TopType = maleTops[NextInt(0, maleTops.Length)];

                CustomerBottomType[] maleBottoms = { CustomerBottomType.Jeans, CustomerBottomType.Chino, CustomerBottomType.CasualPants };
                profile.BottomType = maleBottoms[NextInt(0, maleBottoms.Length)];
            }
            else
            {
                CustomerTopType[] femaleTops = { CustomerTopType.TShirt, CustomerTopType.Blouse, CustomerTopType.Sweatshirt, CustomerTopType.CasualTop };
                profile.TopType = femaleTops[NextInt(0, femaleTops.Length)];

                CustomerBottomType[] femaleBottoms = { CustomerBottomType.Jeans, CustomerBottomType.Skirt, CustomerBottomType.CasualPants };
                profile.BottomType = femaleBottoms[NextInt(0, femaleBottoms.Length)];
            }

            profile.ShoeType = NextFloat() < 0.65f ? CustomerShoeType.Sneakers : CustomerShoeType.CasualShoes;

            // 7. Resolve Materials via Config if available
            if (config != null)
            {
                profile.SkinMaterialName = config.GetSkinMaterialName(profile.SkinTone);
                profile.HairMaterialName = config.GetHairMaterialName(profile.HairColor);
                profile.TopMaterialName = config.GetTopMaterialName(profile.Gender);
                profile.BottomMaterialName = config.GetBottomMaterialName();
                profile.ShoeMaterialName = config.GetShoeMaterialName();
            }
            else
            {
                profile.SkinMaterialName = GetFallbackSkin(profile.SkinTone);
                profile.HairMaterialName = GetFallbackHair(profile.HairColor);
                profile.TopMaterialName = GetFallbackTop(profile.Gender);
                profile.BottomMaterialName = GetFallbackBottom();
                profile.ShoeMaterialName = "Mat_Shoes_Dark";
            }

            // Glasses accessory for adult/middle adult
            profile.HasGlasses = profile.AgeGroup == CustomerAgeGroup.MiddleAdult ? NextFloat() < 0.50f : NextFloat() < 0.20f;
            profile.HasShoppingBasket = false;

            return profile;
        }

        private static bool IsRecentDuplicate(string sig)
        {
            if (string.IsNullOrEmpty(sig)) return false;
            for (int i = 0; i < RecentHistoryCapacity; i++)
            {
                if (RecentSignatures[i] == sig) return true;
            }
            return false;
        }

        private static void RecordSignature(string sig)
        {
            RecentSignatures[_historyIndex] = sig;
            _historyIndex = (_historyIndex + 1) % RecentHistoryCapacity;
        }

        public static void ClearHistory()
        {
            for (int i = 0; i < RecentHistoryCapacity; i++)
            {
                RecentSignatures[i] = null;
            }
            _historyIndex = 0;
        }

        private static string GetFallbackSkin(CustomerSkinTone tone)
        {
            switch (tone)
            {
                case CustomerSkinTone.Fair: return "Mat_Skin_Fair";
                case CustomerSkinTone.Olive: return "Mat_Skin_Olive";
                case CustomerSkinTone.Warm: return "Mat_Skin_Warm";
                case CustomerSkinTone.Tan: return "Mat_Skin_Olive";
                default: return "Mat_Skin_Warm";
            }
        }

        private static string GetFallbackHair(CustomerHairColor color)
        {
            switch (color)
            {
                case CustomerHairColor.Black: return "Mat_Hair_Black";
                case CustomerHairColor.Blonde: case CustomerHairColor.DarkBlonde: return "Mat_Hair_Blonde";
                default: return "Mat_Hair_Brown";
            }
        }

        private static string GetFallbackTop(CustomerGender gender)
        {
            string[] maleTops = { "Mat_Clothes_Blue", "Mat_Clothes_Teal", "Mat_Clothes_Maroon" };
            string[] femaleTops = { "Mat_Clothes_Teal", "Mat_Clothes_Maroon", "Mat_Clothes_Blue" };
            var arr = gender == CustomerGender.Male ? maleTops : femaleTops;
            return arr[NextInt(0, arr.Length)];
        }

        private static string GetFallbackBottom()
        {
            string[] pants = { "Mat_Pants_DarkGrey", "Mat_Pants_Khaki", "Mat_Pants_Navy" };
            return pants[NextInt(0, pants.Length)];
        }
    }
}
