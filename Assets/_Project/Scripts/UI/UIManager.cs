using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Central manager for UI screens, popups, and safe area canvas hierarchy.
    /// </summary>
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
        [Header("Root Canvas")]
        [SerializeField] private Canvas _mainCanvas;

        [Header("Registered Views")]
        [SerializeField] private GameHUDView _gameHUDView;
        [SerializeField] private SettingsPopupView _settingsPopupView;
        [SerializeField] private RestockPopupView _restockPopupView;
        [SerializeField] private UpgradePopupView _upgradePopupView;

        public GameHUDView HUD => _gameHUDView;
        public SettingsPopupView SettingsPopup => _settingsPopupView;
        public RestockPopupView RestockPopup => _restockPopupView;
        public UpgradePopupView UpgradePopup => _upgradePopupView;

        protected override void OnInitialized()
        {
            FindReferencesIfMissing();
        }

        public void FindReferencesIfMissing()
        {
            if (_mainCanvas == null)
            {
                _mainCanvas = FindFirstObjectByType<Canvas>();
            }

            if (_gameHUDView == null)
            {
                _gameHUDView = FindFirstObjectByType<GameHUDView>();
            }

            if (_settingsPopupView == null)
            {
                _settingsPopupView = FindFirstObjectByType<SettingsPopupView>();
            }

            if (_restockPopupView == null)
            {
                _restockPopupView = FindFirstObjectByType<RestockPopupView>(FindObjectsInactive.Include);
            }

            if (_upgradePopupView == null)
            {
                _upgradePopupView = FindFirstObjectByType<UpgradePopupView>(FindObjectsInactive.Include);
            }
        }

        public void OpenSettings()
        {
            if (_settingsPopupView == null)
            {
                FindReferencesIfMissing();
            }

            if (_settingsPopupView != null)
            {
                _settingsPopupView.Show();
            }
            else
            {
                Debug.LogWarning("[UIManager] SettingsPopupView reference is missing in scene.");
            }
        }

        public void OpenRestock()
        {
            if (_restockPopupView == null)
            {
                FindReferencesIfMissing();
            }

            if (_restockPopupView == null && _mainCanvas != null)
            {
                _restockPopupView = RestockPopupView.CreateDynamic(_mainCanvas.transform);
            }

            if (_restockPopupView != null)
            {
                _restockPopupView.Show();
            }
            else
            {
                Debug.LogWarning("[UIManager] RestockPopupView reference could not be created or found.");
            }
        }

        public void OpenUpgrades()
        {
            if (_upgradePopupView == null)
            {
                FindReferencesIfMissing();
            }

            if (_upgradePopupView == null && _mainCanvas != null)
            {
                _upgradePopupView = UpgradePopupView.CreateDynamic(_mainCanvas.transform);
            }

            if (_upgradePopupView != null)
            {
                _upgradePopupView.Show();
            }
            else
            {
                Debug.LogWarning("[UIManager] UpgradePopupView reference could not be created or found.");
            }
        }

        public void CloseSettings()
        {
            if (_settingsPopupView != null)
            {
                _settingsPopupView.Hide();
            }
        }

        public void CloseUpgrades()
        {
            if (_upgradePopupView != null)
            {
                _upgradePopupView.Hide();
            }
        }
    }
}
