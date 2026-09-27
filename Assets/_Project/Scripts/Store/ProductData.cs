using UnityEngine;

namespace MiniMarketTycoon.Store
{
    public enum ProductCategory
    {
        Beverages = 0,
        Dairy = 1,
        Snacks = 2,
        Sweets = 3,
        CannedFood = 4,
        Bakery = 5,
        Produce = 6,
        Frozen = 7,
        PersonalCare = 8,
        Pantry = 9
    }

    public enum ShelfType
    {
        StandardShelf = 0,
        RefrigeratedShelf = 1,
        FreezerShelf = 2
    }

    /// <summary>
    /// ScriptableObject defining product metadata, economy pricing, stock capacities,
    /// shelf display rules, and 3D visual prefab linkages.
    /// </summary>
    [CreateAssetMenu(fileName = "NewProductData", menuName = "MiniMarket/Product Data")]
    public class ProductData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private ProductCategory _category;
        [SerializeField] private ShelfType _shelfType = ShelfType.StandardShelf;

        [Header("Economy Pricing")]
        [Tooltip("Cost paid by the store to restock 1 unit of this product.")]
        [SerializeField] private double _purchasePrice = 1.0;

        [Tooltip("Price paid by customer when purchasing 1 unit at checkout.")]
        [SerializeField] private double _sellPrice = 1.5;

        [Header("Stock & Capacity")]
        [Tooltip("Initial stock granted on new game.")]
        [SerializeField] private int _startingStock = 20;

        [Tooltip("Maximum storage capacity for this item in inventory.")]
        [SerializeField] private int _maxStock = 50;

        [Tooltip("Maximum visual 3D item capacity on the shelf display.")]
        [SerializeField] private int _shelfCapacity = 8;

        [SerializeField] private int _unlockLevel = 1;

        [Header("Visual Asset")]
        [SerializeField] private GameObject _productPrefab;

        public string ID => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public ProductCategory Category => _category;
        public ShelfType ShelfType => _shelfType;
        public string ShelfTypeName => _shelfType.ToString();

        public double PurchasePrice => _purchasePrice;
        public double SellPrice => _sellPrice;
        public double ProfitPerUnit => _sellPrice - _purchasePrice;

        public int StartingStock => _startingStock;
        public int MaxStock => _maxStock;
        public int ShelfCapacity => _shelfCapacity;
        public int UnlockLevel => _unlockLevel;
        public GameObject ProductPrefab => _productPrefab;

        // Backward compatibility properties
        public double BasePrice => _sellPrice;
        public double BaseProfit => ProfitPerUnit;

        public void Configure(string id, string name, ProductCategory cat, double purchase, double sell, int startStock, int maxStock, int shelfCap = 8, ShelfType shelfType = ShelfType.StandardShelf, GameObject prefab = null)
        {
            _id = id;
            _displayName = name;
            _category = cat;
            _purchasePrice = purchase;
            _sellPrice = sell;
            _startingStock = startStock;
            _maxStock = maxStock;
            _shelfCapacity = shelfCap;
            _shelfType = shelfType;
            _productPrefab = prefab;
        }

        // Backward compatibility Configure overload
        public void Configure(string id, string name, ProductCategory cat, double purchase, double sell, int startStock, int maxStock, int shelfCap, string legacyShelfType)
        {
            ShelfType parsedType = ShelfType.StandardShelf;
            if (legacyShelfType != null && legacyShelfType.IndexOf("refrig", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                parsedType = ShelfType.RefrigeratedShelf;
            }
            else if (legacyShelfType != null && legacyShelfType.IndexOf("freez", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                parsedType = ShelfType.FreezerShelf;
            }
            Configure(id, name, cat, purchase, sell, startStock, maxStock, shelfCap, parsedType);
        }
    }
}
