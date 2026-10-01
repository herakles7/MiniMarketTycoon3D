using UnityEngine;
using MiniMarketTycoon.Store;

namespace MiniMarketTycoon.Gameplay
{
    /// <summary>
    /// Physical delivery cardboard box containing wholesale products.
    /// Can be picked up, carried by the player, and unloaded onto retail store shelves.
    /// When emptied, it turns into recyclable cardboard trash.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ProductBoxController : MonoBehaviour, ICarriable, IInteractable
    {
        [Header("Box Cargo")]
        [SerializeField] private string _productId = "prod_water";
        [SerializeField] private int _itemCount = 10;
        [SerializeField] private int _capacity = 10;
        [SerializeField] private bool _isCarried = false;

        [Header("Visual Components")]
        [SerializeField] private TextMesh _boxLabel;
        [SerializeField] private GameObject _openFlaps;
        [SerializeField] private GameObject _contentsVisual;

        private Collider _boxCollider;
        private Rigidbody _rigidbody;
        private Transform _originalParent;

        public string ProductId => _productId;
        public int ItemCount => _itemCount;
        public int Capacity => _capacity;
        public bool IsEmpty => _itemCount <= 0;
        public bool IsCarried => _isCarried;

        public string ItemName => IsEmpty ? "Boş Koli (Geri Dönüşüm)" : $"{GetProductName()} Kolisi ({_itemCount} Adet)";

        private void Awake()
        {
            _boxCollider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            EnsureVisualLabels();
        }

        public void Initialize(string productId, int count)
        {
            _productId = productId;
            _capacity = count;
            _itemCount = count;
            UpdateVisuals();
        }

        private void EnsureVisualLabels()
        {
            if (_boxLabel == null)
            {
                Transform existing = transform.Find("BoxLabel");
                if (existing != null)
                {
                    _boxLabel = existing.GetComponent<TextMesh>();
                }
                else
                {
                    GameObject lblGo = new GameObject("BoxLabel");
                    lblGo.transform.SetParent(transform, false);
                    lblGo.transform.localPosition = new Vector3(0f, 0.28f, 0f);
                    lblGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                    _boxLabel = lblGo.AddComponent<TextMesh>();
                    _boxLabel.alignment = TextAlignment.Center;
                    _boxLabel.anchor = TextAnchor.MiddleCenter;
                    _boxLabel.characterSize = 0.045f;
                    _boxLabel.fontSize = 28;
                    _boxLabel.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                }
            }

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (_boxLabel != null)
            {
                if (IsEmpty)
                {
                    _boxLabel.text = "<color=#888888>[BOŞ KUTU]\nÇöpe At</color>";
                }
                else
                {
                    string pName = GetProductName();
                    _boxLabel.text = $"<b>{pName}</b>\n<color=#007722>x{_itemCount}</color>";
                }
            }

            if (_contentsVisual != null)
            {
                _contentsVisual.SetActive(!IsEmpty);
            }
        }

        private string GetProductName()
        {
            if (InventoryManager.HasInstance)
            {
                var data = InventoryManager.Instance.GetProductData(_productId);
                if (data != null) return data.DisplayName;
            }
            return _productId.Replace("prod_", "").ToUpper();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // ICarriable Implementation
        // ─────────────────────────────────────────────────────────────────────────
        public void AttachToSocket(Transform socket)
        {
            _isCarried = true;
            _originalParent = transform.parent;
            transform.SetParent(socket, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (_boxCollider != null) _boxCollider.enabled = false;
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.detectCollisions = false;
            }
        }

        public void DetachFromSocket()
        {
            _isCarried = false;
            transform.SetParent(_originalParent, true);

            if (_boxCollider != null) _boxCollider.enabled = true;
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = false;
                _rigidbody.detectCollisions = true;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        // IInteractable Implementation (When box is on the floor)
        // ─────────────────────────────────────────────────────────────────────────
        public string GetInteractionPrompt()
        {
            if (IsEmpty)
            {
                return "[E] Boş Koliyi Al (Çöpe At)";
            }
            return $"[E] {GetProductName()} Kolisini Al (x{_itemCount})";
        }

        public Vector3 GetPromptWorldPosition()
        {
            return transform.position + Vector3.up * 0.4f;
        }

        public bool CanInteract(PlayerManagerController player)
        {
            // Can only pick up if player is not already carrying an item
            return !_isCarried && player != null && !player.IsCarryingBox;
        }

        public void OnInteract(PlayerManagerController player)
        {
            if (player != null && !player.IsCarryingBox)
            {
                player.PickUpItem(this);
            }
        }

        public bool OnHoldInteract(PlayerManagerController player, float deltaTime) => false;
        public void OnFocusEnter(PlayerManagerController player) { }
        public void OnFocusExit(PlayerManagerController player) { }

        /// <summary>
        /// Unloads one or more items into a store shelf.
        /// </summary>
        public int UnloadItems(int amount)
        {
            int toUnload = Mathf.Min(amount, _itemCount);
            _itemCount -= toUnload;
            UpdateVisuals();
            return toUnload;
        }
    }
}
