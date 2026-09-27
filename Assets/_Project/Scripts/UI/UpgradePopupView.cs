using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Interactive modal dialog displaying market tier progression, costs,
    /// dynamic perk comparison cards, and safe purchase validation.
    /// </summary>
    public class UpgradePopupView : MonoBehaviour
    {
        [Header("Root & Containers")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backdropButton;

        [Header("Header & Level Badges")]
        [SerializeField] private Text _currentLevelBadgeText;
        [SerializeField] private Text _nextLevelBadgeText;
        [SerializeField] private Text _costValueText;

        [Header("Perk Comparison Rows")]
        [SerializeField] private Text _customersCompareText;
        [SerializeField] private Text _spawnRateCompareText;
        [SerializeField] private Text _shelfCapCompareText;
        [SerializeField] private Text _productCapCompareText;
        [SerializeField] private Text _checkoutSpeedCompareText;
        [SerializeField] private Text _marketSizeCompareText;
        [SerializeField] private Text _expansionStatusText;

        [Header("Action Button & Feedback")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Text _upgradeButtonText;
        [SerializeField] private Image _upgradeButtonImage;
        [SerializeField] private Text _feedbackToastText;

        private Coroutine _toastCoroutine;
        private bool _isProcessingUpgrade;

        private void Awake()
        {
            if (_panelRoot == null) _panelRoot = gameObject;
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
            if (_backdropButton != null) _backdropButton.onClick.AddListener(Hide);
            if (_upgradeButton != null) _upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        private void Start()
        {
            if (MarketUpgradeManager.HasInstance)
            {
                MarketUpgradeManager.Instance.OnProgressionChanged += RefreshUI;
            }

            if (CurrencyManager.HasInstance)
            {
                CurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }
        }

        private void HandleCurrencyChanged(double newCash, double delta)
        {
            if (_panelRoot != null && _panelRoot.activeSelf)
            {
                RefreshUI();
            }
        }

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            _isProcessingUpgrade = false;
            RefreshUI();
        }

        public void Hide()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
            _isProcessingUpgrade = false;
        }

        public void RefreshUI()
        {
            if (!MarketUpgradeManager.HasInstance) return;

            var mgr = MarketUpgradeManager.Instance;
            int curLevel = mgr.CurrentLevel;
            bool isMax = mgr.IsMaxLevel;
            var curData = mgr.CurrentLevelData;
            var nxtData = mgr.NextLevelData;

            if (_currentLevelBadgeText != null)
            {
                _currentLevelBadgeText.text = $"LEVEL {curLevel} — {curData.Title.ToUpper()}";
            }

            if (isMax)
            {
                if (_nextLevelBadgeText != null) _nextLevelBadgeText.text = "MAX LEVEL REACHED";
                if (_costValueText != null) _costValueText.text = "MAX TIER";

                if (_customersCompareText != null) _customersCompareText.text = $"{curData.MaxCustomers} (MAX)";
                if (_spawnRateCompareText != null) _spawnRateCompareText.text = $"{curData.SpawnInterval:F1}s (MAX)";
                if (_shelfCapCompareText != null) _shelfCapCompareText.text = $"{curData.ShelfCapacity} Items (MAX)";
                if (_productCapCompareText != null) _productCapCompareText.text = $"{mgr.GetEffectiveMaxStock(50)} Units (MAX)";
                if (_checkoutSpeedCompareText != null) _checkoutSpeedCompareText.text = $"{curData.CheckoutTime:F1}s (MAX)";
                if (_marketSizeCompareText != null) _marketSizeCompareText.text = "Grand Tycoon Mart (MAX)";
                if (_expansionStatusText != null) _expansionStatusText.text = "<color=#4CE685>ALL STORE AREAS UNLOCKED</color>";

                if (_upgradeButton != null)
                {
                    _upgradeButton.interactable = false;
                    if (_upgradeButtonText != null) _upgradeButtonText.text = "MAX LEVEL REACHED";
                    if (_upgradeButtonImage != null) _upgradeButtonImage.color = new Color(0.3f, 0.35f, 0.4f, 0.8f);
                }
            }
            else
            {
                double cost = mgr.NextUpgradeCost;
                bool canAfford = mgr.CanAffordUpgrade();

                if (_nextLevelBadgeText != null) _nextLevelBadgeText.text = $"NEXT: LEVEL {curLevel + 1} — {nxtData.Title.ToUpper()}";
                if (_costValueText != null) _costValueText.text = $"Upgrade Cost: {CurrencyFormatter.Format(cost)}";

                int custDiff = nxtData.MaxCustomers - curData.MaxCustomers;
                float spawnDiff = curData.SpawnInterval - nxtData.SpawnInterval;
                int shelfDiff = nxtData.ShelfCapacity - curData.ShelfCapacity;
                int curStock50 = mgr.GetEffectiveMaxStock(50);
                int nxtStock50 = Mathf.RoundToInt(50 * nxtData.ProductCapacityMultiplier);
                int stockDiff = nxtStock50 - curStock50;
                float speedDiff = curData.CheckoutTime - nxtData.CheckoutTime;

                string curSizeName = GetMarketSizeName(curLevel);
                string nxtSizeName = GetMarketSizeName(curLevel + 1);
                string newAreaName = GetExpansionAreaName(curLevel + 1);

                if (_customersCompareText != null) _customersCompareText.text = $"{curData.MaxCustomers} → {nxtData.MaxCustomers} <color=#4CE685>(+{custDiff})</color>";
                if (_spawnRateCompareText != null) _spawnRateCompareText.text = $"{curData.SpawnInterval:F1}s → {nxtData.SpawnInterval:F1}s <color=#4CE685>(-{spawnDiff:F1}s)</color>";
                if (_shelfCapCompareText != null) _shelfCapCompareText.text = $"{curData.ShelfCapacity} → {nxtData.ShelfCapacity} <color=#4CE685>(+{shelfDiff})</color>";
                if (_productCapCompareText != null) _productCapCompareText.text = $"{curStock50} → {nxtStock50} <color=#4CE685>(+{stockDiff})</color>";
                if (_checkoutSpeedCompareText != null) _checkoutSpeedCompareText.text = $"{curData.CheckoutTime:F1}s → {nxtData.CheckoutTime:F1}s <color=#4CE685>(-{speedDiff:F1}s)</color>";
                if (_marketSizeCompareText != null) _marketSizeCompareText.text = $"{curSizeName} → {nxtSizeName}";
                if (_expansionStatusText != null) _expansionStatusText.text = $"<color=#FFD700>🔓 UNLOCKS: {newAreaName}</color>";

                if (_upgradeButton != null)
                {
                    _upgradeButton.interactable = true;
                    if (_upgradeButtonText != null)
                    {
                        _upgradeButtonText.text = canAfford
                            ? $"UPGRADE MARKET ({CurrencyFormatter.Format(cost)})"
                            : $"NOT ENOUGH MONEY ({CurrencyFormatter.Format(cost)})";
                    }

                    if (_upgradeButtonImage != null)
                    {
                        _upgradeButtonImage.color = canAfford
                            ? new Color(0.18f, 0.75f, 0.38f, 1f) // Emerald Green
                            : new Color(0.7f, 0.3f, 0.3f, 0.9f); // Dark Red
                    }
                }
            }
        }

        private void OnUpgradeClicked()
        {
            if (_isProcessingUpgrade) return; // Prevent multiple rapid clicks

            if (!MarketUpgradeManager.HasInstance) return;

            var mgr = MarketUpgradeManager.Instance;
            if (mgr.IsMaxLevel)
            {
                ShowToast("Already at maximum market level!", Color.yellow);
                return;
            }

            double cost = mgr.NextUpgradeCost;
            if (!mgr.CanAffordUpgrade())
            {
                double currentCash = CurrencyManager.HasInstance ? CurrencyManager.Instance.Cash : 0.0;
                ShowToast($"Not enough money! Need {CurrencyFormatter.Format(cost)}, have {CurrencyFormatter.Format(currentCash)}", new Color(1f, 0.45f, 0.45f));
                return;
            }

            _isProcessingUpgrade = true;

            int oldLevel = mgr.CurrentLevel;
            bool success = mgr.TryUpgradeMarket();

            if (success)
            {
                int newLevel = mgr.CurrentLevel;
                ShowToast($"MARKET LEVEL UP! LEVEL {newLevel}", new Color(0.3f, 1f, 0.5f));

                if (FloatingFeedbackManager.HasInstance)
                {
                    FloatingFeedbackManager.Instance.ShowLevelUpFeedback(newLevel);
                }

                RefreshUI();
            }
            else
            {
                ShowToast("Upgrade failed!", Color.red);
            }

            _isProcessingUpgrade = false;
        }

        public void ShowToast(string message, Color color)
        {
            if (_feedbackToastText == null) return;

            if (_toastCoroutine != null)
            {
                StopCoroutine(_toastCoroutine);
            }

            _toastCoroutine = StartCoroutine(ToastRoutine(message, color));
        }

        private IEnumerator ToastRoutine(string message, Color color)
        {
            _feedbackToastText.gameObject.SetActive(true);
            _feedbackToastText.text = message;
            _feedbackToastText.color = color;

            yield return new WaitForSeconds(2.5f);

            _feedbackToastText.gameObject.SetActive(false);
            _toastCoroutine = null;
        }

        private void OnDestroy()
        {
            if (MarketUpgradeManager.HasInstance)
            {
                MarketUpgradeManager.Instance.OnProgressionChanged -= RefreshUI;
            }

            if (CurrencyManager.HasInstance)
            {
                CurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        /// <summary>
        /// Creates modern portrait-oriented Upgrade Modal dynamically if not in scene.
        /// </summary>
        public static UpgradePopupView CreateDynamic(Transform parent)
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

            // 1. Root Fullscreen Dim Backdrop
            GameObject root = new GameObject("Upgrade_Popup_Modal");
            root.transform.SetParent(parent, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var backdropImg = root.AddComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.72f);
            var backdropBtn = root.AddComponent<Button>();

            var view = root.AddComponent<UpgradePopupView>();
            view._panelRoot = root;
            view._backdropButton = backdropBtn;

            // 2. Main Window Box
            GameObject window = new GameObject("Window_Box");
            window.transform.SetParent(root.transform, false);
            var winRect = window.AddComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(400f, 640f);

            var winImg = window.AddComponent<Image>();
            winImg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            // 3. Header Bar
            GameObject header = new GameObject("Header");
            header.transform.SetParent(window.transform, false);
            var headRect = header.AddComponent<RectTransform>();
            headRect.anchorMin = new Vector2(0f, 0.92f);
            headRect.anchorMax = new Vector2(1f, 1f);
            headRect.offsetMin = new Vector2(16f, 0f);
            headRect.offsetMax = new Vector2(-16f, 0f);

            var titleTxt = header.AddComponent<Text>();
            titleTxt.font = defaultFont;
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.text = "MARKET EXPANSION";
            titleTxt.alignment = TextAnchor.MiddleLeft;
            titleTxt.color = Color.white;

            // Close [X] Button
            GameObject closeGo = new GameObject("CloseButton");
            closeGo.transform.SetParent(header.transform, false);
            var closeRect = closeGo.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.88f, 0.1f);
            closeRect.anchorMax = new Vector2(1f, 0.9f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;

            var closeImg = closeGo.AddComponent<Image>();
            closeImg.color = new Color(0.85f, 0.25f, 0.25f);
            var closeBtn = closeGo.AddComponent<Button>();
            view._closeButton = closeBtn;
            closeBtn.onClick.AddListener(view.Hide);

            GameObject closeTxtGo = new GameObject("X");
            closeTxtGo.transform.SetParent(closeGo.transform, false);
            var cTxtRect = closeTxtGo.AddComponent<RectTransform>();
            cTxtRect.anchorMin = Vector2.zero;
            cTxtRect.anchorMax = Vector2.one;
            cTxtRect.offsetMin = Vector2.zero;
            cTxtRect.offsetMax = Vector2.zero;
            var cTxt = closeTxtGo.AddComponent<Text>();
            cTxt.font = defaultFont;
            cTxt.fontSize = 18;
            cTxt.fontStyle = FontStyle.Bold;
            cTxt.text = "✕";
            cTxt.alignment = TextAnchor.MiddleCenter;
            cTxt.color = Color.white;

            // 4. Level Badges Banner
            GameObject badgeBox = new GameObject("Level_Badge_Box");
            badgeBox.transform.SetParent(window.transform, false);
            var badgeRect = badgeBox.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.05f, 0.79f);
            badgeRect.anchorMax = new Vector2(0.95f, 0.91f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;

            var badgeImg = badgeBox.AddComponent<Image>();
            badgeImg.color = new Color(0.18f, 0.22f, 0.30f, 0.95f);

            var badgeVlg = badgeBox.AddComponent<VerticalLayoutGroup>();
            badgeVlg.padding = new RectOffset(12, 12, 6, 6);
            badgeVlg.spacing = 3f;
            badgeVlg.childAlignment = TextAnchor.MiddleCenter;
            badgeVlg.childControlWidth = true;
            badgeVlg.childControlHeight = true;

            view._currentLevelBadgeText = CreateLabel(badgeBox.transform, "CurrentLevelTxt", "LEVEL 1", defaultFont, 16, new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);
            view._nextLevelBadgeText = CreateLabel(badgeBox.transform, "NextLevelTxt", "NEXT: LEVEL 2", defaultFont, 13, new Color(0.8f, 0.9f, 1f), TextAnchor.MiddleCenter);

            // 5. Progression Comparison Cards Container
            GameObject cardsContainer = new GameObject("Perks_Container");
            cardsContainer.transform.SetParent(window.transform, false);
            var cardsRect = cardsContainer.AddComponent<RectTransform>();
            cardsRect.anchorMin = new Vector2(0.05f, 0.23f);
            cardsRect.anchorMax = new Vector2(0.95f, 0.78f);
            cardsRect.offsetMin = Vector2.zero;
            cardsRect.offsetMax = Vector2.zero;

            var cardsVlg = cardsContainer.AddComponent<VerticalLayoutGroup>();
            cardsVlg.spacing = 5f;
            cardsVlg.padding = new RectOffset(4, 4, 4, 4);
            cardsVlg.childControlWidth = true;
            cardsVlg.childControlHeight = true;

            view._customersCompareText = CreatePerkCard(cardsContainer.transform, "👥 Customer Capacity", "5 → 7 (+2)", defaultFont);
            view._spawnRateCompareText = CreatePerkCard(cardsContainer.transform, "⏱️ Customer Spawn Interval", "8.0s → 7.0s (-1s)", defaultFont);
            view._shelfCapCompareText = CreatePerkCard(cardsContainer.transform, "📦 Shelf Display Limit", "8 → 12 (+4)", defaultFont);
            view._productCapCompareText = CreatePerkCard(cardsContainer.transform, "🏷️ Warehouse Max Stock", "50 → 75 (+50%)", defaultFont);
            view._checkoutSpeedCompareText = CreatePerkCard(cardsContainer.transform, "💳 Cashier Checkout Time", "2.5s → 2.2s (-0.3s)", defaultFont);
            view._marketSizeCompareText = CreatePerkCard(cardsContainer.transform, "🏪 Store Footprint", "Starter → Expanded", defaultFont);
            view._expansionStatusText = CreatePerkCard(cardsContainer.transform, "🔓 Physical Expansion", "East Wing Deli", defaultFont);

            // 6. Upgrade Cost Display
            GameObject costBox = new GameObject("Cost_Box");
            costBox.transform.SetParent(window.transform, false);
            var costRect = costBox.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0.05f, 0.15f);
            costRect.anchorMax = new Vector2(0.95f, 0.22f);
            costRect.offsetMin = Vector2.zero;
            costRect.offsetMax = Vector2.zero;

            view._costValueText = CreateLabel(costBox.transform, "CostTxt", "Upgrade Cost: $500", defaultFont, 16, new Color(1f, 0.9f, 0.4f), TextAnchor.MiddleCenter);

            // 7. Action Button
            GameObject btnGo = new GameObject("Upgrade_Button");
            btnGo.transform.SetParent(window.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.08f, 0.05f);
            btnRect.anchorMax = new Vector2(0.92f, 0.14f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.75f, 0.38f, 1f);
            var btn = btnGo.AddComponent<Button>();
            view._upgradeButton = btn;
            view._upgradeButtonImage = btnImg;
            btn.onClick.AddListener(view.OnUpgradeClicked);

            GameObject btnTxtGo = new GameObject("Text");
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var bTxtRect = btnTxtGo.AddComponent<RectTransform>();
            bTxtRect.anchorMin = Vector2.zero;
            bTxtRect.anchorMax = Vector2.one;
            bTxtRect.offsetMin = Vector2.zero;
            bTxtRect.offsetMax = Vector2.zero;

            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.font = defaultFont;
            btnTxt.fontSize = 17;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.text = "UPGRADE MARKET";
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            view._upgradeButtonText = btnTxt;

            // 8. Feedback Toast
            GameObject toastGo = new GameObject("FeedbackToast");
            toastGo.transform.SetParent(window.transform, false);
            var toastRect = toastGo.AddComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.05f, 0.015f);
            toastRect.anchorMax = new Vector2(0.95f, 0.075f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;

            var toastTxt = toastGo.AddComponent<Text>();
            toastTxt.font = defaultFont;
            toastTxt.fontSize = 14;
            toastTxt.fontStyle = FontStyle.Bold;
            toastTxt.color = new Color(0.3f, 1f, 0.5f);
            toastTxt.alignment = TextAnchor.MiddleCenter;
            toastGo.SetActive(false);
            view._feedbackToastText = toastTxt;

            root.SetActive(false);
            return view;
        }

        private static Text CreatePerkCard(Transform parent, string title, string initialValue, Font font)
        {
            GameObject card = new GameObject("Card_" + title);
            card.transform.SetParent(parent, false);

            var img = card.AddComponent<Image>();
            img.color = new Color(0.16f, 0.19f, 0.25f, 0.9f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 4, 4);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;

            CreateLabel(card.transform, "Title", title, font, 13, new Color(0.85f, 0.88f, 0.95f), TextAnchor.MiddleLeft);
            Text valTxt = CreateLabel(card.transform, "Value", initialValue, font, 13, Color.white, TextAnchor.MiddleRight);
            valTxt.supportRichText = true;

            return valTxt;
        }

        private static Text CreateLabel(Transform parent, string name, string text, Font font, int size, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = FontStyle.Bold;
            txt.text = text;
            txt.alignment = alignment;
            txt.color = color;
            return txt;
        }

        private static string GetMarketSizeName(int level)
        {
            switch (level)
            {
                case 1: return "Starter Mart";
                case 2: return "Expanded Mini Mart";
                case 3: return "Busy Local Store";
                case 4: return "Popular Superette";
                case 5: default: return "Grand Tycoon Mart";
            }
        }

        private static string GetExpansionAreaName(int level)
        {
            switch (level)
            {
                case 2: return "East Wing & Deli Section";
                case 3: return "East Pantry & Sweet Corridor";
                case 4: return "West Chiller & Produce Bay";
                case 5: default: return "Grand Rear Pavilion";
            }
        }
    }
}
