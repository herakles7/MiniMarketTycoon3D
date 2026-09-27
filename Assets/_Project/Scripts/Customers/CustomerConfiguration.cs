using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Configuration ScriptableObject managing customer spawn rates, navigation speeds,
    /// interaction timings, queue capacities, and debug flags without hardcoded values.
    /// </summary>
    [CreateAssetMenu(fileName = "CustomerConfiguration", menuName = "MiniMarket/Customer Configuration")]
    public class CustomerConfiguration : ScriptableObject
    {
        [Header("Spawn Settings")]
        [Tooltip("Initial number of customers to spawn when the scene begins.")]
        [SerializeField] private int _startingCustomers = 3;

        [Tooltip("Maximum concurrent customers allowed in the market.")]
        [SerializeField] private int _maxCustomers = 8;

        [Tooltip("Time interval in seconds between customer spawn checks.")]
        [SerializeField] private float _spawnInterval = 5f;

        [Header("Movement & Navigation")]
        [Tooltip("Minimum realistic walking speed in m/s.")]
        [SerializeField] private float _walkSpeedMin = 1.1f;

        [Tooltip("Maximum realistic walking speed in m/s.")]
        [SerializeField] private float _walkSpeedMax = 1.4f;

        [Header("Shopping & Interaction")]
        [Tooltip("Minimum time spent inspecting and taking items from a shelf.")]
        [SerializeField] private float _shoppingTimeMin = 2.0f;

        [Tooltip("Maximum time spent inspecting and taking items from a shelf.")]
        [SerializeField] private float _shoppingTimeMax = 3.5f;

        [Tooltip("Time spent paying at the cash register.")]
        [SerializeField] private float _checkoutTime = 2.5f;

        [Header("Queue & Patience")]
        [Tooltip("Maximum number of customers that can stand in the checkout queue.")]
        [SerializeField] private int _maxQueueSize = 4;

        [Tooltip("Maximum waiting patience in seconds before customer gives up.")]
        [SerializeField] private float _patience = 60f;

        [Header("Debugging")]
        [Tooltip("When enabled, shows floating 3D world-space text badges above customer heads displaying their current state.")]
        [SerializeField] private bool _debugCustomerState = false;

        public int StartingCustomers => _startingCustomers;
        public int MaxCustomers => _maxCustomers;
        public float SpawnInterval => _spawnInterval;
        public float WalkSpeedMin => _walkSpeedMin;
        public float WalkSpeedMax => _walkSpeedMax;
        public float ShoppingTimeMin => _shoppingTimeMin;
        public float ShoppingTimeMax => _shoppingTimeMax;
        public float CheckoutTime => _checkoutTime;
        public int MaxQueueSize => _maxQueueSize;
        public float Patience => _patience;
        public bool DebugCustomerState => _debugCustomerState;

        public float GetRandomWalkSpeed()
        {
            return Random.Range(_walkSpeedMin, _walkSpeedMax);
        }

        public float GetRandomShoppingDuration()
        {
            return Random.Range(_shoppingTimeMin, _shoppingTimeMax);
        }
    }
}
