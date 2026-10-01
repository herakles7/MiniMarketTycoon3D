using System;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    public enum CustomerGender
    {
        Male,
        Female
    }

    public enum CustomerAgeGroup
    {
        YoungAdult, // 20 - 30
        Adult,      // 30 - 45
        MiddleAged, // 45 - 60
        Senior,     // 60+
        MiddleAdult = MiddleAged, // backward compatibility
        OlderAdult = Senior       // backward compatibility
    }

    public enum CustomerBodyType
    {
        Slim,
        Average,
        Athletic,
        Heavy
    }

    public enum CustomerClothingStyle
    {
        Casual,
        SmartCasual,
        Everyday,
        Sporty
    }

    public enum CustomerSkinTone
    {
        Fair,
        Peach,
        Olive,
        Warm,
        Tan,
        Deep
    }

    public enum CustomerHairStyle
    {
        Short,
        Fade,
        Medium,
        Long,
        Ponytail,
        Messy,
        Buzz
    }

    public enum CustomerHairColor
    {
        Black,
        DarkBrown,
        Brown,
        LightBrown,
        DarkBlonde,
        Blonde,
        Grey
    }

    public enum CustomerIrisColor
    {
        Brown,
        Blue,
        Green
    }

    public enum CustomerTopType
    {
        TShirt,
        Polo,
        Sweatshirt,
        Shirt,
        Blouse,
        CasualTop,
        Jacket
    }

    public enum CustomerBottomType
    {
        Jeans,
        Chino,
        CasualPants,
        Skirt,
        Trousers,
        Shorts
    }

    public enum CustomerShoeType
    {
        Sneakers,
        CasualShoes,
        Boots
    }

    /// <summary>
    /// Runtime visual profile holding demographic, morphological, and styling attributes
    /// for realistic 3D mobile customer NPCs.
    /// </summary>
    [Serializable]
    public class CustomerVisualProfile
    {
        [Header("Demographics")]
        public CustomerGender Gender = CustomerGender.Male;
        public CustomerAgeGroup AgeGroup = CustomerAgeGroup.Adult;
        public CustomerBodyType BodyType = CustomerBodyType.Average;

        [Header("Morphology")]
        [Range(0.90f, 1.10f)] public float HeightMultiplier = 1.0f;
        [Range(0.90f, 1.10f)] public float BodyWidthMultiplier = 1.0f;
        [Range(0.95f, 1.05f)] public float HeadScale = 1.0f;

        [Header("Appearance Attributes")]
        public CustomerSkinTone SkinTone = CustomerSkinTone.Warm;
        public CustomerHairStyle HairStyle = CustomerHairStyle.Short;
        public CustomerHairColor HairColor = CustomerHairColor.Brown;
        public CustomerIrisColor IrisColor = CustomerIrisColor.Brown;

        [Header("Wardrobe")]
        public CustomerClothingStyle ClothingStyle = CustomerClothingStyle.Casual;
        public CustomerTopType TopType = CustomerTopType.TShirt;
        public CustomerBottomType BottomType = CustomerBottomType.Jeans;
        public CustomerShoeType ShoeType = CustomerShoeType.Sneakers;

        [Header("Materials (Optional Overrides)")]
        public string SkinMaterialName = "Mat_Skin_Warm";
        public string HairMaterialName = "Mat_Hair_Brown";
        public string IrisMaterialName = "Mat_Eyes_Iris_Brown";
        public string TopMaterialName = "Mat_Clothes_Blue";
        public string BottomMaterialName = "Mat_Pants_Khaki";
        public string ShoeMaterialName = "Mat_Shoes_Dark";

        [Header("Accessories")]
        public bool HasGlasses = false;
        public bool HasCap = false;
        public bool HasBackpack = false;
        public bool HasHandbag = false;
        public bool HasShoppingBag = false;
        public bool HasShoppingBasket = false;

        public string GetSignature()
        {
            string acc = (HasGlasses ? "G" : "") + (HasCap ? "C" : "") + (HasBackpack ? "B" : "") + (HasHandbag ? "H" : "") + (HasShoppingBag ? "S" : "");
            return $"{Gender}_{AgeGroup}_{BodyType}_{ClothingStyle}_{SkinTone}_{HairStyle}_{HairColor}_{TopType}_{BottomType}_{ShoeType}_{acc}_{HeightMultiplier:F2}_{BodyWidthMultiplier:F2}";
        }

        public string GetProfileSummary()
        {
            return $"{AgeGroup} {Gender} [{BodyType}, {ClothingStyle}] (H:{HeightMultiplier:F2}, W:{BodyWidthMultiplier:F2}) | Skin: {SkinTone} | Eyes: {IrisColor} | Hair: {HairColor} {HairStyle} | Clothes: {TopType} & {BottomType}";
        }

        public CustomerVisualProfile Clone()
        {
            return new CustomerVisualProfile
            {
                Gender = this.Gender,
                AgeGroup = this.AgeGroup,
                BodyType = this.BodyType,
                ClothingStyle = this.ClothingStyle,
                HeightMultiplier = this.HeightMultiplier,
                BodyWidthMultiplier = this.BodyWidthMultiplier,
                HeadScale = this.HeadScale,
                SkinTone = this.SkinTone,
                HairStyle = this.HairStyle,
                HairColor = this.HairColor,
                IrisColor = this.IrisColor,
                TopType = this.TopType,
                BottomType = this.BottomType,
                ShoeType = this.ShoeType,
                SkinMaterialName = this.SkinMaterialName,
                HairMaterialName = this.HairMaterialName,
                IrisMaterialName = this.IrisMaterialName,
                TopMaterialName = this.TopMaterialName,
                BottomMaterialName = this.BottomMaterialName,
                ShoeMaterialName = this.ShoeMaterialName,
                HasGlasses = this.HasGlasses,
                HasCap = this.HasCap,
                HasBackpack = this.HasBackpack,
                HasHandbag = this.HasHandbag,
                HasShoppingBag = this.HasShoppingBag,
                HasShoppingBasket = this.HasShoppingBasket
            };
        }
    }
}
