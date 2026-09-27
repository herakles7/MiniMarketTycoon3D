using System.Collections;
using UnityEngine;
using MiniMarketTycoon.Audio;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Save;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Initial entry point execution script residing in 01_Boot scene.
    /// Orchestrates orderly bootstrapping of core singletons and starts the scene progression.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Bootstrap Configurations")]
        [SerializeField] private float _minimumSplashDuration = 1.0f;
        [SerializeField] private string _nextScene = SceneLoader.SCENE_MAIN_MENU;

        private void Awake()
        {
            Debug.Log("[GameBootstrap] Initializing Mini Market Tycoon 3D core systems in Awake...");

            // 1. Initialize Configuration
            var configMgr = ConfigurationManager.Instance;

            // 2. Initialize GameStateManager
            var stateMgr = GameStateManager.Instance;
            stateMgr.ChangeState(GameState.Boot);

            // 3. Initialize SaveManager
            var saveMgr = SaveManager.Instance;

            // 4. Initialize QualityManager
            var qualityMgr = QualityManager.Instance;

            // 5. Initialize AudioManager
            var audioMgr = AudioManager.Instance;

            // 6. Initialize CurrencyManager
            var currencyMgr = CurrencyManager.Instance;

            // 7. Initialize GameManager
            var gameMgr = GameManager.Instance;

            // 8. Initialize MarketUpgradeManager
            var upgradeMgr = MiniMarketTycoon.Economy.MarketUpgradeManager.Instance;

            // 9. Initialize SceneLoader
            var sceneLoader = SceneLoader.Instance;
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_minimumSplashDuration);

            Debug.Log($"[GameBootstrap] Core systems ready. Loading '{_nextScene}'...");
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadScene(_nextScene, GameState.MainMenu);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(_nextScene);
            }
        }
    }
}
