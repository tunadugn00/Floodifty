using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private CanvasGroup shopPanel;
    [SerializeField] private RectTransform shopWindow;

    [Header("Item Buttons")]
    [SerializeField] private Button hintBuyButton;
    [SerializeField] private Button hammerBuyButton;
    [SerializeField] private Button colorBombBuyButton;

    [Header("Price Texts")]
    [SerializeField] private TextMeshProUGUI hintPriceText;
    [SerializeField] private TextMeshProUGUI hammerPriceText;
    [SerializeField] private TextMeshProUGUI colorBombPriceText;

    [Header("UI References")]
    [SerializeField] private HUDController hudController;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private Button closeButton;

    [Header("Colors")]
    [SerializeField] private Color canBuyColor = Color.white;
    [SerializeField] private Color cannotBuyColor = Color.gray;

    void Awake()
    {
        // Setup buttons trong Awake - chạy kể cả khi shopPanel đang SetActive(false)
        if (hintBuyButton != null)
            hintBuyButton.onClick.AddListener(OnBuyHint);

        if (hammerBuyButton != null)
            hammerBuyButton.onClick.AddListener(OnBuyHammer);

        if (colorBombBuyButton != null)
            colorBombBuyButton.onClick.AddListener(OnBuyColorBomb);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged += OnCoinsChanged;

        if (shopPanel != null)
            shopPanel.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged -= OnCoinsChanged;
        }
    }

    // ===== OPEN/CLOSE - KHÔNG PAUSE! =====

    public void OpenShop()
    {
        if (shopPanel == null || shopWindow == null) return;

        // KHÔNG pause game! Chỉ block clicks!

        shopPanel.gameObject.SetActive(true);

        shopPanel.alpha = 0;
        shopWindow.localScale = Vector3.zero;

        shopPanel.blocksRaycasts = true;
        shopPanel.interactable = true;

        // Block board input để tránh click xuyên qua shop
        boardManager?.SetUIBlocking(true);

        // Animation bình thường
        shopPanel.DOFade(1f, 0.3f);
        shopWindow.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack);

        // Luôn refresh giá + trạng thái mỗi lần mở
        UpdatePrices();
        UpdateButtonStates();

        SoundManager.Instance?.PlayClick();
    }

    public void CloseShop()
    {
        if (shopPanel == null || shopWindow == null) return;

        shopPanel.blocksRaycasts = false;
        shopPanel.interactable = false;

        // Unblock board input
        boardManager?.SetUIBlocking(false);

        shopPanel.DOFade(0f, 0.2f);
        shopWindow.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                shopPanel.gameObject.SetActive(false);
            });

        SoundManager.Instance?.PlayClick();
    }

    // ===== BUY ITEMS =====

    public void OnBuyHint()
    {
        if (ItemManager.Instance == null) { return; }

        if (ItemManager.Instance.BuyHint())
        {
            OnPurchaseSuccess("Hint");
            hudController?.UpdateItemCounts();
        }
        else
        {
            OnPurchaseFailed();
        }
    }

    public void OnBuyHammer()
    {
        if (ItemManager.Instance == null) { return; }

        if (ItemManager.Instance.BuyHammer())
        {
            OnPurchaseSuccess("Hammer");
            hudController?.UpdateItemCounts();
        }
        else
        {
            OnPurchaseFailed();
        }
    }

    public void OnBuyColorBomb()
    {
        if (ItemManager.Instance == null) { return; }

        if (ItemManager.Instance.BuyColorBomb())
        {
            OnPurchaseSuccess("Color Bomb");
            hudController?.UpdateItemCounts();
        }
        else
        {
            OnPurchaseFailed();
        }
    }

    // ===== PURCHASE FEEDBACK =====

    private void OnPurchaseSuccess(string itemName)
    {
        SoundManager.Instance?.PlayClick();
        UpdateButtonStates();
    }

    private void OnPurchaseFailed()
    {
        SoundManager.VibrateIfEnabled();

        // Shake window
        if (shopWindow != null)
        {
            shopWindow.DOShakePosition(0.3f, 10f, 20, 90, false, true);
        }
    }

    // ===== UPDATE UI =====

    private void UpdatePrices()
    {
        if (ItemManager.Instance == null) return;

        if (hintPriceText != null)
            hintPriceText.text = ItemManager.Instance.GetHintPrice().ToString();

        if (hammerPriceText != null)
            hammerPriceText.text = ItemManager.Instance.GetHammerPrice().ToString();

        if (colorBombPriceText != null)
            colorBombPriceText.text = ItemManager.Instance.GetColorBombPrice().ToString();
    }

    private void UpdateButtonStates()
    {
        if (CurrencyManager.Instance == null || ItemManager.Instance == null) return;

        int currentCoins = CurrencyManager.Instance.GetCoins();

        // Hint button
        UpdateButtonState(hintBuyButton, currentCoins >= ItemManager.Instance.GetHintPrice());

        // Hammer button
        UpdateButtonState(hammerBuyButton, currentCoins >= ItemManager.Instance.GetHammerPrice());

        // Color Bomb button
        UpdateButtonState(colorBombBuyButton, currentCoins >= ItemManager.Instance.GetColorBombPrice());
    }

    private void UpdateButtonState(Button button, bool canBuy)
    {
        if (button == null) return;

        // Luôn cho phép click để khi thiếu tiền vẫn có feedback (rung/shake).
        // Việc mua thành công/ thất bại được quyết định trong OnBuyXXX() (ItemManager.BuyXXX()).
        button.interactable = true;

        // Change color
        var colors = button.colors;
        colors.normalColor = canBuy ? canBuyColor : cannotBuyColor;
        colors.disabledColor = cannotBuyColor;
        button.colors = colors;
    }

    private void OnCoinsChanged(int newAmount)
    {
        UpdateButtonStates();
    }
}