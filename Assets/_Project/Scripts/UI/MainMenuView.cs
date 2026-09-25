using UnityEngine;
using UnityEngine.UI;
using MiniMarketTycoon.Core;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// View controller for the 02_MainMenu scene.
    /// Provides options to start the game or modify settings.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private SettingsPopupView _settingsPopup;

        private void Start()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }
        }

        private void OnPlayClicked()
        {
            Debug.Log("[MainMenuView] Play button clicked. Transitioning to 03_Game.");
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadScene(SceneLoader.SCENE_GAME, GameState.Playing);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneLoader.SCENE_GAME);
            }
        }

        private void OnSettingsClicked()
        {
            if (_settingsPopup != null)
            {
                _settingsPopup.Show();
            }
        }
    }
}
