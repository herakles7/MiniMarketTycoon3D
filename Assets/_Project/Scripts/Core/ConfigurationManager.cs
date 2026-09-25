using UnityEngine;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Central manager for accessing game configuration.
    /// Ensures all systems query a single verified configuration instance without hard-coding.
    /// </summary>
    public class ConfigurationManager : MonoBehaviourSingleton<ConfigurationManager>
    {
        [SerializeField] private GameConfiguration _activeConfiguration;

        public GameConfiguration Config => _activeConfiguration;

        protected override void OnInitialized()
        {
            if (_activeConfiguration == null)
            {
                _activeConfiguration = Resources.Load<GameConfiguration>("GameConfiguration");
                if (_activeConfiguration == null)
                {
                    Debug.LogWarning("[ConfigurationManager] No GameConfiguration found in Resources. Creating runtime fallback configuration.");
                    _activeConfiguration = ScriptableObject.CreateInstance<GameConfiguration>();
                }
            }
        }

        public void SetConfiguration(GameConfiguration config)
        {
            if (config != null)
            {
                _activeConfiguration = config;
            }
        }
    }
}
