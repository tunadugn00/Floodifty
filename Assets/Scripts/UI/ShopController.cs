using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shop Controller - KHÔNG PAUSE GAME!
/// </summary>
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
        else
            Debug.LogError("[Shop] hintBuyButton is NULL! Chưa assign trong Inspector.");

        if (hammerBuyButton != null)
            hammerBuyButton.onClick.AddListener(OnBuyHammer);
        else
            Debug.LogError("[Shop] hammerBuyButton is NULL!");

        if (colorBombBuyButton != null)
            colorBombBuyButton.onClick.AddListener(OnBuyColorBomb);
        else
            Debug.LogError("[Shop] colorBombBuyButton is NULL!");

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
        else
            Debug.LogError("[Shop] closeButton is NULL!");
    }

    void Start()
    {
        // Subscribe to coin changes
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged += OnCoinsChanged;

        // KHÔNG gọi UpdatePrices ở Start() vì ItemManager singleton chưa chắc ready
        // Close shop at start - ShopPanel GameObject phải active lúc này để Awake/Start chạy
        if (shopPanel != null)
            shopPanel.gameObject.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("ShopController IS running!");
            OnBuyHint();
        }
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
        Debug.Log($"[Shop] Opened! Coins={CurrencyManager.Instance?.GetCoins()} | HintPrice={ItemManager.Instance?.GetHintPrice()} | ItemManager null={ItemManager.Instance == null}");
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
        Debug.Log("[Shop] Closed!");
    }

    // ===== BUY ITEMS =====

    public void OnBuyHint()
    {
        Debug.Log($"[Shop] BUY HINT clicked | ItemManager null={ItemManager.Instance == null} | CurrencyManager null={CurrencyManager.Instance == null} | Coins={CurrencyManager.Instance?.GetCoins()} | Price={ItemManager.Instance?.GetHintPrice()}");

        if (ItemManager.Instance == null) { Debug.LogError("[Shop] ItemManager.Instance is NULL!"); return; }

        if (ItemManager.Instance.BuyHint())
        {
            OnPurchaseSuccess("Hint");
            hudController?.UpdateItemCounts();
        }
        else
        {
            Debug.LogWarning($"[Shop] BuyHint FAILED - Coins={CurrencyManager.Instance?.GetCoins()} Price={ItemManager.Instance?.GetHintPrice()}");
            OnPurchaseFailed();
        }
    }

    public void OnBuyHammer()
    {
        Debug.Log($"[Shop] BUY HAMMER clicked | Coins={CurrencyManager.Instance?.GetCoins()} | Price={ItemManager.Instance?.GetHammerPrice()}");

        if (ItemManager.Instance == null) { Debug.LogError("[Shop] ItemManager.Instance is NULL!"); return; }

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
        Debug.Log($"[Shop] BUY COLORBOMB clicked | Coins={CurrencyManager.Instance?.GetCoins()} | Price={ItemManager.Instance?.GetColorBombPrice()}");

        if (ItemManager.Instance == null) { Debug.LogError("[Shop] ItemManager.Instance is NULL!"); return; }

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
        Debug.Log($"[Shop] Bought {itemName}!");
        SoundManager.Instance?.PlayClick();
        UpdateButtonStates();
    }

    private void OnPurchaseFailed()
    {
        Debug.LogWarning("[Shop] Not enough coins!");

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

        button.interactable = canBuy;

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