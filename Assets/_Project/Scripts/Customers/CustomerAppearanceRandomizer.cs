using System;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Generates distinct, natural customer appearance profiles with demographic weighting,
    /// controlled anatomical morphology, and duplicate prevention to avoid cloned-looking NPC swarms in the market.
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

        private static float NextFloatRange(float min, float max)
        {
            return min + (float)Rng.NextDouble() * (max - min);
        }

        private static int NextInt(int min, int max)
        {
            return Rng.Next(min, max);
        }

        /// <summary>
        /// Generates a randomized, demographic-weighted CustomerVisualProfile.
        /// </summary>
        public static CustomerVisualProfile GenerateProfile(CustomerVisualConfig config = null, CustomerPersonalityType personality = CustomerPersonalityType.Normal)
        {
            CustomerVisualProfile profile = null;
            int maxAttempts = 3;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                profile = CreateRandomProfile(config, personality);
                string sig = profile.GetSignature();

                if (!IsRecentDuplicate(sig) || attempt == maxAttempts - 1)
                {
                    RecordSignature(sig);
                    break;
                }
            }

            return profile;
        }

        private static CustomerVisualProfile CreateRandomProfile(CustomerVisualConfig config, CustomerPersonalityType personality)
        {
            var profile = new CustomerVisualProfile();

            // 1. Gender Selection (Weighted)
            float maleProb = config != null ? config.MaleProbability : 0.50f;
            profile.Gender = NextFloat() < maleProb ? CustomerGender.Male : CustomerGender.Female;

            // 2. Age Group Selection (Weighted)
            if (config != null)
            {
                profile.AgeGroup = config.GetRandomAgeGroup();
            }
            else
            {
                float ageRoll = NextFloat();
                if (ageRoll < 0.30f) profile.AgeGroup = CustomerAgeGroup.YoungAdult;
                else if (ageRoll < 0.70f) profile.AgeGroup = CustomerAgeGroup.Adult;
                else if (ageRoll < 0.88f) profile.AgeGroup = CustomerAgeGroup.MiddleAged;
                else profile.AgeGroup = CustomerAgeGroup.Senior;
            }

            // 3. Body Type & Clothing Style Selection (Aşama 11)
            profile.BodyType = config != null ? config.GetRandomBodyType() : (CustomerBodyType)NextInt(0, 4);
            profile.ClothingStyle = config != null ? config.GetRandomClothingStyle() : (CustomerClothingStyle)NextInt(0, 4);

            // 4. Controlled Morphological Ranges (~95% - 105% controlled height)
            float minH = config != null ? config.MinHeightMultiplier : 0.95f;
            float maxH = config != null ? config.MaxHeightMultiplier : 1.05f;
            float minW = config != null ? config.MinBodyWidthMultiplier : 0.92f;
            float maxW = config != null ? config.MaxBodyWidthMultiplier : 1.08f;
            float minHead = config != null ? config.MinHeadScale : 0.97f;
            float maxHead = config != null ? config.MaxHeadScale : 1.03f;

            profile.HeightMultiplier = NextFloatRange(minH, maxH);
            profile.BodyWidthMultiplier = NextFloatRange(minW, maxW);
            profile.HeadScale = NextFloatRange(minHead, maxHead);

            // 5. Skin Tone Selection (6 Natural Tones)
            int skinCount = Enum.GetValues(typeof(CustomerSkinTone)).Length;
            profile.SkinTone = (CustomerSkinTone)NextInt(0, skinCount);

            // 6. Eye Iris Color Selection
            float irisRoll = NextFloat();
            if (irisRoll < 0.65f) profile.IrisColor = CustomerIrisColor.Brown;
            else if (irisRoll < 0.85f) profile.IrisColor = CustomerIrisColor.Blue;
            else profile.IrisColor = CustomerIrisColor.Green;

            // 7. Hair Style Selection (Short, Medium, Long for both Male and Female)
            if (profile.Gender == CustomerGender.Male)
            {
                CustomerHairStyle[] maleStyles = { CustomerHairStyle.Short, CustomerHairStyle.Fade, CustomerHairStyle.Medium, CustomerHairStyle.Long, CustomerHairStyle.Messy, CustomerHairStyle.Buzz };
                profile.HairStyle = maleStyles[NextInt(0, maleStyles.Length)];
            }
            else
            {
                CustomerHairStyle[] femaleStyles = { CustomerHairStyle.Short, CustomerHairStyle.Medium, CustomerHairStyle.Long, CustomerHairStyle.Ponytail };
                profile.HairStyle = femaleStyles[NextInt(0, femaleStyles.Length)];
            }

            // 8. Hair Color Selection
            if (profile.AgeGroup == CustomerAgeGroup.Senior)
            {
                // Senior adults have predominantly grey or silver hair
                float greyRoll = NextFloat();
                if (greyRoll < 0.70f) profile.HairColor = CustomerHairColor.Grey;
                else if (greyRoll < 0.85f) profile.HairColor = CustomerHairColor.DarkBrown;
                else profile.HairColor = CustomerHairColor.DarkBlonde;
            }
            else if (profile.AgeGroup == CustomerAgeGroup.MiddleAged)
            {
                // Middle-aged adults have partial chance of grey
                float greyRoll = NextFloat();
                if (greyRoll < 0.30f) profile.HairColor = CustomerHairColor.Grey;
                else if (greyRoll < 0.60f) profile.HairColor = CustomerHairColor.DarkBrown;
                else if (greyRoll < 0.85f) profile.HairColor = CustomerHairColor.Brown;
                else profile.HairColor = CustomerHairColor.DarkBlonde;
            }
            else
            {
                CustomerHairColor[] youngColors =
                {
                    CustomerHairColor.Black,
                    CustomerHairColor.DarkBrown,
                    CustomerHairColor.Brown,
                    CustomerHairColor.LightBrown,
                    CustomerHairColor.DarkBlonde,
                    CustomerHairColor.Blonde
                };
                profile.HairColor = youngColors[NextInt(0, youngColors.Length)];
            }

            // 9. Coordinated Clothing Selection based on ClothingStyle and Gender
            if (profile.Gender == CustomerGender.Male)
            {
                switch (profile.ClothingStyle)
                {
                    case CustomerClothingStyle.SmartCasual:
                        profile.TopType = NextFloat() < 0.5f ? CustomerTopType.Shirt : CustomerTopType.Polo;
                        profile.BottomType = NextFloat() < 0.6f ? CustomerBottomType.Chino : CustomerBottomType.Trousers;
                        profile.ShoeType = CustomerShoeType.CasualShoes;
                        break;
                    case CustomerClothingStyle.Sporty:
                        profile.TopType = NextFloat() < 0.6f ? CustomerTopType.TShirt : CustomerTopType.Sweatshirt;
                        profile.BottomType = NextFloat() < 0.5f ? CustomerBottomType.Shorts : CustomerBottomType.CasualPants;
                        profile.ShoeType = CustomerShoeType.Sneakers;
                        break;
                    case CustomerClothingStyle.Everyday:
                        profile.TopType = NextFloat() < 0.5f ? CustomerTopType.Polo : CustomerTopType.Sweatshirt;
                        profile.BottomType = NextFloat() < 0.5f ? CustomerBottomType.CasualPants : CustomerBottomType.Jeans;
                        profile.ShoeType = NextFloat() < 0.5f ? CustomerShoeType.Sneakers : CustomerShoeType.CasualShoes;
                        break;
                    case CustomerClothingStyle.Casual:
                    default:
                        profile.TopType = NextFloat() < 0.4f ? CustomerTopType.TShirt : (NextFloat() < 0.7f ? CustomerTopType.Shirt : CustomerTopType.Jacket);
                        profile.BottomType = NextFloat() < 0.6f ? CustomerBottomType.Jeans : CustomerBottomType.CasualPants;
                        profile.ShoeType = NextFloat() < 0.6f ? CustomerShoeType.Sneakers : CustomerShoeType.Boots;
                        break;
                }
            }
            else
            {
                switch (profile.ClothingStyle)
                {
                    case CustomerClothingStyle.SmartCasual:
                        profile.TopType = NextFloat() < 0.5f ? CustomerTopType.Blouse : CustomerTopType.Jacket;
                        profile.BottomType = NextFloat() < 0.5f ? CustomerBottomType.Skirt : CustomerBottomType.Trousers;
                        profile.ShoeType = CustomerShoeType.CasualShoes;
                        break;
                    case CustomerClothingStyle.Sporty:
                        profile.TopType = NextFloat() < 0.6f ? CustomerTopType.TShirt : CustomerTopType.Sweatshirt;
                        profile.BottomType = CustomerBottomType.CasualPants;
                        profile.ShoeType = CustomerShoeType.Sneakers;
                        break;
                    case CustomerClothingStyle.Everyday:
                        profile.TopType = NextFloat() < 0.5f ? CustomerTopType.CasualTop : CustomerTopType.TShirt;
                        profile.BottomType = NextFloat() < 0.6f ? CustomerBottomType.Jeans : CustomerBottomType.CasualPants;
                        profile.ShoeType = NextFloat() < 0.5f ? CustomerShoeType.Sneakers : CustomerShoeType.CasualShoes;
                        break;
                    case CustomerClothingStyle.Casual:
                    default:
                        profile.TopType = NextFloat() < 0.4f ? CustomerTopType.TShirt : (NextFloat() < 0.7f ? CustomerTopType.Blouse : CustomerTopType.Jacket);
                        profile.BottomType = NextFloat() < 0.5f ? CustomerBottomType.Jeans : CustomerBottomType.Skirt;
                        profile.ShoeType = NextFloat() < 0.6f ? CustomerShoeType.Sneakers : CustomerShoeType.Boots;
                        break;
                }
            }

            // 10. Resolve Materials via Config if available
            if (config != null)
            {
                profile.SkinMaterialName = config.GetSkinMaterialName(profile.SkinTone);
                profile.HairMaterialName = config.GetHairMaterialName(profile.HairColor);
                profile.IrisMaterialName = config.GetIrisMaterialName(profile.IrisColor);
                profile.TopMaterialName = config.GetTopMaterialName(profile.Gender);
                profile.BottomMaterialName = config.GetBottomMaterialName();
                profile.ShoeMaterialName = config.GetShoeMaterialName();
            }
            else
            {
                profile.SkinMaterialName = GetFallbackSkin(profile.SkinTone);
                profile.HairMaterialName = GetFallbackHair(profile.HairColor);
                profile.IrisMaterialName = GetFallbackIris(profile.IrisColor);
                profile.TopMaterialName = GetFallbackTop(profile.Gender);
                profile.BottomMaterialName = GetFallbackBottom();
                profile.ShoeMaterialName = "Mat_Shoes_Dark";
            }

            // 11. Accessories (~15-20% chance each, bounded)
            float glassesChance = (profile.AgeGroup == CustomerAgeGroup.Senior) ? 0.55f : (profile.AgeGroup == CustomerAgeGroup.MiddleAged ? 0.35f : (profile.AgeGroup == CustomerAgeGroup.Adult ? 0.20f : 0.12f));
            profile.HasGlasses = NextFloat() < glassesChance;

            float overallAccRoll = NextFloat();
            float accChance = config != null ? config.OverallAccessoryChance : 0.18f;

            if (overallAccRoll < accChance)
            {
                float accTypeRoll = NextFloat();
                if (accTypeRoll < 0.30f)
                {
                    profile.HasCap = true;
                }
                else if (accTypeRoll < 0.55f)
                {
                    if (profile.Gender == CustomerGender.Female) profile.HasHandbag = true;
                    else profile.HasBackpack = true;
                }
                else if (accTypeRoll < 0.80f)
                {
                    profile.HasShoppingBag = true;
                }
                else
                {
                    profile.HasBackpack = true;
                }
            }

            // 11. Personality Shopping Basket assignment
            float basketChance = config != null ? config.GetBasketChanceForPersonality(personality) : 0.50f;
            profile.HasShoppingBasket = NextFloat() < basketChance;

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
                case CustomerSkinTone.Peach: return "Mat_Skin_Peach";
                case CustomerSkinTone.Olive: return "Mat_Skin_Olive";
                case CustomerSkinTone.Warm: return "Mat_Skin_Warm";
                case CustomerSkinTone.Tan: return "Mat_Skin_Tan";
                case CustomerSkinTone.Deep: return "Mat_Skin_Deep";
                default: return "Mat_Skin_Warm";
            }
        }

        private static string GetFallbackHair(CustomerHairColor color)
        {
            switch (color)
            {
                case CustomerHairColor.Black: return "Mat_Hair_Black";
                case CustomerHairColor.DarkBrown: return "Mat_Hair_DarkBrown";
                case CustomerHairColor.Brown: return "Mat_Hair_Brown";
                case CustomerHairColor.LightBrown: return "Mat_Hair_LightBrown";
                case CustomerHairColor.Blonde: case CustomerHairColor.DarkBlonde: return "Mat_Hair_Blonde";
                case CustomerHairColor.Grey: return "Mat_Hair_Grey";
                default: return "Mat_Hair_Brown";
            }
        }

        private static string GetFallbackIris(CustomerIrisColor iris)
        {
            switch (iris)
            {
                case CustomerIrisColor.Blue: return "Mat_Eyes_Iris_Blue";
                case CustomerIrisColor.Green: return "Mat_Eyes_Iris_Green";
                case CustomerIrisColor.Brown:
                default:
                    return "Mat_Eyes_Iris_Brown";
            }
        }

        private static string GetFallbackTop(CustomerGender gender)
        {
            string[] maleTops = { "Mat_Clothes_Blue", "Mat_Clothes_Teal", "Mat_Clothes_Maroon", "Mat_Clothes_Charcoal", "Mat_Clothes_Olive" };
            string[] femaleTops = { "Mat_Clothes_Teal", "Mat_Clothes_Maroon", "Mat_Clothes_Blue", "Mat_Clothes_Coral", "Mat_Clothes_Beige" };
            var arr = gender == CustomerGender.Male ? maleTops : femaleTops;
            return arr[NextInt(0, arr.Length)];
        }

        private static string GetFallbackBottom()
        {
            string[] pants = { "Mat_Pants_DarkGrey", "Mat_Pants_Khaki", "Mat_Pants_Navy", "Mat_Pants_LightJeans", "Mat_Pants_Black" };
            return pants[NextInt(0, pants.Length)];
        }
    }
}
