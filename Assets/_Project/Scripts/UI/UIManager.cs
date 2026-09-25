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

        public GameHUDView HUD => _gameHUDView;
        public SettingsPopupView SettingsPopup => _settingsPopupView;

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

        public void CloseSettings()
        {
            if (_settingsPopupView != null)
            {
                _settingsPopupView.Hide();
            }
        }
    }
}
