using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    const string PRODUCT_REMOVE_ADS = "remove_ads";
    const string PRODUCT_COIN_PACK = "coin_pack";
    const string PRODUCT_ITEM_PACK = "item_pack";

    const int COIN_PACK_AMOUNT = 5000;
    const int ITEM_PACK_AMOUNT = 5; 

    private IStoreController _storeController;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InitializePurchasing();
    }

    void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(PRODUCT_REMOVE_ADS, ProductType.NonConsumable);
        builder.AddProduct(PRODUCT_COIN_PACK, ProductType.Consumable);
        builder.AddProduct(PRODUCT_ITEM_PACK, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    // ── Mua hàng ──────────────────────────────────────
    public void BuyRemoveAds() => BuyProduct(PRODUCT_REMOVE_ADS);
    public void BuyCoinPack() => BuyProduct(PRODUCT_COIN_PACK);
    public void BuyItemPack() => BuyProduct(PRODUCT_ITEM_PACK);

    void BuyProduct(string productId)
    {
        if (_storeController == null)
        {
            Debug.LogWarning("[IAP] Store chưa init!");
            SimulatePurchase(productId);
            return;
        }
        _storeController.InitiatePurchase(productId);
    }

    // ── Callbacks ─────────────────────────────────────
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        Debug.Log("[IAP] Init OK");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning($"[IAP] Init failed: {error}");
    }
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning($"[IAP] Init failed: {error} - {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        GrantReward(args.purchasedProduct.definition.id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} - {reason}");
    }


    // ── Grant reward ──────────────────────────────────
    void GrantReward(string productId)
    {
        switch (productId)
        {
            case PRODUCT_REMOVE_ADS:
                SaveSystem.SetAdsRemoved(true);
                AdsManager.Instance?.HideBannerAd();
                Debug.Log("[IAP] Ads removed!");
                break;

            case PRODUCT_COIN_PACK:
                CurrencyManager.Instance?.AddCoins(COIN_PACK_AMOUNT);
                Debug.Log($"[IAP] +{COIN_PACK_AMOUNT} coins!");
                break;

            case PRODUCT_ITEM_PACK:
                ItemManager.Instance?.AddItems(ITEM_PACK_AMOUNT);
                Debug.Log($"[IAP] +{ITEM_PACK_AMOUNT} mỗi loại item!");
                break;
        }
    }

    // ── Fake purchase khi test không có store ─────────
    public void SimulatePurchase(string productId)
    {
        Debug.Log($"[IAP] Simulate purchase: {productId}");
        GrantReward(productId);
    }
}