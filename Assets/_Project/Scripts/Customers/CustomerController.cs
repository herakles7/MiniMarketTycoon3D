using System;
using System.Collections.Generic;
using UnityEngine;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Customers
{
    /// <summary>
    /// Master controller and state machine driving individual retail customer behavior.
    /// Manages the full customer lifecycle: Spawn -> Enter -> Browse -> Shelves -> Shopping ->
    /// Queue -> Checkout -> Exit -> Pool Release.
    /// </summary>
    [RequireComponent(typeof(CustomerNavigation))]
    [RequireComponent(typeof(CustomerAnimationController))]
    [RequireComponent(typeof(CustomerVisual))]
    public class CustomerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CustomerNavigation _navigation;
        [SerializeField] private CustomerAnimationController _animationController;
        [SerializeField] private CustomerVisual _visual;
        [SerializeField] private CustomerVisualController _visualController;
        [SerializeField] private CustomerLookAtController _lookAtController;

        private CustomerConfiguration _config;
        private CustomerTargetSelector _targetSelector;
        private CustomerQueueController _queueController;
        private CustomerPool _pool;

        private CustomerState _currentState = CustomerState.Idle;
        private readonly CustomerShoppingData _shoppingData = new CustomerShoppingData();
        private readonly CustomerPersonalityData _personalityData = new CustomerPersonalityData();

        private float _stateTimer;
        private float _actionDuration;
        private bool _isInitialized;

        public CustomerState CurrentState => _currentState;
        public CustomerShoppingData ShoppingData => _shoppingData;
        public CustomerPersonalityData PersonalityData => _personalityData;
        public CustomerVisualController VisualController => _visualController;

        public event Action<CustomerController, CustomerState> OnStateChanged;
        public event Action<CustomerController, CustomerPersonalityType> OnCustomerPersonalityAssigned;
        public event Action<CustomerController, int> OnCustomerSatisfactionChanged;
        public event Action<CustomerController> OnCustomerLeftDueToImpatience;

        private void Awake()
        {
            InitializeComponents();
        }

        public void InitializeComponents()
        {
            if (_navigation == null) _navigation = GetComponent<CustomerNavigation>();
            if (_animationController == null) _animationController = GetComponent<CustomerAnimationController>();
            if (_visual == null) _visual = GetComponent<CustomerVisual>();
            if (_visualController == null) _visualController = GetComponent<CustomerVisualController>();
            if (_lookAtController == null) _lookAtController = GetComponent<CustomerLookAtController>();
        }

        public void InitializeDependencies(
            CustomerConfiguration config,
            CustomerTargetSelector targetSelector,
            CustomerQueueController queueController,
            CustomerPool pool)
        {
            _config = config;
            _targetSelector = targetSelector;
            _queueController = queueController;
            _pool = pool;
            _isInitialized = true;
        }

        /// <summary>
        /// Activates and configures customer upon retrieval from the object pool with specific personality data.
        /// </summary>
        public void Spawn(Vector3 spawnPosition, CustomerVariationType variation, CustomerPersonalityData personalityData = null)
        {
            gameObject.SetActive(true);

            // Setup or copy personality data
            if (personalityData != null)
            {
                _personalityData.PersonalityType = personalityData.PersonalityType;
                _personalityData.MoveSpeedMultiplier = personalityData.MoveSpeedMultiplier;
                _personalityData.ShelfTimeMultiplier = personalityData.ShelfTimeMultiplier;
                _personalityData.CheckoutPatienceMultiplier = personalityData.CheckoutPatienceMultiplier;
                _personalityData.PriceSensitivity = personalityData.PriceSensitivity;
                _personalityData.MinProducts = personalityData.MinProducts;
                _personalityData.MaxProducts = personalityData.MaxProducts;
                _personalityData.MinQuantity = personalityData.MinQuantity;
                _personalityData.MaxQuantity = personalityData.MaxQuantity;
                _personalityData.BasketSize = personalityData.BasketSize;
                _personalityData.Satisfaction = personalityData.Satisfaction;
                _personalityData.SkippedItemsCount = 0;
                _personalityData.MaxSkippedItemsBeforeLeave = personalityData.MaxSkippedItemsBeforeLeave;
                _personalityData.HasLeftDueToImpatience = false;
                _personalityData.ShoppingTimer = 0f;
                _personalityData.CheckoutQueueTimer = 0f;
            }
            else
            {
                _personalityData.Reset();
            }

            _personalityData.OnSatisfactionChanged += (sat) => OnCustomerSatisfactionChanged?.Invoke(this, sat);
            OnCustomerPersonalityAssigned?.Invoke(this, _personalityData.PersonalityType);
            _animationController.SetPersonality(_personalityData.PersonalityType);

            // Configure speed & visual variation with personality speed multiplier
            float baseSpeed = _config != null ? _config.GetRandomWalkSpeed() : 1.25f;
            _navigation.SetSpeed(baseSpeed * _personalityData.MoveSpeedMultiplier);
            _navigation.WarpTo(spawnPosition);

            if (_visual != null)
            {
                _visual.ApplyVariation(variation);
            }

            if (_visualController != null)
            {
                var profile = CustomerAppearanceRandomizer.GenerateProfile(null, _personalityData.PersonalityType);
                _visualController.ApplyProfile(profile);
                if (_animationController != null)
                {
                    _animationController.SetGender(profile.Gender);
                    _animationController.SetHeightMultiplier(profile.HeightMultiplier);
                }
            }
            _visual.SetBasketVisible(false);

            // Populate shopping wishlist based on personality parameters
            int itemCount = UnityEngine.Random.Range(_personalityData.MinProducts, _personalityData.MaxProducts + 1);
            itemCount = Mathf.Clamp(itemCount, 1, 5);

            float basePatience = _config != null ? _config.Patience : 60f;
            float patience = basePatience * _personalityData.CheckoutPatienceMultiplier;
            _shoppingData.Initialize(itemCount, patience);
            _shoppingData.Preference = new CustomerPreference
            {
                PersonalityType = _personalityData.PersonalityType,
                PriceSensitivity = _personalityData.PriceSensitivity,
                ShoppingSpeed = _personalityData.MoveSpeedMultiplier,
                Patience = _personalityData.CheckoutPatienceMultiplier,
                BasketSize = _personalityData.BasketSize
            };

            List<ProductData> candidates = new List<ProductData>();
            if (InventoryManager.HasInstance && InventoryManager.Instance.ProductCatalog != null && InventoryManager.Instance.ProductCatalog.Count > 0)
            {
                // Prefer stocked products if available
                foreach (var p in InventoryManager.Instance.ProductCatalog)
                {
                    if (p != null && InventoryManager.Instance.HasStock(p.ID))
                    {
                        candidates.Add(p);
                    }
                }

                if (candidates.Count == 0)
                {
                    foreach (var p in InventoryManager.Instance.ProductCatalog)
                    {
                        if (p != null) candidates.Add(p);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                List<ProductData> pool = new List<ProductData>(candidates);
                int totalQuantity = 0;

                // Price range for Bargain Hunter weighted selection
                double minPrice = double.MaxValue;
                double maxPrice = 0.0;
                for (int pIdx = 0; pIdx < pool.Count; pIdx++)
                {
                    if (pool[pIdx].SellPrice < minPrice) minPrice = pool[pIdx].SellPrice;
                    if (pool[pIdx].SellPrice > maxPrice) maxPrice = pool[pIdx].SellPrice;
                }
                double priceRange = maxPrice - minPrice;

                for (int i = 0; i < itemCount; i++)
                {
                    if (pool.Count == 0) break;

                    int chosenIdx = 0;
                    if (_personalityData.PriceSensitivity > 0.05f && priceRange > 0.001)
                    {
                        float totalWeight = 0f;
                        float[] weights = new float[pool.Count];
                        for (int pIdx = 0; pIdx < pool.Count; pIdx++)
                        {
                            double norm = (pool[pIdx].SellPrice - minPrice) / priceRange;
                            // Cheaper items get higher weight
                            float weight = 1.0f + (float)((1.0 - norm) * _personalityData.PriceSensitivity * 4.0);
                            weights[pIdx] = weight;
                            totalWeight += weight;
                        }

                        float roll = UnityEngine.Random.Range(0f, totalWeight);
                        float accum = 0f;
                        for (int pIdx = 0; pIdx < pool.Count; pIdx++)
                        {
                            accum += weights[pIdx];
                            if (roll <= accum)
                            {
                                chosenIdx = pIdx;
                                break;
                            }
                        }
                    }
                    else
                    {
                        chosenIdx = UnityEngine.Random.Range(0, pool.Count);
                    }

                    var chosen = pool[chosenIdx];
                    int qty = UnityEngine.Random.Range(_personalityData.MinQuantity, _personalityData.MaxQuantity + 1);

                    // Big Shopper max total quantity constraint: max 8 items
                    if (_personalityData.PersonalityType == CustomerPersonalityType.BigShopper)
                    {
                        if (totalQuantity + qty > 8)
                        {
                            qty = Mathf.Max(1, 8 - totalQuantity);
                        }
                    }
                    totalQuantity += qty;

                    _shoppingData.AddShoppingItem(chosen, qty);
                    if (pool.Count > 1)
                    {
                        pool.RemoveAt(chosenIdx);
                    }

                    if (_personalityData.PersonalityType == CustomerPersonalityType.BigShopper && totalQuantity >= 8)
                    {
                        break;
                    }
                }
            }
            else
            {
                string[] fallbackIds = new string[] { "prod_water", "prod_milk", "prod_chips", "prod_chocolate", "prod_soda", "prod_canned" };
                for (int i = 0; i < itemCount; i++)
                {
                    _shoppingData.AddWishlistItem(fallbackIds[UnityEngine.Random.Range(0, fallbackIds.Length)]);
                }
            }

            ChangeState(CustomerState.Entering);
        }

        public void ChangeState(CustomerState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            _stateTimer = 0f;

            if (_animationController == null || _visual == null || _navigation == null)
            {
                InitializeComponents();
            }

            if (_animationController != null) _animationController.SetState(newState);
            if (_visual != null) _visual.SetDebugState(newState, _config != null && _config.DebugCustomerState);
            OnStateChanged?.Invoke(this, newState);

            OnEnterState(newState);
        }

        private void OnEnterState(CustomerState state)
        {
            switch (state)
            {
                case CustomerState.Entering:
                    bool hasBasket = _visualController != null && _visualController.ActiveProfile != null
                        ? _visualController.ActiveProfile.HasShoppingBasket
                        : false;
                    _visual.SetBasketVisible(hasBasket);
                    Vector3 enterTarget = _targetSelector != null && _targetSelector.EntrancePoint != null
                        ? _targetSelector.EntrancePoint.position
                        : new Vector3(0f, 0f, -9.5f);

                    _navigation.MoveTo(enterTarget, onReached: () => ChangeState(CustomerState.Browsing), onStuck: HandleStuckState);
                    break;

                case CustomerState.Browsing:
                    Vector3 browseTarget = _targetSelector != null && _targetSelector.BrowsingPoint != null
                        ? _targetSelector.BrowsingPoint.position
                        : new Vector3(0f, 0f, -5.5f);

                    _navigation.MoveTo(browseTarget, onReached: () =>
                    {
                        // Proceed to first shelf item
                        SelectNextShelfTarget();
                    }, onStuck: HandleStuckState);
                    break;

                case CustomerState.GoingToShelf:
                    if (_shoppingData.CurrentTargetPoint != null)
                    {
                        Vector3 shelfStandPos = _shoppingData.CurrentTargetPoint.Position;
                        if (_lookAtController != null)
                        {
                            _lookAtController.SetTarget(shelfStandPos + Vector3.up * 0.9f);
                        }
                        _navigation.MoveTo(shelfStandPos, onReached: () =>
                        {
                            ChangeState(CustomerState.Shopping);
                        }, onStuck: () =>
                        {
                            // If path to shelf is blocked, pick another shelf
                            SelectNextShelfTarget();
                        });
                    }
                    break;

                case CustomerState.Shopping:
                    _navigation.Stop();
                    if (_shoppingData.CurrentTargetPoint != null)
                    {
                        _navigation.AlignFacing(_shoppingData.CurrentTargetPoint.FacingDirection);
                        if (_lookAtController != null)
                        {
                            _lookAtController.SetTarget(_shoppingData.CurrentTargetPoint.Position + Vector3.up * 0.9f);
                        }
                    }

                    // Stock check: if 0, do not wait at shelf, skip immediately
                    string shopPid = _shoppingData.GetCurrentItem();
                    if (InventoryManager.HasInstance && !string.IsNullOrEmpty(shopPid) && !InventoryManager.Instance.HasStock(shopPid))
                    {
                        _personalityData.RecordItemSkipped();
                        _shoppingData.SkipCurrentItem();

                        if (_personalityData.HasLeftDueToImpatience || _personalityData.Satisfaction < 40)
                        {
                            if (FloatingFeedbackManager.HasInstance)
                            {
                                FloatingFeedbackManager.Instance.ShowMoodFeedback(transform.position, "😤 Missing item!", new Color(1f, 0.35f, 0.35f, 1f));
                            }
                            OnCustomerLeftDueToImpatience?.Invoke(this);
                            ChangeState(CustomerState.Leaving);
                            return;
                        }

                        if (_shoppingData.HasPendingItems())
                        {
                            SelectNextShelfTarget();
                        }
                        else
                        {
                            ChangeState(_shoppingData.ItemsCollected > 0 ? CustomerState.GoingToCheckout : CustomerState.Leaving);
                        }
                        return;
                    }

                    float baseDuration = _config != null ? _config.GetRandomShoppingDuration() : 2.5f;
                    _actionDuration = baseDuration * _personalityData.ShelfTimeMultiplier;
                    break;

                case CustomerState.GoingToCheckout:
                    TryEnterCheckoutQueue();
                    break;

                case CustomerState.WaitingInQueue:
                    // Waiting for queue to advance
                    _navigation.Stop();
                    if (_queueController != null)
                    {
                        _navigation.AlignFacing(_queueController.GetFacingDirection());
                    }
                    if (_lookAtController != null)
                    {
                        Vector3 qLook = transform.position + transform.forward * 3f + Vector3.up * 1f;
                        _lookAtController.SetTarget(qLook);
                    }
                    break;

                case CustomerState.CheckingOut:
                    _navigation.Stop();
                    _visual.SetBasketVisible(false); // Basket placed on counter
                    if (_lookAtController != null)
                    {
                        Vector3 checkoutLook = transform.position + transform.forward * 2f + Vector3.up * 1f;
                        _lookAtController.SetTarget(checkoutLook);
                    }
                    _actionDuration = MiniMarketTycoon.Economy.MarketUpgradeManager.HasInstance
                        ? MiniMarketTycoon.Economy.MarketUpgradeManager.Instance.CurrentCheckoutTime
                        : (_config != null ? _config.CheckoutTime : 2.5f);
                    break;

                case CustomerState.Leaving:
                    if (_lookAtController != null)
                    {
                        _lookAtController.ResetLookAt();
                    }
                    if (_queueController != null)
                    {
                        _queueController.RemoveFromQueue(this);
                    }

                    Vector3 exitTarget = _targetSelector != null && _targetSelector.ExitPoint != null
                        ? _targetSelector.ExitPoint.position
                        : new Vector3(-2f, 0f, -14f);

                    if (_navigation != null)
                    {
                        _navigation.MoveTo(exitTarget, onReached: () =>
                        {
                            ChangeState(CustomerState.Exited);
                        }, onStuck: () =>
                        {
                            // Force release if stuck at exit
                            ChangeState(CustomerState.Exited);
                        });
                    }
                    break;

                case CustomerState.Exited:
                    if (_navigation != null) _navigation.Stop();
                    ResetForPool();
                    if (_pool != null)
                    {
                        _pool.ReturnCustomer(this);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                    break;

                case CustomerState.Stuck:
                    HandleStuckState();
                    break;
            }
        }

        private void Update()
        {
            if (!_isInitialized || _currentState == CustomerState.Idle || _currentState == CustomerState.Exited)
            {
                return;
            }

            // Sync animation movement speed
            _animationController.UpdateMovement(_navigation.CurrentSpeed);

            float dt = Time.deltaTime;
            _stateTimer += dt;
            _personalityData.ShoppingTimer += dt;
            _shoppingData.ConsumePatience(dt);

            // Handle timed states
            switch (_currentState)
            {
                case CustomerState.Shopping:
                    if (_stateTimer >= _actionDuration)
                    {
                        var shoppingItem = _shoppingData.GetCurrentShoppingItem();
                        if (shoppingItem != null && InventoryManager.HasInstance)
                        {
                            string currentPid = shoppingItem.ProductId;
                            bool priceAccepted = true;

                            // Check shelf retail price vs market baseline
                            if (ShelfManager.HasInstance)
                            {
                                var shelf = ShelfManager.Instance.FindShelfForProduct(currentPid);
                                var priceTag = shelf != null ? shelf.GetComponentInChildren<MiniMarketTycoon.Store.ShelfPriceTag>() : null;
                                if (priceTag != null)
                                {
                                    double effectivePrice = priceTag.CurrentPrice;
                                    double marketPrice = priceTag.MarketBasePrice;

                                    if (effectivePrice > marketPrice * 1.25)
                                    {
                                        float refuseChance = (_personalityData.PriceSensitivity * 0.75f) + 0.25f;
                                        if (UnityEngine.Random.value < refuseChance)
                                        {
                                            priceAccepted = false;
                                            if (FloatingFeedbackManager.HasInstance)
                                            {
                                                FloatingFeedbackManager.Instance.ShowMoodFeedback(
                                                    transform.position + Vector3.up * 1.6f,
                                                    $"😡 Çok Pahalı! (${effectivePrice:F2})",
                                                    new Color(1f, 0.25f, 0.25f, 1f));
                                            }
                                            _personalityData.DeductSatisfaction(15, "PriceTooHigh");
                                            _personalityData.RecordItemSkipped();
                                        }
                                    }
                                    else if (effectivePrice < marketPrice * 0.9)
                                    {
                                        _personalityData.Satisfaction = Mathf.Min(100, _personalityData.Satisfaction + 8);
                                        if (FloatingFeedbackManager.HasInstance)
                                        {
                                            FloatingFeedbackManager.Instance.ShowMoodFeedback(
                                                transform.position + Vector3.up * 1.6f,
                                                "😍 İndirimli Ürün!",
                                                new Color(0.2f, 0.95f, 0.4f, 1f));
                                        }
                                    }
                                }
                            }

                            if (priceAccepted)
                            {
                                int currentStock = InventoryManager.Instance.GetStock(currentPid);
                                int wanted = shoppingItem.QuantityRemaining;
                                int take = Mathf.Min(wanted, currentStock);

                                if (take > 0 && InventoryManager.Instance.TryConsumeStock(currentPid, take))
                                {
                                    var pData = InventoryManager.Instance.GetProductData(currentPid);
                                    _shoppingData.AddProductToCart(pData, take);
                                    shoppingItem.QuantityCollected += take;
                                    _shoppingData.RecordCollected(take);

                                    if (take < wanted)
                                    {
                                        _personalityData.DeductSatisfaction(5, "PartialStock");
                                    }
                                }
                                else
                                {
                                    _personalityData.RecordItemSkipped();
                                }
                            }
                        }

                        _shoppingData.AdvanceToNextItem();

                        if (_personalityData.HasLeftDueToImpatience || _personalityData.Satisfaction < 40)
                        {
                            if (FloatingFeedbackManager.HasInstance)
                            {
                                FloatingFeedbackManager.Instance.ShowMoodFeedback(transform.position, "😤 Leaving store!", new Color(1f, 0.35f, 0.35f, 1f));
                            }
                            OnCustomerLeftDueToImpatience?.Invoke(this);
                            ChangeState(CustomerState.Leaving);
                            break;
                        }

                        if (_shoppingData.HasPendingItems())
                        {
                            SelectNextShelfTarget();
                        }
                        else
                        {
                            ChangeState(_shoppingData.ItemsCollected > 0 ? CustomerState.GoingToCheckout : CustomerState.Leaving);
                        }
                    }
                    break;

                case CustomerState.WaitingInQueue:
                    _personalityData.CheckoutQueueTimer += dt;
                    float queuePatienceLimit = 15f * _personalityData.CheckoutPatienceMultiplier;
                    if (_personalityData.PersonalityType == CustomerPersonalityType.ImpatientShopper &&
                        (_personalityData.CheckoutQueueTimer >= queuePatienceLimit || _shoppingData.RemainingPatience <= 0f))
                    {
                        _personalityData.HasLeftDueToImpatience = true;
                        _personalityData.DeductSatisfaction(30, "QueueTooLong");
                        if (FloatingFeedbackManager.HasInstance)
                        {
                            FloatingFeedbackManager.Instance.ShowMoodFeedback(transform.position, "😤 Queue too long!", new Color(1f, 0.3f, 0.3f, 1f));
                        }
                        if (_queueController != null)
                        {
                            _queueController.RemoveFromQueue(this);
                        }
                        OnCustomerLeftDueToImpatience?.Invoke(this);
                        ChangeState(CustomerState.Leaving);
                    }
                    break;

                case CustomerState.CheckingOut:
                    if (_stateTimer >= _actionDuration)
                    {
                        // Checkout payment completed! Calculate revenue & net profit
                        double total = _shoppingData.CalculateCartTotal();
                        double cost = _shoppingData.CalculateCartCost();
                        double profit = total - cost;

                        if (total > 0.0)
                        {
                            if (CurrencyManager.HasInstance)
                            {
                                CurrencyManager.Instance.AddCurrency(total);
                            }

                            if (EconomyManager.HasInstance)
                            {
                                EconomyManager.Instance.RecordSale(total, cost, _shoppingData.ItemsCollected);
                                foreach (var cartItem in _shoppingData.CartItems)
                                {
                                    EconomyManager.Instance.RecordProductSale(cartItem.ProductId, cartItem.Quantity, cartItem.TotalRevenue, cartItem.TotalProfit);
                                }
                            }

                            if (FloatingFeedbackManager.HasInstance)
                            {
                                FloatingFeedbackManager.Instance.ShowCashEarned(transform.position, total, profit);
                            }
                        }

                        if (_queueController != null)
                        {
                            _queueController.RemoveFromQueue(this);
                        }
                        ChangeState(CustomerState.Leaving);
                    }
                    break;

                case CustomerState.GoingToCheckout:
                    // If queue was full or missing, retry periodically
                    if (_stateTimer >= 5.0f && _queueController == null)
                    {
                        ChangeState(CustomerState.Leaving);
                    }
                    else if (_stateTimer >= 1.5f)
                    {
                        _stateTimer = 0f;
                        TryEnterCheckoutQueue();
                    }
                    break;
            }
        }

        private void SelectNextShelfTarget()
        {
            string productId = _shoppingData.GetCurrentItem();
            if (string.IsNullOrEmpty(productId))
            {
                ChangeState(_shoppingData.ItemsCollected > 0 ? CustomerState.GoingToCheckout : CustomerState.Leaving);
                return;
            }

            // Check if product is in stock
            if (InventoryManager.HasInstance && !InventoryManager.Instance.HasStock(productId))
            {
                // Item out of stock! Skip to next item or checkout
                _personalityData.RecordItemSkipped();
                _shoppingData.SkipCurrentItem();

                if (_personalityData.HasLeftDueToImpatience || _personalityData.Satisfaction < 40)
                {
                    if (FloatingFeedbackManager.HasInstance)
                    {
                        FloatingFeedbackManager.Instance.ShowMoodFeedback(transform.position, "😤 Missing item!", new Color(1f, 0.35f, 0.35f, 1f));
                    }
                    OnCustomerLeftDueToImpatience?.Invoke(this);
                    ChangeState(CustomerState.Leaving);
                    return;
                }

                if (_shoppingData.HasPendingItems())
                {
                    SelectNextShelfTarget();
                }
                else
                {
                    ChangeState(_shoppingData.ItemsCollected > 0 ? CustomerState.GoingToCheckout : CustomerState.Leaving);
                }
                return;
            }

            // 1. Locate dedicated product shelf via ShelfManager
            CustomerInteractionPoint pt = null;
            if (ShelfManager.HasInstance)
            {
                var shelf = ShelfManager.Instance.FindShelfForProduct(productId);
                if (shelf != null)
                {
                    pt = shelf.GetAvailableInteractionPoint();
                }
            }

            // 2. Fallback to TargetSelector category point
            if (pt == null && _targetSelector != null)
            {
                string category = MapProductIdToCategory(productId);
                pt = _targetSelector.GetPointForCategory(category);
            }

            if (pt != null)
            {
                _shoppingData.SetTargetPoint(pt);
                ChangeState(CustomerState.GoingToShelf);
            }
            else
            {
                // Fallback: Proceed to checkout if no valid shelf spot is available
                ChangeState(_shoppingData.ItemsCollected > 0 ? CustomerState.GoingToCheckout : CustomerState.Leaving);
            }
        }

        private string MapProductIdToCategory(string productId)
        {
            switch (productId)
            {
                case "prod_water": return "Water";
                case "prod_milk": return "Milk";
                case "prod_chips": return "Snacks";
                case "prod_chocolate": return "Snacks";
                case "prod_soda": return "Soda";
                case "prod_canned": return "Canned";
                case "prod_juice": return "Soda";
                case "prod_yogurt": return "Milk";
                case "prod_cookies": return "Snacks";
                case "prod_candy": return "Snacks";
                case "prod_sauce": return "Canned";
                case "prod_rice": return "Canned";
                default: return productId;
            }
        }

        private void TryEnterCheckoutQueue()
        {
            if (_queueController == null)
            {
                return;
            }

            if (_queueController.TryJoinQueue(this, out int slotIndex, out Vector3 targetPos))
            {
                _shoppingData.SetQueueIndex(slotIndex);
                _navigation.MoveTo(targetPos, onReached: () =>
                {
                    if (_shoppingData.QueueIndex == 0)
                    {
                        ChangeState(CustomerState.CheckingOut);
                    }
                    else
                    {
                        ChangeState(CustomerState.WaitingInQueue);
                    }
                }, onStuck: HandleStuckState);
            }
            else
            {
                // Queue full: if customer has run out of patience, leave the store
                if (_shoppingData.RemainingPatience <= 0f)
                {
                    ChangeState(CustomerState.Leaving);
                }
            }
        }

        /// <summary>
        /// Invoked by CustomerQueueController when queue shifts forward.
        /// </summary>
        public void OnQueuePositionUpdated(int newSlotIndex, Vector3 newPosition, Vector3 facingDir)
        {
            _shoppingData.SetQueueIndex(newSlotIndex);

            if (newSlotIndex == 0)
            {
                // Reached the front of the line (Cash register!)
                _navigation.MoveTo(newPosition, onReached: () =>
                {
                    ChangeState(CustomerState.CheckingOut);
                }, onStuck: () =>
                {
                    ChangeState(CustomerState.CheckingOut);
                });
            }
            else
            {
                // Advance to new waiting spot
                _navigation.MoveTo(newPosition, onReached: () =>
                {
                    _navigation.AlignFacing(facingDir);
                });
            }
        }

        private void HandleStuckState()
        {
            Debug.LogWarning($"[CustomerController] Recovering stuck customer {name} in state {_currentState}. Routing to safe exit.");
            if (_queueController != null)
            {
                _queueController.RemoveFromQueue(this);
            }
            _shoppingData.Clear();
            ChangeState(CustomerState.Leaving);
        }

        public void ApplyVisualProfile(CustomerVisualProfile profile)
        {
            if (_visualController != null)
            {
                _visualController.ApplyProfile(profile);
                if (_animationController != null && profile != null)
                {
                    _animationController.SetGender(profile.Gender);
                    _animationController.SetHeightMultiplier(profile.HeightMultiplier);
                }
            }
        }

        public void ResetForPool()
        {
            if (_visualController == null || _navigation == null || _animationController == null)
            {
                InitializeComponents();
            }

            if (_navigation != null)
            {
                _navigation.Stop();
            }
            if (_queueController != null)
            {
                _queueController.RemoveFromQueue(this);
            }
            if (_shoppingData != null)
            {
                _shoppingData.Clear();
            }
            if (_personalityData != null)
            {
                _personalityData.Reset();
            }
            _currentState = CustomerState.Idle;
            if (_animationController != null)
            {
                _animationController.ResetToNeutral();
                _animationController.SetState(CustomerState.Idle);
                _animationController.SetPersonality(CustomerPersonalityType.Normal);
            }
            if (_visual != null)
            {
                _visual.SetBasketVisible(false);
                _visual.SetDebugState(CustomerState.Idle, false);
            }
            if (_visualController != null)
            {
                _visualController.ResetVisuals();
            }
            if (_lookAtController != null)
            {
                _lookAtController.ResetLookAt();
            }
        }
    }
}
