using UnityEngine;
using UnityEngine.UI;
using MiniMarketTycoon.Audio;
using MiniMarketTycoon.Core;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Reusable settings popup modal for changing audio volume and mobile quality levels.
    /// </summary>
    public class SettingsPopupView : MonoBehaviour
    {
        [Header("Modal Root")]
        [SerializeField] private GameObject _panelRoot;

        [Header("Audio Sliders")]
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Quality Buttons")]
        [SerializeField] private Button _qualityLowButton;
        [SerializeField] private Button _qualityMediumButton;
        [SerializeField] private Button _qualityHighButton;
        [SerializeField] private Button _qualityUltraButton;

        [Header("Close Button")]
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            if (_panelRoot == null)
            {
                _panelRoot = gameObject;
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(Hide);
            }

            if (_musicSlider != null)
            {
                _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            }

            if (_sfxSlider != null)
            {
                _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            }

            if (_qualityLowButton != null) _qualityLowButton.onClick.AddListener(() => OnQualitySelected(MobileQualityLevel.Low));
            if (_qualityMediumButton != null) _qualityMediumButton.onClick.AddListener(() => OnQualitySelected(MobileQualityLevel.Medium));
            if (_qualityHighButton != null) _qualityHighButton.onClick.AddListener(() => OnQualitySelected(MobileQualityLevel.High));
            if (_qualityUltraButton != null) _qualityUltraButton.onClick.AddListener(() => OnQualitySelected(MobileQualityLevel.Ultra));
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        public void Show()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(true);
            }
            RefreshUI();
        }

        public void Hide()
        {
            if (_panelRoot != null)
            {
                _panelRoot.SetActive(false);
            }
        }

        private void RefreshUI()
        {
            if (AudioManager.HasInstance)
            {
                if (_musicSlider != null) _musicSlider.value = AudioManager.Instance.MusicVolume;
                if (_sfxSlider != null) _sfxSlider.value = AudioManager.Instance.SfxVolume;
            }
        }

        private void OnMusicSliderChanged(float val)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.SetMusicVolume(val);
            }
        }

        private void OnSfxSliderChanged(float val)
        {
            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.SetSfxVolume(val);
            }
        }

        private void OnQualitySelected(MobileQualityLevel level)
        {
            if (QualityManager.HasInstance)
            {
                QualityManager.Instance.SetQualityLevel(level);
            }
        }
    }
}
