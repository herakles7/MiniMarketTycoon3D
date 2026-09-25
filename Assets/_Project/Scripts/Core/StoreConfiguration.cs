using UnityEngine;

namespace MiniMarketTycoon.Core
{
    /// <summary>
    /// Configuration for store parameters, capacity, and limits.
    /// </summary>
    [CreateAssetMenu(fileName = "StoreConfiguration", menuName = "MiniMarket/Configuration/Store Configuration")]
    public class StoreConfiguration : ScriptableObject
    {
        [Header("Store Identity")]
        [SerializeField] private string _defaultStoreName = "My Mini Market";

        [Header("Capacities")]
        [SerializeField] private int _baseStoreCapacity = 10;
        [SerializeField] private int _maxShelves = 20;

        [Header("Customer Flow")]
        [SerializeField] private float _customerSpawnInterval = 4.0f;
        [SerializeField] private float _maxQueueWaitTime = 15.0f;

        public string DefaultStoreName => _defaultStoreName;
        public int BaseStoreCapacity => _baseStoreCapacity;
        public int MaxShelves => _maxShelves;
        public float CustomerSpawnInterval => _customerSpawnInterval;
        public float MaxQueueWaitTime => _maxQueueWaitTime;
    }
}
