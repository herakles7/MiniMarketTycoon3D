using UnityEngine;
using UnityEngine.UI;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Portrait Mobile HUD view for 03_Game scene.
    /// Manages Top Bar (Money, Level, Settings) and Bottom Bar (Shop, Upgrades, Workers, Market).
    /// </summary>
    public class GameHUDView : MonoBehaviour
    {
        [Header("Top Bar Elements")]
        [SerializeField] private Text _cashText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Button _settingsButton;

        [Header("Bottom Bar Elements")]
        [SerializeField] private Button _shopTabButton;
        [SerializeField] private Button _upgradesTabButton;
        [SerializeField] private Button _workersTabButton;
        [SerializeField] private Button _marketTabButton;

        private void Start()
        {
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_shopTabButton != null) _shopTabButton.onClick.AddListener(() => OnTabClicked("Shop"));
            if (_upgradesTabButton != null) _upgradesTabButton.onClick.AddListener(() => OnTabClicked("Upgrades"));
            if (_workersTabButton != null) _workersTabButton.onClick.AddListener(() => OnTabClicked("Workers"));
            if (_marketTabButton != null) _marketTabButton.onClick.AddListener(() => OnTabClicked("Market"));

            if (CurrencyManager.HasInstance)
            {
                CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
                UpdateCashDisplay(CurrencyManager.Instance.Cash);
            }
            else
            {
                UpdateCashDisplay(1000.0);
            }

            SetLevel(1);
        }

        private void OnDestroy()
        {
            if (CurrencyManager.HasInstance)
            {
                CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void HandleCurrencyChanged(double newBalance, double delta)
        {
            UpdateCashDisplay(newBalance);
        }

        public void UpdateCashDisplay(double cash)
        {
            if (_cashText != null)
            {
                _cashText.text = CurrencyFormatter.Format(cash);
            }
        }

        public void SetLevel(int level)
        {
            if (_levelText != null)
            {
                _levelText.text = $"Level {level}";
            }
        }

        private void OnSettingsClicked()
        {
            if (UIManager.HasInstance)
            {
                UIManager.Instance.OpenSettings();
            }
        }

        private void OnTabClicked(string tabName)
        {
            Debug.Log($"[GameHUDView] Tab clicked: {tabName}");
        }
    }
}
