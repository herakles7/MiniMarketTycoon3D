using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Represents an individual product line in a customer's shopping list with desired and collected quantities.
    /// </summary>
    public class CustomerShoppingItem
    {
        public ProductData Product;
        private string _productId;

        public string ProductId => Product != null ? Product.ID : _productId;
        public string DisplayName => Product != null ? Product.DisplayName : _productId;
        public int QuantityWanted;
        public int QuantityCollected;

        public bool IsFulfilled => QuantityCollected >= QuantityWanted;
        public int QuantityRemaining => Mathf.Max(0, QuantityWanted - QuantityCollected);

        public CustomerShoppingItem(ProductData product, int quantityWanted = 1)
        {
            Product = product;
            _productId = product != null ? product.ID : "";
            QuantityWanted = Mathf.Max(1, quantityWanted);
            QuantityCollected = 0;
        }

        public CustomerShoppingItem(string productId, int quantityWanted = 1)
        {
            _productId = productId;
            Product = InventoryManager.HasInstance ? InventoryManager.Instance.GetProductData(productId) : null;
            QuantityWanted = Mathf.Max(1, quantityWanted);
            QuantityCollected = 0;
        }
    }

    /// <summary>
    /// Extensible customer shopping preference archetype for category affinity and bias.
    /// Expanded in Stage 8 to support personality archetypes, speed, patience, and price sensitivity.
    /// </summary>
    [System.Serializable]
    public class CustomerPreference
    {
        public CustomerPersonalityType PersonalityType = CustomerPersonalityType.Normal;
        public ProductCategory PreferredCategory = ProductCategory.Beverages;
        public float CategoryAffinity = 1.0f;
        public float PriceSensitivity = 0.5f;
        public float ShoppingSpeed = 1.0f;
        public float Patience = 1.0f;
        public int BasketSize = 2;
    }

    /// <summary>
    /// Item stored in a customer's physical basket for checkout calculation.
    /// </summary>
    public class CustomerCartItem
    {
        public string ProductId;
        public string DisplayName;
        public int Quantity;
        public double UnitSellPrice;
        public double UnitPurchasePrice;

        public double TotalRevenue => Quantity * UnitSellPrice;
        public double TotalCost => Quantity * UnitPurchasePrice;
        public double TotalProfit => TotalRevenue - TotalCost;
    }

    /// <summary>
    /// Runtime shopping state and cart data for a customer instance.
    /// Manages the wishlist with quantities, current target shelf, item collection, and checkout cart totals.
    /// </summary>
    public class CustomerShoppingData
    {
        private readonly List<CustomerShoppingItem> _shoppingItems = new List<CustomerShoppingItem>(5);
        private readonly List<string> _legacyShoppingList = new List<string>(5);
        private readonly Dictionary<string, CustomerCartItem> _cart = new Dictionary<string, CustomerCartItem>(5);

        private int _itemsWanted;
        private int _itemsCollected;
        private int _currentItemIndex;
        private CustomerInteractionPoint _currentTargetPoint;
        private int _queueIndex = -1;
        private float _remainingPatience;

        public IReadOnlyList<CustomerShoppingItem> ShoppingItems => _shoppingItems;
        public IReadOnlyList<string> ShoppingList => _legacyShoppingList;
        public IReadOnlyCollection<CustomerCartItem> CartItems => _cart.Values;
        public int ItemsWanted => _itemsWanted;
        public int ItemsCollected => _itemsCollected;
        public CustomerInteractionPoint CurrentTargetPoint => _currentTargetPoint;
        public int QueueIndex => _queueIndex;
        public float RemainingPatience => _remainingPatience;
        public CustomerPreference Preference { get; set; }

        public void Initialize(int itemsWanted, float initialPatience)
        {
            _shoppingItems.Clear();
            _legacyShoppingList.Clear();
            _cart.Clear();
            _itemsWanted = Mathf.Clamp(itemsWanted, 1, 5);
            _itemsCollected = 0;
            _currentItemIndex = 0;
            _currentTargetPoint = null;
            _queueIndex = -1;
            _remainingPatience = initialPatience;
        }

        public void AddShoppingItem(ProductData product, int quantityWanted = 1)
        {
            if (product == null) return;
            if (_shoppingItems.Count < _itemsWanted)
            {
                var item = new CustomerShoppingItem(product, quantityWanted);
                _shoppingItems.Add(item);
                _legacyShoppingList.Add(product.ID);
            }
        }

        public void AddWishlistItem(string productIdOrCategory)
        {
            if (_shoppingItems.Count < _itemsWanted)
            {
                var item = new CustomerShoppingItem(productIdOrCategory, 1);
                _shoppingItems.Add(item);
                _legacyShoppingList.Add(productIdOrCategory);
            }
        }

        public bool HasPendingItems()
        {
            return _currentItemIndex < _shoppingItems.Count;
        }

        public CustomerShoppingItem GetCurrentShoppingItem()
        {
            if (_currentItemIndex >= 0 && _currentItemIndex < _shoppingItems.Count)
            {
                return _shoppingItems[_currentItemIndex];
            }
            return null;
        }

        public string GetCurrentItem()
        {
            var item = GetCurrentShoppingItem();
            return item != null ? item.ProductId : null;
        }

        public void SetTargetPoint(CustomerInteractionPoint point)
        {
            if (_currentTargetPoint != null && _currentTargetPoint != point)
            {
                _currentTargetPoint.SetOccupied(false);
            }
            _currentTargetPoint = point;
            if (_currentTargetPoint != null)
            {
                _currentTargetPoint.SetOccupied(true);
            }
        }

        public void AddProductToCart(ProductData product, int quantity = 1)
        {
            if (product == null || quantity <= 0) return;

            string id = product.ID;
            if (_cart.TryGetValue(id, out var existingItem))
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _cart[id] = new CustomerCartItem
                {
                    ProductId = id,
                    DisplayName = product.DisplayName,
                    Quantity = quantity,
                    UnitSellPrice = product.SellPrice,
                    UnitPurchasePrice = product.PurchasePrice
                };
            }
        }

        public double CalculateCartTotal()
        {
            double total = 0.0;
            foreach (var item in _cart.Values)
            {
                total += item.TotalRevenue;
            }
            return total;
        }

        public double CalculateCartCost()
        {
            double cost = 0.0;
            foreach (var item in _cart.Values)
            {
                cost += item.TotalCost;
            }
            return cost;
        }

        public void RecordCollected(int count)
        {
            _itemsCollected += count;
        }

        public void MarkCurrentItemCollected(int count = 1)
        {
            _itemsCollected += count;
            AdvanceToNextItem();
        }

        public void SkipCurrentItem()
        {
            AdvanceToNextItem();
        }

        public void AdvanceToNextItem()
        {
            _currentItemIndex++;
            if (_currentTargetPoint != null)
            {
                _currentTargetPoint.SetOccupied(false);
                _currentTargetPoint = null;
            }
        }

        public void SetQueueIndex(int index)
        {
            _queueIndex = index;
        }

        public void ConsumePatience(float deltaTime)
        {
            _remainingPatience = Mathf.Max(0f, _remainingPatience - deltaTime);
        }

        public void Clear()
        {
            if (_currentTargetPoint != null)
            {
                _currentTargetPoint.SetOccupied(false);
                _currentTargetPoint = null;
            }
            _shoppingItems.Clear();
            _legacyShoppingList.Clear();
            _cart.Clear();
            _itemsWanted = 0;
            _itemsCollected = 0;
            _currentItemIndex = 0;
            _queueIndex = -1;
            _remainingPatience = 0f;
        }
    }
}
