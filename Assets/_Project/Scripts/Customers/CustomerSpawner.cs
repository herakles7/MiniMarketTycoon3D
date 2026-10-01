using System.Collections;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Coordinates customer spawning according to the CustomerConfiguration rules.
    /// Handles initial customer entry, staggered spawning, and continuous population management.
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CustomerConfiguration _configuration;
        [SerializeField] private CustomerPersonalityConfig _personalityConfig;

        [Header("Dependencies")]
        [SerializeField] private CustomerPool _pool;
        [SerializeField] private CustomerTargetSelector _targetSelector;
        [SerializeField] private CustomerQueueController _queueController;

        private float _spawnTimer;
        private int _variationCounter;
        private bool _isStarted;
        private int _activeMaxCustomers = 5;
        private float _activeSpawnInterval = 8.0f;

        public int ActiveMaxCustomers => _activeMaxCustomers;
        public float ActiveSpawnInterval => _activeSpawnInterval;
        public CustomerPersonalityConfig PersonalityConfig => _personalityConfig;

        private void Awake()
        {
            if (_pool == null) _pool = GetComponent<CustomerPool>() ?? GetComponentInChildren<CustomerPool>() ?? FindFirstObjectByType<CustomerPool>();
            if (_targetSelector == null) _targetSelector = GetComponent<CustomerTargetSelector>() ?? GetComponentInChildren<CustomerTargetSelector>() ?? FindFirstObjectByType<CustomerTargetSelector>();
            if (_queueController == null) _queueController = GetComponent<CustomerQueueController>() ?? GetComponentInChildren<CustomerQueueController>() ?? FindFirstObjectByType<CustomerQueueController>();
            if (_configuration == null)
            {
#if UNITY_EDITOR
                _configuration = UnityEditor.AssetDatabase.LoadAssetAtPath<CustomerConfiguration>("Assets/_Project/ScriptableObjects/Configuration/CustomerConfiguration.asset");
#endif
                if (_configuration == null)
                {
                    _configuration = ScriptableObject.CreateInstance<CustomerConfiguration>();
                }
            }

            if (_personalityConfig == null)
            {
#if UNITY_EDITOR
                _personalityConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CustomerPersonalityConfig>("Assets/_Project/ScriptableObjects/Configuration/CustomerPersonalityConfig.asset");
#endif
                if (_personalityConfig == null)
                {
                    _personalityConfig = ScriptableObject.CreateInstance<CustomerPersonalityConfig>();
                    _personalityConfig.InitializeDefaults();
                }
            }
        }

        private void Start()
        {
            UpdateActiveProgressionValues(1);

            if (MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance)
            {
                MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.OnMarketLevelUpgraded += HandleMarketUpgraded;
            }

            StartCoroutine(SpawnInitialCustomersRoutine());
        }

        private void HandleMarketUpgraded(int newLevel)
        {
            UpdateActiveProgressionValues(newLevel);
            Debug.Log($"[CustomerSpawner] Market upgraded to Lv.{newLevel}. MaxCustomers updated to {_activeMaxCustomers}, SpawnInterval to {_activeSpawnInterval}s");
        }

        public void UpdateActiveProgressionValues(int level)
        {
            if (MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance)
            {
                _activeMaxCustomers = MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.CurrentMaxCustomers;
                _activeSpawnInterval = MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.CurrentSpawnInterval;
            }
            else if (_configuration != null)
            {
                _activeMaxCustomers = _configuration.MaxCustomers;
                _activeSpawnInterval = _configuration.SpawnInterval;
            }
            else
            {
                _activeMaxCustomers = 5;
                _activeSpawnInterval = 8.0f;
            }
        }

        private IEnumerator SpawnInitialCustomersRoutine()
        {
            yield return new WaitForSeconds(0.6f); // Brief delay for NavMesh & environment initialization

            int initialCount = _configuration != null ? _configuration.StartingCustomers : 3;
            initialCount = Mathf.Min(initialCount, _activeMaxCustomers);

            for (int i = 0; i < initialCount; i++)
            {
                SpawnCustomer();
                yield return new WaitForSeconds(1.6f); // Staggered entry
            }

            _isStarted = true;
        }

        private void Update()
        {
            if (!_isStarted || _pool == null) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _activeSpawnInterval)
            {
                _spawnTimer = 0f;

                if (_pool.ActiveCount < _activeMaxCustomers)
                {
                    SpawnCustomer();
                }
            }
        }

        private void OnDestroy()
        {
            if (MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance)
            {
                MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.OnMarketLevelUpgraded -= HandleMarketUpgraded;
            }
        }

        public CustomerController SpawnCustomer()
        {
            if (_pool == null || _targetSelector == null) return null;

            Vector3 spawnPos = _targetSelector.SpawnPoint != null
                ? _targetSelector.SpawnPoint.position
                : new Vector3(0f, 0f, -13f);

            // Add slight jitter so multiple spawns don't overlap exactly
            Vector3 jitter = new Vector3(Random.Range(-0.4f, 0.4f), 0f, Random.Range(-0.3f, 0.3f));
            Vector3 finalSpawnPos = spawnPos + jitter;

            CustomerController customer = _pool.GetCustomer(_configuration, _targetSelector, _queueController);
            if (customer != null)
            {
                // Cycle through all available customer variations (A through L)
                int totalVariations = System.Enum.GetValues(typeof(CustomerVariationType)).Length;
                CustomerVariationType variation = (CustomerVariationType)(_variationCounter % totalVariations);
                _variationCounter++;

                CustomerPersonalityData personalityData = null;
                if (_personalityConfig != null)
                {
                    personalityData = _personalityConfig.CreateRandomRuntimeData();
                }
                else
                {
                    personalityData = new CustomerPersonalityData();
                }

                customer.Spawn(finalSpawnPos, variation, personalityData);
            }

            return customer;
        }
    }
}
