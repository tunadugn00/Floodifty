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

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;

    public static Action OnUserEarnedReward;

    private Action _onRewardedGranted;

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
        MobileAds.Initialize((InitializationStatus status) =>
        {
            LoadBannerAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    #region --- Banner Ads ----
    public void LoadBannerAd()
    {
        AdRequest request = new AdRequest();
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
        AdRequest request = new AdRequest();
        InterstitialAd.Load(_interstitialId, request, (InterstitialAd ad, LoadAdError error) =>
        {
            if( error != null || ad == null)
            {
                return;
            }
            _interstitialAd = ad;
            RegisterInterstitialEvents(ad);
        });
    }

    public void ShowInterstitialAd()
    {
        if(_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
        }
        else
        {
            LoadInterstitialAd();
        }
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            _interstitialAd.Destroy();
            LoadInterstitialAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            _interstitialAd.Destroy();
            LoadInterstitialAd();
        };
    }
    #endregion

    #region --- Rewarded Ads ---
    public bool CanShowRewardedAd()
    {
        return _rewardedAd != null && _rewardedAd.CanShowAd();
    }

    public void LoadRewardedAd()
    {
        AdRequest request = new AdRequest();

        RewardedAd.Load(_rewardedId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                return;
            }
            _rewardedAd = ad;

            RegisterRewardedEvents(ad);
        });
    }

    public void ShowRewardedAd()
    {
        ShowRewardedAd(null);
    }

    public void ShowRewardedAd(Action onRewardGranted)
    {
        _onRewardedGranted = onRewardGranted;

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                if (_onRewardedGranted != null)
                {
                    var cb = _onRewardedGranted;
                    _onRewardedGranted = null;
                    cb.Invoke();
                }
                else
                {
                    OnUserEarnedReward?.Invoke();
                }
            });
        }
        else
        {
            _onRewardedGranted = null;
            LoadRewardedAd();
            SoundManager.VibrateIfEnabled();
        }
    }
    private void RegisterRewardedEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            _rewardedAd.Destroy();
            LoadRewardedAd();
        };
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            _rewardedAd.Destroy();
            LoadRewardedAd();
        };
    }
    #endregion
}
