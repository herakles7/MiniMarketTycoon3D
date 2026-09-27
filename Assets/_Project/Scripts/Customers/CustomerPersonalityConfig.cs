using System;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Customers
{
    [Serializable]
    public class PersonalityDefinition
    {
        public CustomerPersonalityType PersonalityType = CustomerPersonalityType.Normal;
        public string DisplayName = "Normal Shopper";
        [TextArea(1, 3)] public string Description = "Standard customer with balanced shopping patterns.";
        [Range(0f, 100f)] public float SpawnWeight = 30f;

        [Header("Movement & Interaction Multipliers")]
        public Vector2 MoveSpeedMultiplierRange = new Vector2(0.95f, 1.05f);
        public float ShelfInteractionMultiplier = 1.0f;
        public float CheckoutPatienceMultiplier = 1.0f;
        public float ShoppingTimeMultiplier = 1.0f;

        [Header("Product Demand & Limits")]
        public int MinProducts = 1;
        public int MaxProducts = 3;
        public int MinQuantityPerProduct = 1;
        public int MaxQuantityPerProduct = 2;

        [Header("Sensitivities & Tolerances")]
        [Range(0f, 1f)] public float PriceSensitivity = 0.2f;
        public int MaxSkippedItemsBeforeLeave = 99;

        [Header("Category Preference")]
        public ProductCategory PreferredCategory = ProductCategory.Beverages;
        public float CategoryAffinity = 1.0f;
    }

    /// <summary>
    /// Configuration ScriptableObject holding definitions and spawn weights for all 6 customer personality types (Aşama 8).
    /// </summary>
    [CreateAssetMenu(fileName = "CustomerPersonalityConfig", menuName = "MiniMarket/Customer Personality Config")]
    public class CustomerPersonalityConfig : ScriptableObject
    {
        [SerializeField] private List<PersonalityDefinition> _personalities = new List<PersonalityDefinition>();

        public IReadOnlyList<PersonalityDefinition> Personalities => _personalities;

        private void Reset()
        {
            InitializeDefaults();
        }

        public void InitializeDefaults()
        {
            if (_personalities == null)
            {
                _personalities = new List<PersonalityDefinition>();
            }
            _personalities.Clear();

            // 1. NORMAL
            _personalities.Add(new PersonalityDefinition
            {
                PersonalityType = CustomerPersonalityType.Normal,
                DisplayName = "Normal Shopper",
                Description = "Balanced shopping time, 1-3 products, normal checkout patience.",
                SpawnWeight = 30f,
                MoveSpeedMultiplierRange = new Vector2(0.98f, 1.04f),
                ShelfInteractionMultiplier = 1.0f,
                CheckoutPatienceMultiplier = 1.0f,
                MinProducts = 1,
                MaxProducts = 3,
                MinQuantityPerProduct = 1,
                MaxQuantityPerProduct = 2,
                PriceSensitivity = 0.2f,
                MaxSkippedItemsBeforeLeave = 99
            });

            // 2. QUICK SHOPPER
            _personalities.Add(new PersonalityDefinition
            {
                PersonalityType = CustomerPersonalityType.QuickShopper,
                DisplayName = "Quick Shopper",
                Description = "Fast walker (1.10-1.20x speed), quick shelf time (0.7x), buys 1-2 items.",
                SpawnWeight = 15f,
                MoveSpeedMultiplierRange = new Vector2(1.12f, 1.20f),
                ShelfInteractionMultiplier = 0.70f,
                CheckoutPatienceMultiplier = 0.80f,
                MinProducts = 1,
                MaxProducts = 2,
                MinQuantityPerProduct = 1,
                MaxQuantityPerProduct = 1,
                PriceSensitivity = 0.2f,
                MaxSkippedItemsBeforeLeave = 99
            });

            // 3. BARGAIN HUNTER
            _personalities.Add(new PersonalityDefinition
            {
                PersonalityType = CustomerPersonalityType.BargainHunter,
                DisplayName = "Bargain Hunter",
                Description = "High price sensitivity, strong bias towards lower-priced retail items.",
                SpawnWeight = 15f,
                MoveSpeedMultiplierRange = new Vector2(0.96f, 1.04f),
                ShelfInteractionMultiplier = 1.0f,
                CheckoutPatienceMultiplier = 1.0f,
                MinProducts = 1,
                MaxProducts = 3,
                MinQuantityPerProduct = 1,
                MaxQuantityPerProduct = 2,
                PriceSensitivity = 0.85f,
                MaxSkippedItemsBeforeLeave = 99
            });

            // 4. BIG SHOPPER
            _personalities.Add(new PersonalityDefinition
            {
                PersonalityType = CustomerPersonalityType.BigShopper,
                DisplayName = "Big Shopper",
                Description = "Buys 3-5 distinct products with quantities up to 3 (max total 8 items).",
                SpawnWeight = 15f,
                MoveSpeedMultiplierRange = new Vector2(0.92f, 1.00f),
                ShelfInteractionMultiplier = 1.0f,
                CheckoutPatienceMultiplier = 1.10f,
                MinProducts = 3,
                MaxProducts = 5,
                MinQuantityPerProduct = 1,
                MaxQuantityPerProduct = 3,
                PriceSensitivity = 0.10f,
                MaxSkippedItemsBeforeLeave = 99
            });

            // 5. PATIENT SHOPPER
            _personalities.Add(new PersonalityDefinition
            {
                PersonalityType = CustomerPersonalityType.PatientShopper,
                DisplayName = "Patient Shopper",
                Description = "High tolerance (1.5x checkout patience), calm demeanor in queues.",
                SpawnWeight = 10f,
                MoveSpeedMultiplierRange = new Vector2(0.92f, 1.00f),
                ShelfInteractionMultiplier = 1.20f,
                CheckoutPatienceMultiplier = 1.50f,
                MinProducts = 1,
                MaxProducts = 3,
                MinQuantityPerProduct = 1,
                MaxQuantityPerProduct = 2,
                PriceSensitivity = 0.20f,
                MaxSkippedItemsBeforeLeave = 99
            });

            // 6. IMPATIENT SHOPPER
            _personalities.Add(new PersonalityDefinition
            {
                PersonalityType = CustomerPersonalityType.ImpatientShopper,
                DisplayName = "Impatient Shopper",
                Description = "Low queue patience (0.6x), leaves if queue is blocked or 2 items missing.",
                SpawnWeight = 15f,
                MoveSpeedMultiplierRange = new Vector2(1.05f, 1.15f),
                ShelfInteractionMultiplier = 0.80f,
                CheckoutPatienceMultiplier = 0.60f,
                MinProducts = 1,
                MaxProducts = 3,
                MinQuantityPerProduct = 1,
                MaxQuantityPerProduct = 2,
                PriceSensitivity = 0.30f,
                MaxSkippedItemsBeforeLeave = 2
            });
        }

        public PersonalityDefinition GetDefinition(CustomerPersonalityType type)
        {
            if (_personalities == null || _personalities.Count == 0)
            {
                InitializeDefaults();
            }

            for (int i = 0; i < _personalities.Count; i++)
            {
                if (_personalities[i] != null && _personalities[i].PersonalityType == type)
                {
                    return _personalities[i];
                }
            }

            return _personalities.Count > 0 ? _personalities[0] : null;
        }

        private static readonly System.Random _sysRandom = new System.Random();

        public CustomerPersonalityType GetRandomPersonalityType()
        {
            if (_personalities == null || _personalities.Count == 0)
            {
                InitializeDefaults();
            }

            float totalWeight = 0f;
            for (int i = 0; i < _personalities.Count; i++)
            {
                if (_personalities[i] != null && _personalities[i].SpawnWeight > 0f)
                {
                    totalWeight += _personalities[i].SpawnWeight;
                }
            }

            if (totalWeight <= 0f)
            {
                return CustomerPersonalityType.Normal;
            }

            float roll = (float)(_sysRandom.NextDouble() * totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < _personalities.Count; i++)
            {
                var def = _personalities[i];
                if (def != null && def.SpawnWeight > 0f)
                {
                    cumulative += def.SpawnWeight;
                    if (roll <= cumulative)
                    {
                        return def.PersonalityType;
                    }
                }
            }

            return CustomerPersonalityType.Normal;
        }

        public CustomerPersonalityData CreateRuntimeData(CustomerPersonalityType type)
        {
            var def = GetDefinition(type);
            if (def == null)
            {
                return new CustomerPersonalityData();
            }

            // Provide slight natural variance within the defined range
            float t = (float)_sysRandom.NextDouble();
            float speedMult = Mathf.Lerp(def.MoveSpeedMultiplierRange.x, def.MoveSpeedMultiplierRange.y, t);
            int basketRange = def.MaxProducts - def.MinProducts + 1;
            int basketSize = def.MinProducts + _sysRandom.Next(0, Mathf.Max(1, basketRange));

            return new CustomerPersonalityData
            {
                PersonalityType = def.PersonalityType,
                MoveSpeedMultiplier = speedMult,
                ShelfTimeMultiplier = def.ShelfInteractionMultiplier,
                CheckoutPatienceMultiplier = def.CheckoutPatienceMultiplier,
                PriceSensitivity = def.PriceSensitivity,
                MinProducts = def.MinProducts,
                MaxProducts = def.MaxProducts,
                MinQuantity = def.MinQuantityPerProduct,
                MaxQuantity = def.MaxQuantityPerProduct,
                BasketSize = basketSize,
                Satisfaction = 100,
                SkippedItemsCount = 0,
                MaxSkippedItemsBeforeLeave = def.MaxSkippedItemsBeforeLeave,
                HasLeftDueToImpatience = false
            };
        }

        public CustomerPersonalityData CreateRandomRuntimeData()
        {
            CustomerPersonalityType type = GetRandomPersonalityType();
            return CreateRuntimeData(type);
        }
    }
}
