using System;
using UnityEngine;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Master gameplay coordinator persisting throughout the entire application lifecycle.
    /// Bridges high-level game states, pause controls, and idle offline earnings.
    /// </summary>
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [Header("State Tracking")]
        [SerializeField] private bool _isPaused = false;

        public bool IsPaused => _isPaused;

        public event Action<bool> OnGamePauseToggled;
        public event Action<double> OnOfflineEarningsProcessed;

        protected override void OnInitialized()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState oldState, GameState newState)
        {
            if (newState == GameState.Playing && oldState != GameState.Paused)
            {
                ProcessOfflineEarnings();
            }
        }

        public void PauseGame()
        {
            if (_isPaused) return;

            _isPaused = true;
            Time.timeScale = 0f;

            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.ChangeState(GameState.Paused);
            }

            OnGamePauseToggled?.Invoke(true);
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;

            _isPaused = false;
            Time.timeScale = 1f;

            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }

            OnGamePauseToggled?.Invoke(false);
        }

        private void ProcessOfflineEarnings()
        {
            if (!SaveManager.HasInstance) return;

            TimeSpan offlineTime = SaveManager.Instance.GetOfflineTimeElapsed();
            if (offlineTime.TotalMinutes < 1.0)
            {
                return; // Less than a minute offline, no calculation needed
            }

            float maxHours = 6.0f;
            float efficiency = 0.5f;

            if (ConfigurationManager.HasInstance && ConfigurationManager.Instance.Config != null &&
                ConfigurationManager.Instance.Config.EconomyConfig != null)
            {
                maxHours = ConfigurationManager.Instance.Config.EconomyConfig.OfflineEarningsMaxHours;
                efficiency = ConfigurationManager.Instance.Config.EconomyConfig.OfflineEarningsEfficiency;
            }

            double cappedSeconds = Math.Min(offlineTime.TotalSeconds, maxHours * 3600);
            
            // Base idle rate calculation: e.g. $1 per second base rate * efficiency
            double earnedCash = cappedSeconds * 0.5 * efficiency;

            if (earnedCash > 0 && CurrencyManager.HasInstance)
            {
                Debug.Log($"[GameManager] Welcome back! Offline duration: {offlineTime.TotalMinutes:F1} min. Offline earnings: ${earnedCash:F0}");
                CurrencyManager.Instance.AddCurrency(earnedCash);
                OnOfflineEarningsProcessed?.Invoke(earnedCash);
            }
        }

        protected override void OnDestroy()
        {
            if (GameStateManager.HasInstance)
            {
                GameStateManager.Instance.OnStateChanged -= HandleStateChanged;
            }
            base.OnDestroy();
        }
    }
}
