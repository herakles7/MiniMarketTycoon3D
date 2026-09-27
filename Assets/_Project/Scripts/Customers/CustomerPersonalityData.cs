using System;
using UnityEngine;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Holds active runtime customer personality parameters, satisfaction, and tolerances.
    /// Dedicated per-customer instance to ensure ScriptableObjects are never mutated during gameplay.
    /// </summary>
    [Serializable]
    public class CustomerPersonalityData
    {
        [Header("Personality Identity")]
        public CustomerPersonalityType PersonalityType = CustomerPersonalityType.Normal;

        [Header("Movement & Interaction Multipliers")]
        public float MoveSpeedMultiplier = 1.0f;
        public float ShelfTimeMultiplier = 1.0f;
        public float CheckoutPatienceMultiplier = 1.0f;

        [Header("Product & Cart Preferences")]
        public float PriceSensitivity = 0.2f;
        public int MinProducts = 1;
        public int MaxProducts = 3;
        public int MinQuantity = 1;
        public int MaxQuantity = 2;
        public int BasketSize = 2;

        [Header("Runtime State & Satisfaction")]
        public int Satisfaction = 100;
        public int SkippedItemsCount = 0;
        public int MaxSkippedItemsBeforeLeave = 99;
        public bool HasLeftDueToImpatience = false;
        public float ShoppingTimer = 0f;
        public float CheckoutQueueTimer = 0f;

        public event Action<int> OnSatisfactionChanged;

        public void AddSatisfaction(int delta)
        {
            if (delta <= 0) return;
            Satisfaction = Mathf.Clamp(Satisfaction + delta, 0, 100);
            OnSatisfactionChanged?.Invoke(Satisfaction);
        }

        public void DeductSatisfaction(int delta, string reason = null)
        {
            if (delta <= 0) return;
            Satisfaction = Mathf.Clamp(Satisfaction - delta, 0, 100);
            OnSatisfactionChanged?.Invoke(Satisfaction);
        }

        public void RecordItemSkipped()
        {
            SkippedItemsCount++;
            DeductSatisfaction(15, "ItemOutOfStock");

            if (SkippedItemsCount >= MaxSkippedItemsBeforeLeave)
            {
                HasLeftDueToImpatience = true;
            }
        }

        /// <summary>
        /// Thoroughly resets all runtime state when returned to the object pool.
        /// Prevents state leakage between consecutive customer spawns.
        /// </summary>
        public void Reset()
        {
            PersonalityType = CustomerPersonalityType.Normal;
            MoveSpeedMultiplier = 1.0f;
            ShelfTimeMultiplier = 1.0f;
            CheckoutPatienceMultiplier = 1.0f;
            PriceSensitivity = 0.2f;
            MinProducts = 1;
            MaxProducts = 3;
            MinQuantity = 1;
            MaxQuantity = 2;
            BasketSize = 2;

            Satisfaction = 100;
            SkippedItemsCount = 0;
            MaxSkippedItemsBeforeLeave = 99;
            HasLeftDueToImpatience = false;
            ShoppingTimer = 0f;
            CheckoutQueueTimer = 0f;
        }
    }
}
