using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// High-performance object pool for customer NPCs to prevent runtime GC allocations
    /// and mobile frame drops caused by frequent Instantiate/Destroy calls.
    /// </summary>
    public class CustomerPool : MonoBehaviour
    {
        [Header("Pool Setup")]
        [SerializeField] private GameObject _customerPrefab;
        [SerializeField] private int _initialPoolSize = 10;

        private readonly Queue<CustomerController> _availablePool = new Queue<CustomerController>(12);
        private readonly List<CustomerController> _activeCustomers = new List<CustomerController>(12);

        public int ActiveCount => _activeCustomers.Count;
        public IReadOnlyList<CustomerController> ActiveCustomers => _activeCustomers;

        private void Start()
        {
            if (_availablePool.Count == 0)
            {
                var spawner = FindFirstObjectByType<CustomerSpawner>();
                var targetSelector = FindFirstObjectByType<CustomerTargetSelector>();
                var queueController = FindFirstObjectByType<CustomerQueueController>();
                var config = ScriptableObject.CreateInstance<CustomerConfiguration>();
                Initialize(_customerPrefab, _initialPoolSize, config, targetSelector, queueController);
            }
        }

        public void Initialize(GameObject prefab, int poolSize, CustomerConfiguration config, CustomerTargetSelector targetSelector, CustomerQueueController queueController)
        {
            if (prefab != null) _customerPrefab = prefab;
            _initialPoolSize = poolSize;

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewCustomerInstance(config, targetSelector, queueController);
            }
        }

        private CustomerController CreateNewCustomerInstance(CustomerConfiguration config, CustomerTargetSelector targetSelector, CustomerQueueController queueController)
        {
            if (_customerPrefab == null)
            {
#if UNITY_EDITOR
                _customerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Customers/PF_Customer_NPC.prefab");
#endif
                if (_customerPrefab == null)
                {
                    GameObject fallbackRoot = new GameObject("PF_Customer_NPC_Runtime");
                    fallbackRoot.AddComponent<UnityEngine.AI.NavMeshAgent>();
                    fallbackRoot.AddComponent<CapsuleCollider>();
                    fallbackRoot.AddComponent<CustomerNavigation>();
                    fallbackRoot.AddComponent<CustomerAnimationController>();
                    fallbackRoot.AddComponent<CustomerLookAtController>();
                    fallbackRoot.AddComponent<CustomerVisualController>();
                    fallbackRoot.AddComponent<CustomerVisual>();
                    fallbackRoot.AddComponent<CustomerController>();
                    _customerPrefab = fallbackRoot;
                }
            }

            GameObject obj = Instantiate(_customerPrefab, transform);
            obj.name = $"Customer_NPC_{_availablePool.Count + _activeCustomers.Count + 1}";
            obj.SetActive(false);

            CustomerController controller = obj.GetComponent<CustomerController>();
            if (controller == null)
            {
                controller = obj.AddComponent<CustomerController>();
            }

            controller.InitializeDependencies(config, targetSelector, queueController, this);
            _availablePool.Enqueue(controller);
            return controller;
        }

        public CustomerController GetCustomer(CustomerConfiguration config, CustomerTargetSelector targetSelector, CustomerQueueController queueController)
        {
            CustomerController customer = null;

            if (_availablePool.Count > 0)
            {
                customer = _availablePool.Dequeue();
            }
            else
            {
                // Safety expand if needed
                customer = CreateNewCustomerInstance(config, targetSelector, queueController);
                if (_availablePool.Count > 0)
                {
                    customer = _availablePool.Dequeue();
                }
            }

            if (customer != null)
            {
                _activeCustomers.Add(customer);
            }

            return customer;
        }

        public void ReturnCustomer(CustomerController customer)
        {
            if (customer == null) return;

            _activeCustomers.Remove(customer);
            customer.ResetForPool();
            customer.gameObject.SetActive(false);
            customer.transform.parent = transform;

            if (!_availablePool.Contains(customer))
            {
                _availablePool.Enqueue(customer);
            }
        }
    }
}
