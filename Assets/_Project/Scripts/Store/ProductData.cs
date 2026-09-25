using UnityEngine;

namespace MiniMarketTycoon.Store
{
    public enum ProductCategory
    {
        Bakery,
        Beverages,
        Dairy,
        Produce,
        Snacks,
        Frozen,
        PersonalCare
    }

    /// <summary>
    /// ScriptableObject defining product metadata, economy stats, and display properties.
    /// </summary>
    [CreateAssetMenu(fileName = "NewProductData", menuMenuName = "MiniMarket/Product Data")]
    public class ProductData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private ProductCategory _category;

        [Header("Economy Stats")]
        [SerializeField] private double _basePrice = 5.0;
        [SerializeField] private double _baseProfit = 2.0;
        [SerializeField] private int _unlockLevel = 1;

        public string ID => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public ProductCategory Category => _category;
        public double BasePrice => _basePrice;
        public double BaseProfit => _baseProfit;
        public int UnlockLevel => _unlockLevel;
    }
}
