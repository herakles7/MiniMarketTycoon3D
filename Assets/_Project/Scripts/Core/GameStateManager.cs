using System;
using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Core
{
    public enum GameState
    {
        Boot,
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver,
        Saving
    }

    /// <summary>
    /// Manages the high-level state machine of the game with validation and transition events.
    /// </summary>
    public class GameStateManager : MonoBehaviourSingleton<GameStateManager>
    {
        [SerializeField] private GameState _currentState = GameState.Boot;

        public GameState CurrentState => _currentState;

        public event Action<GameState, GameState> OnStateChanged;
        public event Action<GameState> OnStateEntered;
        public event Action<GameState> OnStateExited;

        public bool ChangeState(GameState newState)
        {
            if (_currentState == newState)
            {
                return false;
            }

            if (!IsValidTransition(_currentState, newState))
            {
                Debug.LogWarning($"[GameStateManager] Invalid state transition attempted: {_currentState} -> {newState}");
                return false;
            }

            GameState previousState = _currentState;
            OnStateExited?.Invoke(previousState);

            _currentState = newState;
            Debug.Log($"[GameStateManager] State changed: {previousState} -> {newState}");

            OnStateChanged?.Invoke(previousState, newState);
            OnStateEntered?.Invoke(newState);
            return true;
        }

        private bool IsValidTransition(GameState from, GameState to)
        {
            // Allow state transitions logically
            switch (from)
            {
                case GameState.Boot:
                    return to == GameState.MainMenu || to == GameState.Loading;

                case GameState.MainMenu:
                    return to == GameState.Loading || to == GameState.Playing;

                case GameState.Loading:
                    return to == GameState.Playing || to == GameState.MainMenu;

                case GameState.Playing:
                    return to == GameState.Paused || to == GameState.GameOver || to == GameState.Saving || to == GameState.MainMenu || to == GameState.Loading;

                case GameState.Paused:
                    return to == GameState.Playing || to == GameState.MainMenu || to == GameState.Saving;

                case GameState.Saving:
                    return to == GameState.Playing || to == GameState.Paused || to == GameState.MainMenu;

                case GameState.GameOver:
                    return to == GameState.MainMenu || to == GameState.Loading;

                default:
                    return true;
            }
        }
    }
}
