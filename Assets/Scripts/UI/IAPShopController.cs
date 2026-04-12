using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class IAPShopController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup shopPanel;
    [SerializeField] private RectTransform shopWindow;

    [Header("Price Texts")]
    [SerializeField] private TextMeshProUGUI removeAdsPriceText;
    [SerializeField] private TextMeshProUGUI itemPackPriceText;
    [SerializeField] private TextMeshProUGUI coinPackPriceText;

    [Header("Remove Ads Button")]
    [SerializeField] private Button removeAdsBanner;

    void Start()
    {
        if (removeAdsPriceText) removeAdsPriceText.text = "49.000 VND";
        if (itemPackPriceText) itemPackPriceText.text = "19.000 VND";
        if (coinPackPriceText) coinPackPriceText.text = "24.000 VND";

        if (SaveSystem.IsAdsRemoved() && removeAdsBanner != null)
            removeAdsBanner.gameObject.SetActive(false);

        if (shopPanel != null)
            shopPanel.gameObject.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.gameObject.SetActive(true);
        shopPanel.alpha = 0;
        shopWindow.localScale = Vector3.zero;
        shopPanel.blocksRaycasts = true;
        shopPanel.interactable = true;

        shopPanel.DOFade(1f, 0.3f);
        shopWindow.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        SoundManager.Instance?.PlayClick();
    }

    public void CloseShop()
    {
        shopPanel.blocksRaycasts = false;
        shopPanel.interactable = false;

        shopPanel.DOFade(0f, 0.2f);
        shopWindow.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => shopPanel.gameObject.SetActive(false));

        SoundManager.Instance?.PlayClick();
    }

    public void OnBuyRemoveAds()
    {
        SoundManager.Instance?.PlayClick();
        if (IAPManager.Instance != null)
            IAPManager.Instance.BuyRemoveAds();
        else
            SimulateBuy("remove_ads");
    }

    public void OnBuyItemPack()
    {
        SoundManager.Instance?.PlayClick();
        if (IAPManager.Instance != null)
            IAPManager.Instance.BuyItemPack();
        else
            SimulateBuy("item_pack");
    }

    public void OnBuyCoinPack()
    {
        SoundManager.Instance?.PlayClick();
        if (IAPManager.Instance != null)
            IAPManager.Instance.BuyCoinPack();
        else
            SimulateBuy("coin_pack");
    }

    void SimulateBuy(string productId)
    {
        IAPManager.Instance?.SimulatePurchase(productId);
    }
}