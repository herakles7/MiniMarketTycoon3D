using System;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Store
{
    /// <summary>
    /// Manages real-time retail inventory stock levels, consumption by customers,
    /// restock operations, and persistence integration.
    /// </summary>
    public class InventoryManager : MonoBehaviourSingleton<InventoryManager>
    {
        [Header("Product Catalog")]
        [SerializeField] private List<ProductData> _productCatalog = new List<ProductData>();

        [Header("Runtime Stock Status")]
        private readonly Dictionary<string, int> _currentStock = new Dictionary<string, int>(8);
        private readonly Dictionary<string, ProductData> _catalogById = new Dictionary<string, ProductData>(8);

        public IReadOnlyList<ProductData> ProductCatalog => _productCatalog;

        /// <summary>
        /// Event fired whenever stock of an item changes: (productId, newStock, maxStock)
        /// </summary>
        public event Action<string, int, int> OnStockChanged;

        protected override void OnInitialized()
        {
            LoadCatalog();
            InitializeStockFromSaveOrDefaults();

            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
            }

            if (MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance)
            {
                MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.OnMarketLevelUpgraded += HandleMarketUpgraded;
            }
        }

        private void HandleMarketUpgraded(int newLevel)
        {
            // Broadcast updated max capacity for all products so UI and shelves adjust cleanly
            foreach (var kvp in _currentStock)
            {
                int max = GetMaxStock(kvp.Key);
                OnStockChanged?.Invoke(kvp.Key, kvp.Value, max);
            }
        }

        private void LoadCatalog()
        {
            _catalogById.Clear();

            if (_productCatalog.Count == 0)
            {
                var resProds = UnityEngine.Resources.LoadAll<ProductData>("Products");
                if (resProds != null && resProds.Length > 0)
                {
                    _productCatalog.AddRange(resProds);
                }
#if UNITY_EDITOR
                if (_productCatalog.Count == 0)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ProductData", new[] { "Assets/_Project/ScriptableObjects/Products" });
                    foreach (string guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var prod = UnityEditor.AssetDatabase.LoadAssetAtPath<ProductData>(path);
                        if (prod != null && !string.IsNullOrEmpty(prod.ID))
                        {
                            _productCatalog.Add(prod);
                        }
                    }
                }
#endif
            }

            // Index loaded products
            foreach (var prod in _productCatalog)
            {
                if (prod != null && !string.IsNullOrEmpty(prod.ID))
                {
                    _catalogById[prod.ID] = prod;
                }
            }

            // Ensure all 12 canonical products exist in catalog
            EnsureProductInCatalog("prod_water", "Pure Water", ProductCategory.Beverages, 1.00, 1.50, 20, 50, 8, ShelfType.StandardShelf);
            EnsureProductInCatalog("prod_milk", "Daily Milk", ProductCategory.Dairy, 1.50, 2.20, 15, 40, 8, ShelfType.RefrigeratedShelf);
            EnsureProductInCatalog("prod_chips", "Crunch Chips", ProductCategory.Snacks, 1.20, 2.00, 20, 50, 8, ShelfType.StandardShelf);
            EnsureProductInCatalog("prod_chocolate", "Sweet Chocolate", ProductCategory.Sweets, 1.00, 1.80, 15, 40, 8, ShelfType.StandardShelf);
            EnsureProductInCatalog("prod_soda", "Fresh Soda", ProductCategory.Beverages, 1.10, 1.90, 20, 50, 8, ShelfType.RefrigeratedShelf);
            EnsureProductInCatalog("prod_canned", "Daily Canned Food", ProductCategory.CannedFood, 1.80, 3.00, 10, 30, 8, ShelfType.StandardShelf);

            // New generic products
            EnsureProductInCatalog("prod_juice", "Orange Juice", ProductCategory.Beverages, 1.40, 2.40, 15, 40, 8, ShelfType.RefrigeratedShelf);
            EnsureProductInCatalog("prod_yogurt", "Yogurt", ProductCategory.Dairy, 1.20, 2.00, 15, 40, 8, ShelfType.RefrigeratedShelf);
            EnsureProductInCatalog("prod_cookies", "Cookies", ProductCategory.Snacks, 1.30, 2.20, 15, 45, 8, ShelfType.StandardShelf);
            EnsureProductInCatalog("prod_candy", "Candy", ProductCategory.Sweets, 0.80, 1.50, 25, 60, 8, ShelfType.StandardShelf);
            EnsureProductInCatalog("prod_sauce", "Tomato Sauce", ProductCategory.CannedFood, 1.60, 2.70, 12, 35, 8, ShelfType.StandardShelf);
            EnsureProductInCatalog("prod_rice", "Rice", ProductCategory.CannedFood, 2.00, 3.50, 10, 30, 8, ShelfType.StandardShelf);
        }

        private void EnsureProductInCatalog(string id, string name, ProductCategory cat, double purchase, double sell, int startStock, int maxStock, int shelfCap, ShelfType shelfType)
        {
            if (!_catalogById.ContainsKey(id))
            {
                var p = ScriptableObject.CreateInstance<ProductData>();
                p.Configure(id, name, cat, purchase, sell, startStock, maxStock, shelfCap, shelfType);
                _productCatalog.Add(p);
                _catalogById[id] = p;
            }
        }

        public List<ProductData> GetProductsByCategory(ProductCategory category)
        {
            List<ProductData> list = new List<ProductData>();
            for (int i = 0; i < _productCatalog.Count; i++)
            {
                var p = _productCatalog[i];
                if (p != null && p.Category == category)
                {
                    list.Add(p);
                }
            }
            return list;
        }

        private void InitializeStockFromSaveOrDefaults()
        {
            _currentStock.Clear();

            // 1. Try to load from SaveManager
            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null &&
                SaveManager.Instance.CurrentSave.Products != null && SaveManager.Instance.CurrentSave.Products.Count > 0)
            {
                foreach (var saveEntry in SaveManager.Instance.CurrentSave.Products)
                {
                    if (!string.IsNullOrEmpty(saveEntry.ProductId))
                    {
                        int max = GetMaxStock(saveEntry.ProductId);
                        _currentStock[saveEntry.ProductId] = Mathf.Clamp(saveEntry.CurrentStock, 0, max);
                    }
                }
            }

            // 2. Populate any missing products with their default StartingStock
            foreach (var kvp in _catalogById)
            {
                if (!_currentStock.ContainsKey(kvp.Key))
                {
                    int max = GetMaxStock(kvp.Key);
                    _currentStock[kvp.Key] = Mathf.Clamp(kvp.Value.StartingStock, 0, max);
                }
            }

            // Notify initial state
            foreach (var kvp in _currentStock)
            {
                int max = GetMaxStock(kvp.Key);
                OnStockChanged?.Invoke(kvp.Key, kvp.Value, max);
            }
        }

        private void HandleBeforeSave(GameSaveData saveData)
        {
            if (saveData == null) return;

            if (saveData.Products == null)
            {
                saveData.Products = new List<ProductSaveData>();
            }

            foreach (var kvp in _currentStock)
            {
                var existing = saveData.Products.Find(p => p.ProductId == kvp.Key);
                if (existing != null)
                {
                    existing.CurrentStock = kvp.Value;
                }
                else
                {
                    saveData.Products.Add(new ProductSaveData(kvp.Key, true, kvp.Value));
                }
            }
        }

        public bool HasStock(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return false;
            return _currentStock.TryGetValue(productId, out int stock) && stock > 0;
        }

        public int GetStock(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return 0;
            return _currentStock.TryGetValue(productId, out int stock) ? stock : 0;
        }

        public int GetMaxStock(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return 0;
            int baseMax = _catalogById.TryGetValue(productId, out var data) ? data.MaxStock : 50;
            if (MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance)
            {
                return MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.GetEffectiveMaxStock(baseMax);
            }
            return baseMax;
        }

        public ProductData GetProductData(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;
            _catalogById.TryGetValue(productId, out var data);
            return data;
        }

        /// <summary>
        /// Consumes 1 or more units of stock when a customer takes a product from the shelf.
        /// </summary>
        public bool TryConsumeStock(string productId, int amount = 1)
        {
            if (string.IsNullOrEmpty(productId) || amount <= 0) return false;

            if (!_currentStock.TryGetValue(productId, out int current) || current < amount)
            {
                return false;
            }

            int newStock = current - amount;
            _currentStock[productId] = newStock;

            int max = GetMaxStock(productId);
            OnStockChanged?.Invoke(productId, newStock, max);
            return true;
        }

        /// <summary>
        /// Restocks inventory up to MaxStock. Returns actual restocked count and required cost.
        /// </summary>
        public bool TryRestock(string productId, int requestedAmount, out int actualRestocked, out double totalCost)
        {
            actualRestocked = 0;
            totalCost = 0.0;

            if (string.IsNullOrEmpty(productId) || requestedAmount <= 0) return false;
            if (!_catalogById.TryGetValue(productId, out var data)) return false;

            int current = GetStock(productId);
            int maxStock = GetMaxStock(productId);
            int capacityLeft = Mathf.Max(0, maxStock - current);

            if (capacityLeft <= 0)
            {
                // Stock is already full!
                return false;
            }

            actualRestocked = Mathf.Min(requestedAmount, capacityLeft);
            totalCost = actualRestocked * data.PurchasePrice;

            int newStock = current + actualRestocked;
            _currentStock[productId] = newStock;

            OnStockChanged?.Invoke(productId, newStock, maxStock);
            return true;
        }

        protected override void OnDestroy()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnBeforeSave -= HandleBeforeSave;
            }

            if (MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance)
            {
                MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.OnMarketLevelUpgraded -= HandleMarketUpgraded;
            }

            base.OnDestroy();
        }
    }
}
