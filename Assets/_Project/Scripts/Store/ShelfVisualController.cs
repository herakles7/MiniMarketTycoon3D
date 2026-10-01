using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Customers;

namespace MiniMarketTycoon.Store
{
    /// <summary>
    /// Controls a 3D physical retail display unit (gondola, refrigerator, or wall shelf).
    /// Manages the visual product fullness density synchronized with real-time InventoryManager stock,
    /// links to assigned ProductData, shelf type, customer interaction standing points,
    /// and player stocking interactions via physical delivery boxes.
    /// </summary>
    [SelectionBase]
    public class ShelfVisualController : MonoBehaviour, MiniMarketTycoon.Gameplay.IInteractable
    {
        [Header("Product & Shelf Configuration")]
        [SerializeField] private ProductData _productData;
        [SerializeField] private string _productId = "prod_water";
        [SerializeField] private ShelfType _shelfType = ShelfType.StandardShelf;
        [SerializeField] private int _maxVisibleItems = 8;

        [Header("Visual Display Items")]
        [SerializeField] private List<GameObject> _displayItems = new List<GameObject>();

        [Header("Customer Interaction")]
        [SerializeField] private List<CustomerInteractionPoint> _interactionPoints = new List<CustomerInteractionPoint>();

        public ProductData ProductData => _productData;
        public string ProductId => _productData != null ? _productData.ID : _productId;
        public ShelfType ShelfType => _shelfType;
        public int MaxVisibleItems => _maxVisibleItems;
        public IReadOnlyList<GameObject> DisplayItems => _displayItems;
        public IReadOnlyList<CustomerInteractionPoint> InteractionPoints => _interactionPoints;

        public int CurrentStock => InventoryManager.HasInstance ? InventoryManager.Instance.GetStock(ProductId) : 0;
        public int MaxStock => InventoryManager.HasInstance ? InventoryManager.Instance.GetMaxStock(ProductId) : (_productData != null ? _productData.MaxStock : 50);

        private void OnEnable()
        {
            if (ShelfManager.Instance != null)
            {
                ShelfManager.Instance.RegisterShelf(this);
            }
        }

        private void Start()
        {
            if (ShelfManager.Instance != null)
            {
                ShelfManager.Instance.RegisterShelf(this);
            }

            if (_displayItems.Count == 0)
            {
                DiscoverDisplayItems();
            }

            if (_interactionPoints.Count == 0)
            {
                DiscoverInteractionPoints();
            }

            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnStockChanged += HandleStockChanged;
                int current = InventoryManager.Instance.GetStock(ProductId);
                int max = InventoryManager.Instance.GetMaxStock(ProductId);
                UpdateVisualDisplay(current, max);
            }
        }

        public void Initialize(ProductData data, ShelfType shelfType, List<GameObject> items, List<CustomerInteractionPoint> interactionPoints = null)
        {
            _productData = data;
            if (data != null)
            {
                _productId = data.ID;
            }
            _shelfType = shelfType;
            _displayItems = items ?? new List<GameObject>();
            if (interactionPoints != null)
            {
                _interactionPoints = interactionPoints;
            }
            else
            {
                DiscoverInteractionPoints();
            }

            if (ShelfManager.HasInstance)
            {
                ShelfManager.Instance.RegisterShelf(this);
            }
        }

        public void Initialize(string productId, List<GameObject> items)
        {
            _productId = productId;
            if (InventoryManager.HasInstance)
            {
                _productData = InventoryManager.Instance.GetProductData(productId);
            }
            _displayItems = items ?? new List<GameObject>();
            DiscoverInteractionPoints();

            if (ShelfManager.HasInstance)
            {
                ShelfManager.Instance.RegisterShelf(this);
            }
        }

        public void DiscoverDisplayItems()
        {
            _displayItems.Clear();
            var products = GetComponentsInChildren<ProductVisual>(true);
            foreach (var p in products)
            {
                _displayItems.Add(p.gameObject);
            }
        }

        public void DiscoverInteractionPoints()
        {
            _interactionPoints.Clear();
            var points = GetComponentsInChildren<CustomerInteractionPoint>(true);
            if (points != null && points.Length > 0)
            {
                _interactionPoints.AddRange(points);
            }
        }

        /// <summary>
        /// Retrieves an interaction standing point for customer pathfinding.
        /// Prioritizes unoccupied spots if available.
        /// </summary>
        public CustomerInteractionPoint GetAvailableInteractionPoint()
        {
            if (_interactionPoints.Count == 0)
            {
                DiscoverInteractionPoints();
            }

            if (_interactionPoints.Count == 0) return null;

            for (int i = 0; i < _interactionPoints.Count; i++)
            {
                var pt = _interactionPoints[i];
                if (pt != null && !pt.IsOccupied)
                {
                    return pt;
                }
            }

            return _interactionPoints[0];
        }

        private void HandleStockChanged(string changedProductId, int currentStock, int maxStock)
        {
            if (string.Equals(ProductId, changedProductId, System.StringComparison.OrdinalIgnoreCase))
            {
                UpdateVisualDisplay(currentStock, maxStock);
            }
        }

        /// <summary>
        /// Updates the 3D visual count of products on the shelf according to stock level.
        /// MaxStock -> All items visible (e.g. 8)
        /// 50% Stock -> Half items visible (e.g. 4)
        /// > 0 Stock -> At least 1 item visible
        /// 0 Stock -> Empty shelf (0 items visible)
        /// </summary>
        public void UpdateVisualDisplay(int currentStock, int maxStock)
        {
            if (_displayItems.Count == 0) return;

            int targetVisible = 0;

            if (currentStock <= 0)
            {
                targetVisible = 0; // Empty shelf
            }
            else if (maxStock > 0)
            {
                float ratio = (float)currentStock / maxStock;
                targetVisible = Mathf.RoundToInt(ratio * _displayItems.Count);
                targetVisible = Mathf.Clamp(targetVisible, 1, _displayItems.Count);
            }
            else
            {
                targetVisible = _displayItems.Count;
            }

            for (int i = 0; i < _displayItems.Count; i++)
            {
                if (_displayItems[i] != null)
                {
                    _displayItems[i].SetActive(i < targetVisible);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // IInteractable Implementation (Player Physical Interaction)
        // ─────────────────────────────────────────────────────────────────────────
        public string GetInteractionPrompt()
        {
            var player = MiniMarketTycoon.Gameplay.PlayerManagerController.Instance;
            if (player != null && player.IsCarryingBox && player.CarriedItem is MiniMarketTycoon.Gameplay.ProductBoxController box)
            {
                if (box.IsEmpty)
                {
                    return "[Boş Kutu] Çöpe At";
                }

                if (string.Equals(box.ProductId, ProductId, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (CurrentStock >= MaxStock)
                    {
                        return $"Raf Dolu ({CurrentStock}/{MaxStock})";
                    }
                    return $"[E] Rafı Doldur (+{box.ItemCount})";
                }

                return "Farklı Ürün Kolisi!";
            }

            string name = _productData != null ? _productData.DisplayName : ProductId.Replace("prod_", "").ToUpper();
            return $"[E] {name} ({CurrentStock}/{MaxStock})";
        }

        public Vector3 GetPromptWorldPosition()
        {
            return transform.position + Vector3.up * 1.4f;
        }

        public bool CanInteract(MiniMarketTycoon.Gameplay.PlayerManagerController player)
        {
            return true;
        }

        public void OnInteract(MiniMarketTycoon.Gameplay.PlayerManagerController player)
        {
            if (player == null) return;

            if (player.IsCarryingBox && player.CarriedItem is MiniMarketTycoon.Gameplay.ProductBoxController box)
            {
                if (!box.IsEmpty && string.Equals(box.ProductId, ProductId, System.StringComparison.OrdinalIgnoreCase))
                {
                    int needed = MaxStock - CurrentStock;
                    if (needed > 0)
                    {
                        int unloaded = box.UnloadItems(needed);
                        if (unloaded > 0 && InventoryManager.HasInstance)
                        {
                            InventoryManager.Instance.Restock(ProductId, unloaded);
                            if (MiniMarketTycoon.UI.FloatingFeedbackManager.HasInstance)
                            {
                                MiniMarketTycoon.UI.FloatingFeedbackManager.Instance.ShowMoodFeedback(
                                    transform.position + Vector3.up * 1.5f,
                                    $"+{unloaded} {box.ProductId.Replace("prod_", "").ToUpper()}",
                                    new Color(0.2f, 0.95f, 0.35f, 1f));
                            }
                        }
                    }
                }
            }
            else
            {
                // Open Restock / Wholesale popup
                if (MiniMarketTycoon.UI.UIManager.HasInstance)
                {
                    MiniMarketTycoon.UI.UIManager.Instance.OpenRestock();
                }
            }
        }

        public bool OnHoldInteract(MiniMarketTycoon.Gameplay.PlayerManagerController player, float deltaTime) => false;
        public void OnFocusEnter(MiniMarketTycoon.Gameplay.PlayerManagerController player) { }
        public void OnFocusExit(MiniMarketTycoon.Gameplay.PlayerManagerController player) { }

        private void OnDisable()
        {
            if (ShelfManager.HasInstance)
            {
                ShelfManager.Instance.UnregisterShelf(this);
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnStockChanged -= HandleStockChanged;
            }

            if (ShelfManager.HasInstance)
            {
                ShelfManager.Instance.UnregisterShelf(this);
            }
        }
    }
}
