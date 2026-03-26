using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
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
    
    [Header("Watch Ad Buttons")]
    [SerializeField] private Button hintWatchAdButton;
    [SerializeField] private Button hammerWatchAdButton;
    [SerializeField] private Button colorBombWatchAdButton;

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
        if (hintBuyButton != null)
        {
            if (!HasPersistentListener(hintBuyButton.onClick, nameof(OnBuyHint)))
                hintBuyButton.onClick.AddListener(OnBuyHint);
        }

        if (hintWatchAdButton != null)
        {
            if (!HasPersistentListener(hintWatchAdButton.onClick, nameof(OnWatchAdForHint)))
                hintWatchAdButton.onClick.AddListener(OnWatchAdForHint);
        }

        if (hammerBuyButton != null)
        {
            if (!HasPersistentListener(hammerBuyButton.onClick, nameof(OnBuyHammer)))
                hammerBuyButton.onClick.AddListener(OnBuyHammer);
        }

        if (colorBombBuyButton != null)
        {
            if (!HasPersistentListener(colorBombBuyButton.onClick, nameof(OnBuyColorBomb)))
                colorBombBuyButton.onClick.AddListener(OnBuyColorBomb);
        }

        if (hammerWatchAdButton != null)
        {
            if (!HasPersistentListener(hammerWatchAdButton.onClick, nameof(OnWatchAdForHammer)))
                hammerWatchAdButton.onClick.AddListener(OnWatchAdForHammer);
        }

        if (colorBombWatchAdButton != null)
        {
            if (!HasPersistentListener(colorBombWatchAdButton.onClick, nameof(OnWatchAdForColorBomb)))
                colorBombWatchAdButton.onClick.AddListener(OnWatchAdForColorBomb);
        }

        if (closeButton != null)
        {
            if (!HasPersistentListener(closeButton.onClick, nameof(CloseShop)))
                closeButton.onClick.AddListener(CloseShop);
        }
    }

    private static bool HasPersistentListener(UnityEvent unityEvent, string methodName)
    {
        if (unityEvent == null) return false;

        int count = unityEvent.GetPersistentEventCount();
        for (int i = 0; i < count; i++)
        {
            if (unityEvent.GetPersistentMethodName(i) == methodName)
                return true;
        }
        return false;
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


    public void OpenShop()
    {
        if (IsTutorialLevelOne())
        {
            SoundManager.VibrateIfEnabled();
            return;
        }

        if (shopPanel == null || shopWindow == null) return;

        shopPanel.gameObject.SetActive(true);

        shopPanel.alpha = 0;
        shopWindow.localScale = Vector3.zero;

        shopPanel.blocksRaycasts = true;
        shopPanel.interactable = true;

        boardManager?.SetUIBlocking(true);

        shopPanel.DOFade(1f, 0.3f);
        shopWindow.DOScale(1f, 0.5f)
            .SetEase(Ease.OutBack);

        UpdatePrices();
        UpdateButtonStates();

        SoundManager.Instance?.PlayClick();
    }

    private static bool IsTutorialLevelOne()
    {
        if (GameManager.Instance == null || GameManager.Instance.isEndlessMode) return false;
        return PlayerPrefs.GetInt("SelectedLevel", 1) == 1;
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
        if (hintBuyButton != null) hintBuyButton.interactable = false;

        try
        {
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
        finally
        {
            if (hintBuyButton != null) hintBuyButton.interactable = true;
        }
    }

    public void OnWatchAdForHint()
    {
        if (AdsManager.Instance == null || ItemManager.Instance == null) return;
        if (hintWatchAdButton != null) hintWatchAdButton.interactable = false;

        SoundManager.Instance?.PlayClick();
        if (!AdsManager.Instance.CanShowRewardedAd())
        {
            AdsManager.Instance.LoadRewardedAd();
            if (hintWatchAdButton != null) hintWatchAdButton.interactable = true;
            return;
        }

        AdsManager.Instance.ShowRewardedAd(() =>
        {
            ItemManager.Instance.GrantHintFromRewardedAd();
            hudController?.UpdateItemCounts();
            SoundManager.Instance?.PlayAdReward();
            UpdateButtonStates();
            if (hintWatchAdButton != null) hintWatchAdButton.interactable = true;
        });
    }

    public void OnBuyHammer()
    {
        if (ItemManager.Instance == null) { return; }
        if (hammerBuyButton != null) hammerBuyButton.interactable = false;

        try
        {
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
        finally
        {
            if (hammerBuyButton != null) hammerBuyButton.interactable = true;
        }
    }

    public void OnBuyColorBomb()
    {
        if (ItemManager.Instance == null) { return; }
        if (colorBombBuyButton != null) colorBombBuyButton.interactable = false;

        try
        {
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
        finally
        {
            if (colorBombBuyButton != null) colorBombBuyButton.interactable = true;
        }
    }

    public void OnWatchAdForHammer()
    {
        if (AdsManager.Instance == null || ItemManager.Instance == null) return;
        if (hammerWatchAdButton != null) hammerWatchAdButton.interactable = false;

        SoundManager.Instance?.PlayClick();
        if (!AdsManager.Instance.CanShowRewardedAd())
        {
            AdsManager.Instance.LoadRewardedAd();
            if (hammerWatchAdButton != null) hammerWatchAdButton.interactable = true;
            return;
        }

        AdsManager.Instance.ShowRewardedAd(() =>
        {
            ItemManager.Instance.GrantHammerFromRewardedAd();
            hudController?.UpdateItemCounts();
            SoundManager.Instance?.PlayAdReward();
            UpdateButtonStates();
            if (hammerWatchAdButton != null) hammerWatchAdButton.interactable = true;
        });
    }

    public void OnWatchAdForColorBomb()
    {
        if (AdsManager.Instance == null || ItemManager.Instance == null) return;
        if (colorBombWatchAdButton != null) colorBombWatchAdButton.interactable = false;

        SoundManager.Instance?.PlayClick();
        if (!AdsManager.Instance.CanShowRewardedAd())
        {
            AdsManager.Instance.LoadRewardedAd();
            if (colorBombWatchAdButton != null) colorBombWatchAdButton.interactable = true;
            return;
        }

        AdsManager.Instance.ShowRewardedAd(() =>
        {
            ItemManager.Instance.GrantColorBombFromRewardedAd();
            hudController?.UpdateItemCounts();
            SoundManager.Instance?.PlayAdReward();
            UpdateButtonStates();
            if (colorBombWatchAdButton != null) colorBombWatchAdButton.interactable = true;
        });
    }

    // ===== PURCHASE FEEDBACK =====

    private void OnPurchaseSuccess(string itemName)
    {
        SoundManager.Instance?.PlayShopPurchase();
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

        if (hintWatchAdButton != null)
        {
            bool show = AdsManager.Instance != null
                        && currentCoins < ItemManager.Instance.GetHintPrice();
            hintWatchAdButton.gameObject.SetActive(show);
        }

        // Hammer button
        UpdateButtonState(hammerBuyButton, currentCoins >= ItemManager.Instance.GetHammerPrice());
        if (hammerWatchAdButton != null)
        {
            bool show = AdsManager.Instance != null
                        && currentCoins < ItemManager.Instance.GetHammerPrice();
            hammerWatchAdButton.gameObject.SetActive(show);
        }

        // Color Bomb button
        UpdateButtonState(colorBombBuyButton, currentCoins >= ItemManager.Instance.GetColorBombPrice());
        if (colorBombWatchAdButton != null)
        {
            bool show = AdsManager.Instance != null
                        && currentCoins < ItemManager.Instance.GetColorBombPrice();
            colorBombWatchAdButton.gameObject.SetActive(show);
        }
    }

    private void UpdateButtonState(Button button, bool canBuy)
    {
        if (button == null) return;
        button.interactable = true;

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