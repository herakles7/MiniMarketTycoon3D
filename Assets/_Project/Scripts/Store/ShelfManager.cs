using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Store
{
    /// <summary>
    /// Central manager for all store shelving units and refrigerators.
    /// Manages shelf registrations, product-to-shelf spatial lookups for customer pathfinding,
    /// and category organization.
    /// </summary>
    public class ShelfManager : MonoBehaviourSingleton<ShelfManager>
    {
        [Header("Registered Shelves")]
        [SerializeField] private List<ShelfVisualController> _shelves = new List<ShelfVisualController>();

        public IReadOnlyList<ShelfVisualController> Shelves => _shelves;

        protected override void OnInitialized()
        {
            if (_shelves.Count == 0)
            {
                DiscoverShelves();
            }
        }

        public void DiscoverShelves()
        {
            _shelves.Clear();
            var found = FindObjectsByType<ShelfVisualController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (found != null && found.Length > 0)
            {
                _shelves.AddRange(found);
            }
        }

        public void RegisterShelf(ShelfVisualController shelf)
        {
            if (shelf != null && !_shelves.Contains(shelf))
            {
                _shelves.Add(shelf);
            }
        }

        public void UnregisterShelf(ShelfVisualController shelf)
        {
            if (shelf != null)
            {
                _shelves.Remove(shelf);
            }
        }

        /// <summary>
        /// Finds the most suitable shelf carrying the specified product ID.
        /// Prioritizes active shelves that have stock > 0 and unoccupied customer interaction points.
        /// Safely handles duplicate shelves across expansion areas.
        /// </summary>
        public ShelfVisualController FindShelfForProduct(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            if (_shelves.Count == 0)
            {
                DiscoverShelves();
            }

            ShelfVisualController firstMatching = null;
            ShelfVisualController bestStockedShelf = null;

            for (int i = 0; i < _shelves.Count; i++)
            {
                var s = _shelves[i];
                if (s == null || !s.gameObject.activeInHierarchy || !s.enabled) continue;

                if (string.Equals(s.ProductId, productId, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (firstMatching == null)
                    {
                        firstMatching = s;
                    }

                    bool hasStock = s.CurrentStock > 0;
                    var pt = s.GetAvailableInteractionPoint();
                    bool spotAvailable = pt != null && !pt.IsOccupied;

                    if (hasStock)
                    {
                        if (bestStockedShelf == null)
                        {
                            bestStockedShelf = s;
                        }

                        if (spotAvailable)
                        {
                            return s; // Ideal choice: has stock and unoccupied customer spot
                        }
                    }
                    else if (spotAvailable && bestStockedShelf == null)
                    {
                        firstMatching = s;
                    }
                }
            }

            return bestStockedShelf ?? firstMatching;
        }

        public ShelfVisualController FindShelfForProduct(ProductData product)
        {
            if (product == null) return null;
            return FindShelfForProduct(product.ID);
        }

        /// <summary>
        /// Returns all shelves dedicated to products within the specified category.
        /// </summary>
        public List<ShelfVisualController> GetShelvesForCategory(ProductCategory category)
        {
            List<ShelfVisualController> matching = new List<ShelfVisualController>();
            for (int i = 0; i < _shelves.Count; i++)
            {
                var s = _shelves[i];
                if (s != null && s.ProductData != null && s.ProductData.Category == category)
                {
                    matching.Add(s);
                }
            }
            return matching;
        }
    }
}
