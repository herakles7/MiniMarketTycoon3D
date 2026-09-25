using System;
using UnityEngine;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Core
{
    public enum MobileQualityLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }

    /// <summary>
    /// Scalable graphics and performance manager designed for Android mobile devices.
    /// Manages framerate targets, quality tiers, shadow distance, and texture resolutions.
    /// </summary>
    public class QualityManager : MonoBehaviourSingleton<QualityManager>
    {
        [Header("Active Quality Level")]
        [SerializeField] private MobileQualityLevel _currentQuality = MobileQualityLevel.Medium;

        public MobileQualityLevel CurrentQuality => _currentQuality;

        public event Action<MobileQualityLevel> OnQualityChanged;

        protected override void OnInitialized()
        {
            int targetFps = 60;
            if (ConfigurationManager.HasInstance && ConfigurationManager.Instance.Config != null)
            {
                targetFps = ConfigurationManager.Instance.Config.TargetFrameRate;
            }

            Application.targetFrameRate = targetFps;
            QualitySettings.vSyncCount = 0; // Essential for mobile battery and frame pacing

            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                _currentQuality = (MobileQualityLevel)SaveManager.Instance.CurrentSave.Settings.QualityLevel;
            }

            ApplyQuality(_currentQuality);
        }

        public void SetQualityLevel(MobileQualityLevel level)
        {
            _currentQuality = level;
            ApplyQuality(_currentQuality);

            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.Settings.QualityLevel = (int)_currentQuality;
            }

            OnQualityChanged?.Invoke(_currentQuality);
            Debug.Log($"[QualityManager] Mobile quality set to: {_currentQuality}");
        }

        private void ApplyQuality(MobileQualityLevel level)
        {
            switch (level)
            {
                case MobileQualityLevel.Low:
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowDistance = 20f;
                    QualitySettings.globalTextureMipmapLimit = 1; // Half-res textures for low RAM
                    QualitySettings.antiAliasing = 0;
                    break;

                case MobileQualityLevel.Medium:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowDistance = 35f;
                    QualitySettings.globalTextureMipmapLimit = 0;
                    QualitySettings.antiAliasing = 2;
                    break;

                case MobileQualityLevel.High:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.globalTextureMipmapLimit = 0;
                    QualitySettings.antiAliasing = 4;
                    break;

                case MobileQualityLevel.Ultra:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowDistance = 70f;
                    QualitySettings.globalTextureMipmapLimit = 0;
                    QualitySettings.antiAliasing = 8;
                    break;
            }
        }
    }
}
