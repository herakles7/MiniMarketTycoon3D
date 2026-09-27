using System;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Economy
{
    [Serializable]
    public class ProductSalesStats
    {
        public string ProductId;
        public int ItemsSold;
        public double Revenue;
        public double Profit;

        public ProductSalesStats() { }

        public ProductSalesStats(string productId)
        {
            ProductId = productId;
        }
    }

    /// <summary>
    /// Tracks retail financial statistics, revenue, wholesale restock expenses,
    /// net profit margins, items sold, customer footfall analytics, and per-product runtime sales performance.
    /// </summary>
    public class EconomyManager : MonoBehaviourSingleton<EconomyManager>
    {
        [Header("Runtime Daily Accounting")]
        [SerializeField] private double _todayRevenue;
        [SerializeField] private double _todayExpenses;
        [SerializeField] private int _itemsSold;
        [SerializeField] private int _customersServed;

        [Header("Lifetime Statistics")]
        [SerializeField] private double _lifetimeRevenue;
        [SerializeField] private double _lifetimeProfit;

        [Header("Per-Product Runtime Statistics")]
        private readonly Dictionary<string, ProductSalesStats> _productStats = new Dictionary<string, ProductSalesStats>(16);

        public double TodayRevenue => _todayRevenue;
        public double TodayExpenses => _todayExpenses;
        public double TodayProfit => _todayRevenue - _todayExpenses;
        public int ItemsSold => _itemsSold;
        public int CustomersServed => _customersServed;
        public double LifetimeRevenue => _lifetimeRevenue;
        public double LifetimeProfit => _lifetimeProfit;
        public IReadOnlyDictionary<string, ProductSalesStats> ProductStats => _productStats;

        public event Action<double, double> OnSaleRecorded; // (revenue, netProfit)
        public event Action<double> OnExpenseRecorded;      // (expenseAmount)
        public event Action<string, int, double, double> OnProductSaleRecorded; // (productId, quantity, revenue, profit)

        protected override void OnInitialized()
        {
            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                var econ = SaveManager.Instance.CurrentSave.Economy;
                if (econ != null)
                {
                    _todayRevenue = econ.TodayRevenue;
                    _todayExpenses = econ.TodayExpenses;
                    _itemsSold = econ.ItemsSold;
                    _customersServed = econ.CustomersServed;
                    _lifetimeRevenue = econ.LifetimeRevenue;
                    _lifetimeProfit = econ.LifetimeProfit;
                }

                // Restore per-product stats from save if available
                if (SaveManager.Instance.CurrentSave.Products != null)
                {
                    foreach (var p in SaveManager.Instance.CurrentSave.Products)
                    {
                        if (!string.IsNullOrEmpty(p.ProductId))
                        {
                            var stats = GetProductStats(p.ProductId);
                            stats.ItemsSold = p.ItemsSold;
                            stats.Revenue = p.Revenue;
                            stats.Profit = p.Profit;
                        }
                    }
                }

                SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
            }
        }

        private void HandleBeforeSave(GameSaveData saveData)
        {
            if (saveData == null) return;

            if (saveData.Economy == null)
            {
                saveData.Economy = new EconomySaveData();
            }

            saveData.Economy.TodayRevenue = _todayRevenue;
            saveData.Economy.TodayExpenses = _todayExpenses;
            saveData.Economy.TodayProfit = TodayProfit;
            saveData.Economy.ItemsSold = _itemsSold;
            saveData.Economy.CustomersServed = _customersServed;
            saveData.Economy.LifetimeRevenue = _lifetimeRevenue;
            saveData.Economy.LifetimeProfit = _lifetimeProfit;

            // Sync per-product stats into product save entries
            if (saveData.Products != null)
            {
                foreach (var kvp in _productStats)
                {
                    var entry = saveData.Products.Find(p => p.ProductId == kvp.Key);
                    if (entry != null)
                    {
                        entry.ItemsSold = kvp.Value.ItemsSold;
                        entry.Revenue = kvp.Value.Revenue;
                        entry.Profit = kvp.Value.Profit;
                    }
                }
            }
        }

        public ProductSalesStats GetProductStats(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return null;

            if (!_productStats.TryGetValue(productId, out var stats))
            {
                stats = new ProductSalesStats(productId);
                _productStats[productId] = stats;
            }
            return stats;
        }

        /// <summary>
        /// Records individual product sales metrics for future Best Selling Product tracking.
        /// </summary>
        public void RecordProductSale(string productId, int quantity, double revenue, double profit)
        {
            if (string.IsNullOrEmpty(productId) || quantity <= 0) return;

            var stats = GetProductStats(productId);
            stats.ItemsSold += quantity;
            stats.Revenue += revenue;
            stats.Profit += profit;

            OnProductSaleRecorded?.Invoke(productId, quantity, revenue, profit);
        }

        /// <summary>
        /// Records revenue and profit generated from a customer checkout.
        /// </summary>
        public void RecordSale(double revenue, double purchaseCost, int itemsCount = 1)
        {
            if (revenue <= 0) return;

            double netProfit = revenue - purchaseCost;

            _todayRevenue += revenue;
            _itemsSold += Mathf.Max(1, itemsCount);
            _customersServed++;
            _lifetimeRevenue += revenue;
            _lifetimeProfit += netProfit;

            OnSaleRecorded?.Invoke(revenue, netProfit);
        }

        /// <summary>
        /// Records inventory wholesale restock costs as store operating expenses.
        /// </summary>
        public void RecordExpense(double expense)
        {
            if (expense <= 0) return;

            _todayExpenses += expense;
            OnExpenseRecorded?.Invoke(expense);
        }

        public void ResetDailyStats()
        {
            _todayRevenue = 0.0;
            _todayExpenses = 0.0;
            _itemsSold = 0;
            _customersServed = 0;
        }

        protected override void OnDestroy()
        {
            if (SaveManager.HasInstance)
            {
                SaveManager.Instance.OnBeforeSave -= HandleBeforeSave;
            }
            base.OnDestroy();
        }
    }
}
