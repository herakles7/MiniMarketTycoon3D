using System;
using UnityEngine;
using MiniMarketTycoon.Gameplay;
using MiniMarketTycoon.UI;

namespace MiniMarketTycoon.Store
{
    /// <summary>
    /// 3D physical price tag mounted on supermarket shelf rails.
    /// Displays the product name, wholesale cost, and player-configured selling price.
    /// Allows the store manager to adjust retail prices and observe customer price elasticity.
    /// </summary>
    public class ShelfPriceTag : MonoBehaviour, IInteractable
    {
        [Header("Shelf Reference")]
        [SerializeField] private ShelfVisualController _shelf;

        [Header("Price Tag Settings")]
        [SerializeField] private double _customPrice = -1.0; // -1 means default product sell price
        [SerializeField] private TextMesh _priceTextMesh;
        [SerializeField] private TextMesh _nameTextMesh;
        [SerializeField] private Transform _tagPlate;

        public ShelfVisualController Shelf => _shelf;
        public double CurrentPrice => GetEffectivePrice();
        public double MarketBasePrice => _shelf != null && _shelf.ProductData != null ? _shelf.ProductData.SellPrice : 2.00;
        public double PurchaseCost => _shelf != null && _shelf.ProductData != null ? _shelf.ProductData.PurchasePrice : 1.00;

        public event Action<double> OnPriceAdjusted;

        private void Awake()
        {
            if (_shelf == null) _shelf = GetComponentInParent<ShelfVisualController>();
            EnsureVisuals();
        }

        private void Start()
        {
            UpdateTagVisuals();
        }

        public double GetEffectivePrice()
        {
            if (_customPrice > 0.0) return _customPrice;
            if (_shelf != null && _shelf.ProductData != null) return _shelf.ProductData.SellPrice;
            return 2.00;
        }

        public void SetCustomPrice(double newPrice)
        {
            _customPrice = Math.Round(Math.Max(0.50, newPrice), 2);
            UpdateTagVisuals();
            OnPriceAdjusted?.Invoke(_customPrice);

            if (FloatingFeedbackManager.HasInstance)
            {
                FloatingFeedbackManager.Instance.ShowMoodFeedback(
                    transform.position + Vector3.up * 0.8f,
                    $"FİYAT: ${_customPrice:F2}",
                    new Color(0.2f, 0.85f, 1.0f));
            }
        }

        public void IncreasePrice(double step = 0.25)
        {
            SetCustomPrice(GetEffectivePrice() + step);
        }

        public void DecreasePrice(double step = 0.25)
        {
            SetCustomPrice(GetEffectivePrice() - step);
        }

        private void EnsureVisuals()
        {
            Transform existing = transform.Find("TagPlate");
            if (existing == null)
            {
                GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "TagPlate";
                plate.transform.SetParent(transform, false);
                plate.transform.localScale = new Vector3(0.35f, 0.16f, 0.02f);
                plate.transform.localPosition = Vector3.zero;

                var c = plate.GetComponent<Collider>();
                if (c != null) Destroy(c);

                var r = plate.GetComponent<Renderer>();
                if (r != null)
                {
                    Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                    Material m = new Material(s);
                    m.color = new Color(0.98f, 0.98f, 0.96f); // Clean white price label
                    r.material = m;
                }
                _tagPlate = plate.transform;

                // Price Text
                GameObject priceTxt = new GameObject("PriceText");
                priceTxt.transform.SetParent(transform, false);
                priceTxt.transform.localPosition = new Vector3(0f, 0.02f, -0.015f);
                _priceTextMesh = priceTxt.AddComponent<TextMesh>();
                _priceTextMesh.alignment = TextAlignment.Center;
                _priceTextMesh.anchor = TextAnchor.MiddleCenter;
                _priceTextMesh.characterSize = 0.024f;
                _priceTextMesh.fontSize = 32;
                _priceTextMesh.color = new Color(0.05f, 0.55f, 0.2f, 1f); // Vibrant price green
            }
            else
            {
                _tagPlate = existing;
                _priceTextMesh = GetComponentInChildren<TextMesh>();
            }
        }

        public void UpdateTagVisuals()
        {
            if (_priceTextMesh == null) EnsureVisuals();
            if (_priceTextMesh == null) return;

            double price = GetEffectivePrice();
            double market = MarketBasePrice;

            if (price > market * 1.2)
            {
                // Expensive tag (Red tint)
                _priceTextMesh.color = new Color(0.85f, 0.15f, 0.15f, 1f);
                _priceTextMesh.text = $"<b>${price:F2}</b>\n<size=18><color=#888888>Piyasa: ${market:F2}</color></size>";
            }
            else if (price < market * 0.9)
            {
                // Discount tag (Blue tint)
                _priceTextMesh.color = new Color(0.1f, 0.5f, 0.95f, 1f);
                _priceTextMesh.text = $"<b>${price:F2}</b>\n<size=18><color=#008833>İndirim!</color></size>";
            }
            else
            {
                // Normal green
                _priceTextMesh.color = new Color(0.08f, 0.6f, 0.25f, 1f);
                _priceTextMesh.text = $"<b>${price:F2}</b>\n<size=18><color=#666666>Piyasa: ${market:F2}</color></size>";
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // IInteractable Implementation (Price Setting)
        // ─────────────────────────────────────────────────────────────────────────
        public string GetInteractionPrompt()
        {
            double price = GetEffectivePrice();
            return $"[E] Fiyat Ayarla (${price:F2})";
        }

        public Vector3 GetPromptWorldPosition()
        {
            return transform.position + Vector3.up * 0.35f;
        }

        public bool CanInteract(PlayerManagerController player)
        {
            return player != null && !player.IsCarryingBox;
        }

        public void OnInteract(PlayerManagerController player)
        {
            // Cycles price: +$0.25, loops back if too high
            double cur = GetEffectivePrice();
            double market = MarketBasePrice;
            double next = cur + 0.25;
            if (next > market * 1.6)
            {
                next = market * 0.8; // Cycle back to discount
            }
            SetCustomPrice(next);
        }

        public bool OnHoldInteract(PlayerManagerController player, float deltaTime) => false;
        public void OnFocusEnter(PlayerManagerController player) { }
        public void OnFocusExit(PlayerManagerController player) { }
    }
}
