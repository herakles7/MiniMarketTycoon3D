using System.Collections.Generic;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Manages the cashier checkout queue slots, FIFO ordering, customer progression,
    /// and spatial alignment in front of the cash register counter.
    /// </summary>
    public class CustomerQueueController : MonoBehaviour
    {
        [Header("Queue Layout")]
        [Tooltip("The cash register interaction spot where the front customer pays.")]
        [SerializeField] private Vector3 _frontSlotPosition = new Vector3(-2.0f, 0f, -5.6f);

        [Tooltip("The queue line direction going backwards into the store aisle.")]
        [SerializeField] private Vector3 _queueDirection = new Vector3(0f, 0f, 1f);

        [Tooltip("Distance in meters between queuing customers.")]
        [SerializeField] private float _slotSpacing = 1.3f;

        [Tooltip("Maximum capacity of the checkout queue.")]
        [SerializeField] private int _maxQueueCapacity = 4;

        [Tooltip("Direction the customer should face while standing in the queue (towards cashier).")]
        [SerializeField] private Vector3 _facingDirection = new Vector3(0f, 0f, -1f);

        private readonly List<CustomerController> _queuedCustomers = new List<CustomerController>(4);

        public int MaxCapacity => _maxQueueCapacity;
        public int QueuedCount => _queuedCustomers.Count;
        public bool IsFull => _queuedCustomers.Count >= _maxQueueCapacity;
        public CustomerController CurrentCustomer => _queuedCustomers.Count > 0 ? _queuedCustomers[0] : null;

        public void Initialize(Vector3 frontSlot, Vector3 lineDir, Vector3 faceDir, int capacity = 4, float spacing = 1.3f)
        {
            _frontSlotPosition = frontSlot;
            _queueDirection = lineDir.normalized;
            _facingDirection = faceDir.normalized;
            _maxQueueCapacity = capacity;
            _slotSpacing = spacing;
        }

        public bool TryJoinQueue(CustomerController customer, out int slotIndex, out Vector3 targetPosition)
        {
            if (IsFull || _queuedCustomers.Contains(customer))
            {
                slotIndex = -1;
                targetPosition = Vector3.zero;
                return false;
            }

            slotIndex = _queuedCustomers.Count;
            _queuedCustomers.Add(customer);
            targetPosition = GetSlotPosition(slotIndex);
            return true;
        }

        public void RemoveFromQueue(CustomerController customer)
        {
            int index = _queuedCustomers.IndexOf(customer);
            if (index >= 0)
            {
                _queuedCustomers.RemoveAt(index);
                AdvanceQueuePositions();
            }
        }

        public void AdvanceQueuePositions()
        {
            for (int i = 0; i < _queuedCustomers.Count; i++)
            {
                var customer = _queuedCustomers[i];
                if (customer != null)
                {
                    customer.OnQueuePositionUpdated(i, GetSlotPosition(i), _facingDirection);
                }
            }
        }

        public Vector3 GetSlotPosition(int slotIndex)
        {
            return _frontSlotPosition + (_queueDirection * (slotIndex * _slotSpacing));
        }

        public Vector3 GetFacingDirection()
        {
            return _facingDirection;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            for (int i = 0; i < _maxQueueCapacity; i++)
            {
                Vector3 pos = GetSlotPosition(i);
                Gizmos.color = (i < _queuedCustomers.Count) ? Color.red : Color.yellow;
                Gizmos.DrawWireSphere(pos, 0.3f);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(pos, _facingDirection * 0.6f);
            }
        }
#endif
    }
}
