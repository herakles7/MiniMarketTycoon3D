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
        MiddleAdult // 45 - 60
    }

    public enum CustomerSkinTone
    {
        Fair,
        Olive,
        Warm,
        Tan
    }

    public enum CustomerHairStyle
    {
        Short,
        Fade,
        Medium,
        Long,
        Ponytail
    }

    public enum CustomerHairColor
    {
        Black,
        DarkBrown,
        Brown,
        LightBrown,
        DarkBlonde,
        Blonde
    }

    public enum CustomerTopType
    {
        TShirt,
        Polo,
        Sweatshirt,
        Shirt,
        Blouse,
        CasualTop
    }

    public enum CustomerBottomType
    {
        Jeans,
        Chino,
        CasualPants,
        Skirt
    }

    public enum CustomerShoeType
    {
        Sneakers,
        CasualShoes
    }

    /// <summary>
    /// Runtime visual profile holding demographic, morphological, and styling attributes
    /// for stylized-realistic 3D mobile customer NPCs.
    /// </summary>
    [Serializable]
    public class CustomerVisualProfile
    {
        [Header("Demographics")]
        public CustomerGender Gender = CustomerGender.Male;
        public CustomerAgeGroup AgeGroup = CustomerAgeGroup.Adult;

        [Header("Appearance Attributes")]
        public CustomerSkinTone SkinTone = CustomerSkinTone.Warm;
        public CustomerHairStyle HairStyle = CustomerHairStyle.Short;
        public CustomerHairColor HairColor = CustomerHairColor.Brown;

        [Header("Wardrobe")]
        public CustomerTopType TopType = CustomerTopType.TShirt;
        public CustomerBottomType BottomType = CustomerBottomType.Jeans;
        public CustomerShoeType ShoeType = CustomerShoeType.Sneakers;

        [Header("Materials (Optional Overrides)")]
        public string SkinMaterialName = "Mat_Skin_Warm";
        public string HairMaterialName = "Mat_Hair_Brown";
        public string TopMaterialName = "Mat_Clothes_Blue";
        public string BottomMaterialName = "Mat_Pants_Khaki";
        public string ShoeMaterialName = "Mat_Shoes_Dark";

        [Header("Accessories")]
        public bool HasGlasses = false;
        public bool HasShoppingBasket = false;

        public string GetSignature()
        {
            return $"{Gender}_{AgeGroup}_{SkinTone}_{HairStyle}_{HairColor}_{TopType}_{BottomType}_{ShoeType}";
        }

        public string GetProfileSummary()
        {
            return $"{AgeGroup} {Gender} | Skin: {SkinTone} | Hair: {HairColor} {HairStyle} | Clothes: {TopType} & {BottomType}";
        }

        public CustomerVisualProfile Clone()
        {
            return new CustomerVisualProfile
            {
                Gender = this.Gender,
                AgeGroup = this.AgeGroup,
                SkinTone = this.SkinTone,
                HairStyle = this.HairStyle,
                HairColor = this.HairColor,
                TopType = this.TopType,
                BottomType = this.BottomType,
                ShoeType = this.ShoeType,
                SkinMaterialName = this.SkinMaterialName,
                HairMaterialName = this.HairMaterialName,
                TopMaterialName = this.TopMaterialName,
                BottomMaterialName = this.BottomMaterialName,
                ShoeMaterialName = this.ShoeMaterialName,
                HasGlasses = this.HasGlasses,
                HasShoppingBasket = this.HasShoppingBasket
            };
        }
    }
}
