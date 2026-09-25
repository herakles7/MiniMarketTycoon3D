using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Handles asynchronous scene loading with progress reporting and state synchronization.
    /// </summary>
    public class SceneLoader : MonoBehaviourSingleton<SceneLoader>
    {
        public const string SCENE_BOOT = "01_Boot";
        public const string SCENE_MAIN_MENU = "02_MainMenu";
        public const string SCENE_GAME = "03_Game";

        public event Action<float> OnLoadingProgress;
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        private bool _isLoading = false;
        public bool IsLoading => _isLoading;

        public void LoadScene(string sceneName, GameState targetStateAfterLoad = GameState.Playing)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading a scene. Ignoring request for '{sceneName}'.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName, targetStateAfterLoad));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, GameState targetStateAfterLoad)
        {
            _isLoading = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.ChangeState(GameState.Loading);
            }

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp == null)
            {
                Debug.LogError($"[SceneLoader] Failed to start loading scene: '{sceneName}'. Check Build Settings.");
                _isLoading = false;
                yield break;
            }

            asyncOp.allowSceneActivation = false;

            while (asyncOp.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                OnLoadingProgress?.Invoke(progress);
                yield return null;
            }

            OnLoadingProgress?.Invoke(1.0f);
            yield return new WaitForSeconds(0.2f); // Smooth visual completion

            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            _isLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneName);

            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.ChangeState(targetStateAfterLoad);
            }

            Debug.Log($"[SceneLoader] Scene '{sceneName}' loaded successfully. State set to {targetStateAfterLoad}.");
        }
    }
}
