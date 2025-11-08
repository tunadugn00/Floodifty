using GoogleMobileAds.Api;
using System;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    //--- ID Ads ---
    // google ID test
#if UNITY_ANDROID
    private string _bannerId = "ca-app-pub-3940256099942544/6300978111";
    private string _interstitialId = "ca-app-pub-3940256099942544/1033173712";
    private string _rewardedId = "ca-app-pub-3940256099942544/5224354917";

#else 
    private string _bannerId = "unused";
    private string _interstitialId = "unused";
    private string _rewardedId = "unused";
#endif

    // Các đối tượng quảng cáo
    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;

    //Callback cho quảng cáo có thưởng
    public static Action OnUserEarnedReward;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Khởi tạo AdMob SDK
        MobileAds.Initialize((InitializationStatus status) =>
        {
            Debug.Log("AdMob SDK Initialized");

            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    #region --- Banner Ads ----
    public void LoadBannerAd()
    {
        Debug.Log("Loading Banner Ad....");
        //Tạo request
        AdRequest request = new AdRequest();
        //Tạo banner
        _bannerView = new BannerView(_bannerId, AdSize.Banner, AdPosition.Bottom);
        _bannerView.LoadAd(request);
        _bannerView.Show();
    }

    public void HideBannerAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
        }
    }
    #endregion
    #region --- Interstitial Ads ----
    public void LoadInterstitialAd()
    {
        Debug.Log("Loading Interstitial Ad...");
        AdRequest request = new AdRequest();

        //tải ads
        InterstitialAd.Load(_interstitialId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if( error != null || ad == null)
            {
                Debug.LogError("Interstitial ad failed to load: " + error);
                return;
            }
            _interstitialAd = ad;
            Debug.Log("Interstitial Ad loaded.");
            RegisterInterstitialEvents(ad);
        });
    }

    public void ShowInterstitialAd()
    {
        if(_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing Interstitial Ad.");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
            LoadInterstitialAd();
        }
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial Ad closed.");
            _interstitialAd.Destroy();
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to show: " + error.GetMessage());
            _interstitialAd.Destroy();
            LoadInterstitialAd();
        };
    }
    #endregion

    #region --- Rewarded Ads ---
    public void LoadRewardedAd()
    {
        Debug.Log("Loading Rewarded Ad...");
        AdRequest request = new AdRequest();

        RewardedAd.Load(_rewardedId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed to load: " + error);
                return;
            }

            _rewardedAd = ad;
            Debug.Log("Rewarded Ad loaded.");

            // Đăng ký sự kiện
            RegisterRewardedEvents(ad);
        });
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            Debug.Log("Showing Rewarded Ad.");
            _rewardedAd.Show((Reward reward) =>
            {
                //trả thưởng
                Debug.Log("User earned reward! Type: " + reward.Type + ", Amount: " + reward.Amount);
                OnUserEarnedReward?.Invoke();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
            LoadRewardedAd();
        }
    }
    private void RegisterRewardedEvents(RewardedAd ad)
    {
        //Khi ads bị đóng
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad closed.");
            _rewardedAd.Destroy();
            LoadRewardedAd();
        };

        //khi lỗi show
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to show: " + error.GetMessage());
            _rewardedAd.Destroy();
            LoadRewardedAd();
        };
    }
    #endregion
}
