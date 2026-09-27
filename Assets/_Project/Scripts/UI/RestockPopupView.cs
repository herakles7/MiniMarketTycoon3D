using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MiniMarketTycoon.Store;
using MiniMarketTycoon.Economy;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.UI
{
    /// <summary>
    /// Interactive Restock modal popup where the player can order inventory wholesale,
    /// filter products by category (All, Beverages, Dairy, Snacks, Sweets, Canned Food),
    /// monitor costs, and view daily revenue/profit accounting.
    /// </summary>
    public class RestockPopupView : MonoBehaviour
    {
        [Header("Root & Containers")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private Transform _categoryTabsContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Text _feedbackToastText;

        [Header("Accounting Summary")]
        [SerializeField] private Text _revenueSummaryText;
        [SerializeField] private Text _expenseSummaryText;
        [SerializeField] private Text _profitSummaryText;
        [SerializeField] private Text _soldSummaryText;

        private readonly List<RestockCardItem> _cardItems = new List<RestockCardItem>();
        private readonly List<CategoryTabButton> _categoryTabs = new List<CategoryTabButton>();
        private ProductCategory? _activeCategoryFilter = null; // null = ALL
        private Coroutine _toastCoroutine;

        private class RestockCardItem
        {
            public GameObject CardRoot;
            public ProductData Product;
            public Text StockText;
            public Button RestockButton;
            public Text ButtonText;
        }

        private class CategoryTabButton
        {
            public ProductCategory? Category;
            public Button Button;
            public Image Background;
            public Text Label;
        }

        private void Awake()
        {
            if (_panelRoot == null) _panelRoot = gameObject;
            if (_closeButton != null) _closeButton.onClick.AddListener(Hide);
        }

        private void Start()
        {
            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnStockChanged += HandleStockChanged;
            }
        }

        public void Show()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
            RefreshAllCards();
            RefreshAccountingSummary();
        }

        public void Hide()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        public void SetCategoryFilter(ProductCategory? filter)
        {
            _activeCategoryFilter = filter;
            UpdateCategoryTabVisuals();

            foreach (var card in _cardItems)
            {
                if (card != null && card.CardRoot != null)
                {
                    bool show = (_activeCategoryFilter == null) || (card.Product != null && card.Product.Category == _activeCategoryFilter.Value);
                    card.CardRoot.SetActive(show);
                }
            }
        }

        private void UpdateCategoryTabVisuals()
        {
            Color activeBg = new Color(0.06f, 0.65f, 0.45f); // Vibrant emerald
            Color inactiveBg = new Color(0.18f, 0.22f, 0.28f); // Dark slate

            foreach (var tab in _categoryTabs)
            {
                if (tab == null) continue;
                bool isSelected = tab.Category == _activeCategoryFilter;
                if (tab.Background != null)
                {
                    tab.Background.color = isSelected ? activeBg : inactiveBg;
                }
                if (tab.Label != null)
                {
                    tab.Label.color = isSelected ? Color.white : new Color(0.75f, 0.8f, 0.85f);
                }
            }
        }

        public void RefreshAllCards()
        {
            if (!InventoryManager.HasInstance) return;

            var catalog = InventoryManager.Instance.ProductCatalog;
            if (catalog == null || catalog.Count == 0) return;

            // Build tabs and cards dynamically if containers are uninitialized
            if (_categoryTabsContainer != null && _categoryTabs.Count == 0)
            {
                BuildCategoryTabs();
            }

            if (_cardsContainer != null && _cardsContainer.childCount == 0)
            {
                BuildDynamicProductCards(catalog);
            }

            // Update all existing cards
            foreach (var card in _cardItems)
            {
                UpdateCard(card);
            }

            SetCategoryFilter(_activeCategoryFilter);
        }

        private void BuildCategoryTabs()
        {
            _categoryTabs.Clear();
            if (_categoryTabsContainer == null) return;

            var tabs = new (string label, ProductCategory? category)[]
            {
                ("ALL", null),
                ("DRINKS", ProductCategory.Beverages),
                ("DAIRY", ProductCategory.Dairy),
                ("SNACKS", ProductCategory.Snacks),
                ("SWEETS", ProductCategory.Sweets),
                ("CANNED", ProductCategory.CannedFood)
            };

            var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            foreach (var (label, category) in tabs)
            {
                GameObject tabGo = new GameObject($"Tab_{label}");
                tabGo.transform.SetParent(_categoryTabsContainer, false);
                var rect = tabGo.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(75f, 32f);

                var img = tabGo.AddComponent<Image>();
                img.color = new Color(0.18f, 0.22f, 0.28f);

                var btn = tabGo.AddComponent<Button>();

                GameObject txtGo = new GameObject("Text");
                txtGo.transform.SetParent(tabGo.transform, false);
                var txtRect = txtGo.AddComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.offsetMin = Vector2.zero;
                txtRect.offsetMax = Vector2.zero;

                var txt = txtGo.AddComponent<Text>();
                txt.font = defaultFont;
                txt.fontSize = 12;
                txt.fontStyle = FontStyle.Bold;
                txt.text = label;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(0.8f, 0.85f, 0.9f);

                var tabItem = new CategoryTabButton
                {
                    Category = category,
                    Button = btn,
                    Background = img,
                    Label = txt
                };

                btn.onClick.AddListener(() => SetCategoryFilter(category));
                _categoryTabs.Add(tabItem);
            }

            UpdateCategoryTabVisuals();
        }

        private void BuildDynamicProductCards(IReadOnlyList<ProductData> catalog)
        {
            _cardItems.Clear();

            // Sort by category then name for clear store organization
            List<ProductData> sorted = new List<ProductData>(catalog);
            sorted.Sort((a, b) =>
            {
                int catComp = a.Category.CompareTo(b.Category);
                return catComp != 0 ? catComp : string.Compare(a.DisplayName, b.DisplayName, System.StringComparison.Ordinal);
            });

            foreach (var prod in sorted)
            {
                if (prod == null) continue;

                GameObject cardGo = CreateCardGameObject(prod);
                cardGo.transform.SetParent(_cardsContainer, false);
            }
        }

        private GameObject CreateCardGameObject(ProductData prod)
        {
            GameObject card = new GameObject($"Card_{prod.ID}");
            var rect = card.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 90f);

            var img = card.AddComponent<Image>();
            img.color = new Color(0.15f, 0.17f, 0.22f, 0.96f);

            var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            string catTag = GetCategoryTag(prod.Category);
            Color catColor = GetCategoryColor(prod.Category);

            // 1. Title & Category Text
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(card.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.04f, 0.55f);
            titleRect.anchorMax = new Vector2(0.62f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = defaultFont;
            titleTxt.fontSize = 18;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = Color.white;
            titleTxt.supportRichText = true;
            string hexColor = ColorUtility.ToHtmlStringRGB(catColor);
            titleTxt.text = $"<color=#{hexColor}>[{catTag}]</color> {prod.DisplayName}";
            titleTxt.alignment = TextAnchor.MiddleLeft;

            // 2. Stock Level Text
            GameObject stockGo = new GameObject("StockText");
            stockGo.transform.SetParent(card.transform, false);
            var stockRect = stockGo.AddComponent<RectTransform>();
            stockRect.anchorMin = new Vector2(0.04f, 0.1f);
            stockRect.anchorMax = new Vector2(0.62f, 0.5f);
            stockRect.offsetMin = Vector2.zero;
            stockRect.offsetMax = Vector2.zero;

            var stockTxt = stockGo.AddComponent<Text>();
            stockTxt.font = defaultFont;
            stockTxt.fontSize = 15;
            stockTxt.color = new Color(0.8f, 0.85f, 0.9f);
            stockTxt.alignment = TextAnchor.MiddleLeft;

            // 3. Restock Button
            GameObject btnGo = new GameObject("RestockButton");
            btnGo.transform.SetParent(card.transform, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.65f, 0.15f);
            btnRect.anchorMax = new Vector2(0.96f, 0.85f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.10f, 0.65f, 0.35f);
            var btn = btnGo.AddComponent<Button>();

            GameObject btnTxtGo = new GameObject("BtnText");
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxtRect = btnTxtGo.AddComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.offsetMin = Vector2.zero;
            btnTxtRect.offsetMax = Vector2.zero;

            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.font = defaultFont;
            btnTxt.fontSize = 16;
            btnTxt.fontStyle = FontStyle.Bold;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;

            var cardItem = new RestockCardItem
            {
                CardRoot = card,
                Product = prod,
                StockText = stockTxt,
                RestockButton = btn,
                ButtonText = btnTxt
            };

            btn.onClick.AddListener(() => OnRestockClicked(prod));
            _cardItems.Add(cardItem);
            UpdateCard(cardItem);

            return card;
        }

        private string GetCategoryTag(ProductCategory category)
        {
            switch (category)
            {
                case ProductCategory.Beverages: return "DRINKS";
                case ProductCategory.Dairy: return "DAIRY";
                case ProductCategory.Snacks: return "SNACKS";
                case ProductCategory.Sweets: return "SWEETS";
                case ProductCategory.CannedFood: return "CANNED";
                default: return category.ToString().ToUpper();
            }
        }

        private Color GetCategoryColor(ProductCategory category)
        {
            switch (category)
            {
                case ProductCategory.Beverages: return new Color(0.22f, 0.74f, 0.97f); // Sky blue
                case ProductCategory.Dairy: return new Color(0.38f, 0.65f, 0.98f);     // Soft blue
                case ProductCategory.Snacks: return new Color(0.98f, 0.75f, 0.14f);    // Amber
                case ProductCategory.Sweets: return new Color(0.96f, 0.45f, 0.71f);    // Pink
                case ProductCategory.CannedFood: return new Color(0.98f, 0.57f, 0.24f); // Orange
                default: return Color.white;
            }
        }

        private void UpdateCard(RestockCardItem card)
        {
            if (card == null || card.Product == null || !InventoryManager.HasInstance) return;

            int current = InventoryManager.Instance.GetStock(card.Product.ID);
            int max = InventoryManager.Instance.GetMaxStock(card.Product.ID);
            double price = card.Product.PurchasePrice;

            if (card.StockText != null)
            {
                card.StockText.text = $"Stock: {current}/{max}  •  ${price:F2}/ea";
            }

            int buyAmount = Mathf.Min(10, Mathf.Max(0, max - current));
            double cost = buyAmount * price;

            if (card.ButtonText != null)
            {
                if (current >= max)
                {
                    card.ButtonText.text = "FULL";
                    if (card.RestockButton != null)
                    {
                        card.RestockButton.interactable = false;
                        var img = card.RestockButton.GetComponent<Image>();
                        if (img != null) img.color = new Color(0.3f, 0.35f, 0.4f);
                    }
                }
                else
                {
                    card.ButtonText.text = $"+{buyAmount} (${cost:F2})";
                    if (card.RestockButton != null)
                    {
                        card.RestockButton.interactable = true;
                        var img = card.RestockButton.GetComponent<Image>();
                        if (img != null) img.color = new Color(0.10f, 0.65f, 0.35f);
                    }
                }
            }
        }

        private void OnRestockClicked(ProductData prod)
        {
            if (prod == null || !InventoryManager.HasInstance || !CurrencyManager.HasInstance) return;

            int current = InventoryManager.Instance.GetStock(prod.ID);
            int maxStock = InventoryManager.Instance.GetMaxStock(prod.ID);
            int capacityLeft = Mathf.Max(0, maxStock - current);

            if (capacityLeft <= 0)
            {
                ShowToast("Stock is already at maximum capacity!");
                return;
            }

            int amountToBuy = Mathf.Min(10, capacityLeft);
            double totalCost = amountToBuy * prod.PurchasePrice;

            if (!CurrencyManager.Instance.CanAfford(totalCost))
            {
                ShowToast($"Not enough money! Need ${totalCost:F2}");
                return;
            }

            // Deduct cash, add stock, record expense
            bool deducted = CurrencyManager.Instance.RemoveCurrency(totalCost);
            if (deducted)
            {
                InventoryManager.Instance.TryRestock(prod.ID, amountToBuy, out int actualRestocked, out _);

                if (EconomyManager.HasInstance)
                {
                    EconomyManager.Instance.RecordExpense(totalCost);
                }

                ShowToast($"Restocked +{actualRestocked} {prod.DisplayName} for -${totalCost:F2}!");
                RefreshAllCards();
                RefreshAccountingSummary();
            }
        }

        private void HandleStockChanged(string productId, int newStock, int maxStock)
        {
            var card = _cardItems.Find(c => c.Product != null && c.Product.ID == productId);
            if (card != null)
            {
                UpdateCard(card);
            }
        }

        public void RefreshAccountingSummary()
        {
            if (!EconomyManager.HasInstance) return;

            var em = EconomyManager.Instance;
            if (_revenueSummaryText != null) _revenueSummaryText.text = $"Revenue: ${em.TodayRevenue:F2}";
            if (_expenseSummaryText != null) _expenseSummaryText.text = $"Expenses: ${em.TodayExpenses:F2}";
            if (_profitSummaryText != null)
            {
                double profit = em.TodayProfit;
                _profitSummaryText.text = $"Net Profit: {(profit >= 0 ? "+" : "")}${profit:F2}";
                _profitSummaryText.color = profit >= 0 ? new Color(0.2f, 0.95f, 0.3f) : new Color(0.95f, 0.3f, 0.2f);
            }
            if (_soldSummaryText != null) _soldSummaryText.text = $"Sold: {em.ItemsSold} items ({em.CustomersServed} customers)";
        }

        public void ShowToast(string message)
        {
            if (_feedbackToastText == null) return;

            _feedbackToastText.text = message;
            _feedbackToastText.gameObject.SetActive(true);

            if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(ToastFadeRoutine());
        }

        private System.Collections.IEnumerator ToastFadeRoutine()
        {
            yield return new WaitForSeconds(2.0f);
            if (_feedbackToastText != null)
            {
                _feedbackToastText.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.OnStockChanged -= HandleStockChanged;
            }
        }

        public static RestockPopupView CreateDynamic(Transform canvasTransform) => CreatePopupRuntime(canvasTransform);

        public static RestockPopupView CreatePopupRuntime(Transform canvasTransform)
        {
            var existing = canvasTransform.GetComponentInChildren<RestockPopupView>(true);
            if (existing != null)
            {
                return existing;
            }

            var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 1. Root Overlay
            GameObject root = new GameObject("RestockPopupModal");
            root.transform.SetParent(canvasTransform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var bgImg = root.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.75f);

            var view = root.AddComponent<RestockPopupView>();
            view._panelRoot = root;

            // 2. Central Window Panel
            GameObject window = new GameObject("Restock_Window");
            window.transform.SetParent(root.transform, false);
            var winRect = window.AddComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(460f, 620f);

            var winImg = window.AddComponent<Image>();
            winImg.color = new Color(0.11f, 0.13f, 0.17f, 0.98f);

            // 3. Header Bar
            GameObject header = new GameObject("HeaderBar");
            header.transform.SetParent(window.transform, false);
            var headerRect = header.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.04f, 0.91f);
            headerRect.anchorMax = new Vector2(0.96f, 0.98f);
            headerRect.offsetMin = Vector2.zero;
            headerRect.offsetMax = Vector2.zero;

            var titleTxt = CreateLabel(header.transform, "Title", "WHOLESALE SUPPLIES & RESTOCK", defaultFont, 16, Color.white);
            titleTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
            titleTxt.rectTransform.anchorMax = new Vector2(0.85f, 1f);
            titleTxt.fontStyle = FontStyle.Bold;

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

            // 4. Accounting Summary Panel
            GameObject summaryBox = new GameObject("Accounting_Summary");
            summaryBox.transform.SetParent(window.transform, false);
            var sumRect = summaryBox.AddComponent<RectTransform>();
            sumRect.anchorMin = new Vector2(0.04f, 0.81f);
            sumRect.anchorMax = new Vector2(0.96f, 0.90f);
            sumRect.offsetMin = Vector2.zero;
            sumRect.offsetMax = Vector2.zero;

            var sumImg = summaryBox.AddComponent<Image>();
            sumImg.color = new Color(0.16f, 0.19f, 0.25f, 0.95f);

            var sumGrid = summaryBox.AddComponent<GridLayoutGroup>();
            sumGrid.cellSize = new Vector2(200f, 24f);
            sumGrid.spacing = new Vector2(6f, 2f);
            sumGrid.padding = new RectOffset(6, 6, 4, 4);

            view._revenueSummaryText = CreateLabel(summaryBox.transform, "RevenueTxt", "Revenue: $0.00", defaultFont, 13, new Color(0.7f, 0.9f, 1f));
            view._expenseSummaryText = CreateLabel(summaryBox.transform, "ExpenseTxt", "Expenses: $0.00", defaultFont, 13, new Color(1f, 0.7f, 0.7f));
            view._profitSummaryText = CreateLabel(summaryBox.transform, "ProfitTxt", "Net Profit: +$0.00", defaultFont, 13, new Color(0.3f, 0.95f, 0.4f));
            view._soldSummaryText = CreateLabel(summaryBox.transform, "SoldTxt", "Sold: 0 items", defaultFont, 13, new Color(0.9f, 0.9f, 0.9f));

            // 5. Category Filter Tabs Bar
            GameObject catBar = new GameObject("Category_Filter_Bar");
            catBar.transform.SetParent(window.transform, false);
            var catRect = catBar.AddComponent<RectTransform>();
            catRect.anchorMin = new Vector2(0.04f, 0.74f);
            catRect.anchorMax = new Vector2(0.96f, 0.80f);
            catRect.offsetMin = Vector2.zero;
            catRect.offsetMax = Vector2.zero;

            var hlg = catBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            view._categoryTabsContainer = catBar.transform;

            // 6. Scroll Rect for Product Cards
            GameObject scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(window.transform, false);
            var scrollRectTransform = scrollGo.AddComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0.04f, 0.08f);
            scrollRectTransform.anchorMax = new Vector2(0.96f, 0.73f);
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;

            var scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scrollGo.AddComponent<RectMask2D>();

            GameObject contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            view._cardsContainer = contentGo.transform;

            // 7. Toast Feedback Notification
            GameObject toastGo = new GameObject("FeedbackToast");
            toastGo.transform.SetParent(window.transform, false);
            var toastRect = toastGo.AddComponent<RectTransform>();
            toastRect.anchorMin = new Vector2(0.05f, 0.015f);
            toastRect.anchorMax = new Vector2(0.95f, 0.065f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;

            var toastTxt = toastGo.AddComponent<Text>();
            toastTxt.font = defaultFont;
            toastTxt.fontSize = 15;
            toastTxt.fontStyle = FontStyle.Bold;
            toastTxt.color = new Color(1f, 0.85f, 0.2f);
            toastTxt.alignment = TextAnchor.MiddleCenter;
            toastGo.SetActive(false);
            view._feedbackToastText = toastTxt;

            root.SetActive(false);
            return view;
        }

        private static Text CreateLabel(Transform parent, string name, string text, Font font, int size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = FontStyle.Bold;
            txt.text = text;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = color;
            return txt;
        }
    }
}
