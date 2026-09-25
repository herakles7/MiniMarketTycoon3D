using UnityEngine;
using UnityEngine.UI;
using MiniMarketTycoon.Core;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Displays splash/loading progress during the 01_Boot sequence.
    /// </summary>
    public class BootLoadingView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Text _statusText;

        private void Start()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnLoadingProgress += HandleLoadingProgress;
                SceneLoader.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
            }
        }

        private void OnDestroy()
        {
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.OnLoadingProgress -= HandleLoadingProgress;
                SceneLoader.Instance.OnSceneLoadStarted -= HandleSceneLoadStarted;
            }
        }

        private void HandleLoadingProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_statusText != null)
            {
                _statusText.text = $"Loading Supermarket... {Mathf.RoundToInt(progress * 100)}%";
            }
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            if (_statusText != null)
            {
                _statusText.text = $"Loading {sceneName}...";
            }
        }
    }
}
