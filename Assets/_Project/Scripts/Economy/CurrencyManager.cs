using System;
using UnityEngine;
using MiniMarketTycoon.Core;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Economy
{
    /// <summary>
    /// Manages player cash, transactions, validation, and balance notifications.
    /// Double precision is used to allow high idle tycoon number ranges without floating point degradation.
    /// </summary>
    public class CurrencyManager : MonoBehaviourSingleton<CurrencyManager>
    {
        [Header("Runtime State")]
        [SerializeField] private double _cash = 1000.0;

        public double Cash => _cash;

        /// <summary>
        /// Event fired whenever cash balance changes: (newBalance, deltaAmount)
        /// </summary>
        public event Action<double, double> OnCurrencyChanged;

        protected override void OnInitialized()
        {
            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                _cash = SaveManager.Instance.CurrentSave.Currency.Cash;
                SaveManager.Instance.OnBeforeSave += HandleBeforeSave;
            }
            else
            {
                // Fallback to configuration if save not initialized yet
                if (ConfigurationManager.HasInstance && ConfigurationManager.Instance.Config != null &&
                    ConfigurationManager.Instance.Config.EconomyConfig != null)
                {
                    _cash = ConfigurationManager.Instance.Config.EconomyConfig.StartingCash;
                }
            }

            OnCurrencyChanged?.Invoke(_cash, 0.0);
        }

        private void HandleBeforeSave(GameSaveData saveData)
        {
            if (saveData != null && saveData.Currency != null)
            {
                saveData.Currency.Cash = _cash;
            }
        }

        public void AddCurrency(double amount)
        {
            if (amount <= 0) return;

            _cash += amount;
            OnCurrencyChanged?.Invoke(_cash, amount);
        }

        public bool RemoveCurrency(double amount)
        {
            if (amount <= 0) return true;

            if (!CanAfford(amount))
            {
                return false;
            }

            _cash -= amount;
            OnCurrencyChanged?.Invoke(_cash, -amount);
            return true;
        }

        public bool CanAfford(double amount)
        {
            return _cash >= amount;
        }

        public double GetBalance()
        {
            return _cash;
        }

        public void SetBalance(double newBalance)
        {
            double oldBalance = _cash;
            _cash = Math.Max(0, newBalance);
            OnCurrencyChanged?.Invoke(_cash, _cash - oldBalance);
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
