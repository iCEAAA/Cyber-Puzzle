#if (DEBUG || UNITY_EDITOR)
#define VERBOSE
#endif

using System;
using UnityEngine;
#if USE_ADMOB
using GoogleMobileAds.Api;
#endif

/*
 * Download the package here - https://github.com/googleads/googleads-mobile-unity/releases
 * Get the Ad Units from here - https://www.google.com/admob/
*/

[Serializable]
public enum TestMode
{
    Enabled,
    Disabled
}

public class AdmobManager : MonoBehaviour
{
    internal static AdmobManager instance;

    public TestMode testMode = TestMode.Enabled;

    public GameObject ConsentCanvas;

    public bool EnableInterstitialAds = true, EnableRewardedAds = true;

    [Range(1, 10)]
    public int ShowInterstitialAdAfterXGameovers = 1;

#if UNITY_ANDROID
    public string AndroidAppId = "ENTER_APP_ID_HERE";
    public string AndroidInterstitialId = "ENTER_INTERSTITIAL_ID_HERE";
    public string AndroidRewardedVideoId = "ENTER_REWARDED_VIDEO_ID_HERE";
#elif UNITY_IOS
    public string IosAppId = "ENTER_APP_ID_HERE";
    public string IosInterstitialId = "ENTER_INTERSTITIAL_ID_HERE";
    public string IosRewardedVideoId = "ENTER_REWARDED_VIDEO_ID_HERE";
#endif

#if USE_ADMOB

#if UNITY_ANDROID
    private const string testAppId = "ca-app-pub-3940256099942544~3347511713";
    private const string testInterstitialId = "ca-app-pub-3940256099942544/1033173712";
    private const string testRewardedVideoId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IOS
    private const string testAppId = "ca-app-pub-3940256099942544~1458002511";
    private const string testInterstitialId = "ca-app-pub-3940256099942544/4411468910";
    private const string testRewardedVideoId = "ca-app-pub-3940256099942544~1458002511";
#endif

    private const string PersonalizedAdsKey = "ShowPersonalizedAds";
    private bool PersonalizedAds = false;

    private string ActiveAppId, ActiveInterstitialId, ActiveRewardedVideoId;

    private RewardBasedVideoAd rewardBasedVideo;
    private InterstitialAd interstitial;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        if (testMode == TestMode.Enabled)
        {
            ActiveAppId = testAppId;
            if (EnableInterstitialAds)
                ActiveInterstitialId = testInterstitialId;
            if (EnableRewardedAds)
                ActiveRewardedVideoId = testRewardedVideoId;

            EnableInterstitialAds = true;
            EnableRewardedAds = true;
        }
        else
        {
#if UNITY_ANDROID
            ActiveAppId = AndroidAppId;

            if (EnableInterstitialAds)
                ActiveInterstitialId = AndroidInterstitialId;

            if (EnableRewardedAds)
                ActiveRewardedVideoId = AndroidRewardedVideoId;
#elif UNITY_IOS
            ActiveAppId = IosAppId;

            if (EnableInterstitialAds)
                ActiveInterstitialId = IosInterstitialId;

            if (EnableRewardedAds)
                ActiveRewardedVideoId = IosRewardedVideoId;
#endif
        }

        //Check consent
        CheckConsent();

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(ActiveAppId);

        if (EnableRewardedAds)
        {
            // Get singleton reward based video ad reference.
            rewardBasedVideo = RewardBasedVideoAd.Instance;

            // RewardBasedVideoAd is a singleton, so handlers should only be registered once.

            // Called when an ad request has successfully loaded.
            rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
            // Called when an ad request failed to load.
            rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
            // Called when the ad starts to play.
            rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
            // Called when the user should be rewarded for watching a video.
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
            // Called when the ad click caused the user to leave the application.
            rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;
        }
    }

    //Check if the user has given consent to receive personalized ads
    internal void CheckConsent()
    {
        if (!PlayerPrefs.HasKey(PersonalizedAdsKey))
        {
            RequestConsent();
            return;
        }

        if (ConsentCanvas)
            ConsentCanvas.SetActive(false);

        PersonalizedAds = (PlayerPrefs.GetInt(PersonalizedAdsKey, 0) == 1) ? true : false;
    }

    //Call this to request for consent
    public void RequestConsent()
    {
        if (ConsentCanvas)
            ConsentCanvas.SetActive(true);
    }

    //Call this if the player OPTS INTO receiving personalized ads
    public void AcceptConsent()
    {
        PlayerPrefs.SetInt(PersonalizedAdsKey, 1);
        PlayerPrefs.Save();

        PersonalizedAds = true;

        if (ConsentCanvas)
            ConsentCanvas.SetActive(false);
    }

    //Call this if the player OPTS OUT of receiving personalized ads
    public void DeclineConsent()
    {
        PlayerPrefs.SetInt(PersonalizedAdsKey, 0);
        PlayerPrefs.Save();

        PersonalizedAds = false;

        if (ConsentCanvas)
            ConsentCanvas.SetActive(false);
    }

    // Returns an ad request.
    private AdRequest CreateAdRequest()
    {
        if (PersonalizedAds)
        {
            return new AdRequest.Builder()
                .AddTestDevice(AdRequest.TestDeviceSimulator)
                //.AddTestDevice("123456789")
                //.AddKeyword("game")
                //.SetGender(Gender.Male)
                //.SetBirthday(new DateTime(1985, 1, 1))
                //.TagForChildDirectedTreatment(false)
                .Build();
        }
        else
        {
            return new AdRequest.Builder()
                .AddTestDevice(AdRequest.TestDeviceSimulator)
                //.AddTestDevice("123456789")
                //.AddKeyword("game")
                //.SetGender(Gender.Male)
                //.SetBirthday(new DateTime(1985, 1, 1))
                //.TagForChildDirectedTreatment(false)
                .AddExtra("npa", "1")
                .Build();
        }
    }


    #region Interstitial Ads

    internal void RequestInterstitial()
    {
        if (!EnableInterstitialAds)
            return;

        if (CheckInterstitialStatus())
            return;

        if (interstitial != null)
            DestroyInterstitial();

        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(ActiveInterstitialId); ;

        // Called when an ad request has successfully loaded.
        interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        interstitial.OnAdOpening += HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        interstitial.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        // Load the interstitial with a request.
        interstitial.LoadAd(CreateAdRequest());
    }

    internal void ShowInterstitialAd()
    {
        if (CheckInterstitialStatus())
            interstitial.Show();
    }

    internal bool CheckInterstitialStatus()
    {
        if (interstitial == null)
            return false;

        if (interstitial.IsLoaded())
            return true;
        else
            return false;
    }

    private void DestroyInterstitial()
    {
        if (!EnableInterstitialAds)
            return;

        if (interstitial == null)
            return;

        // Destroy the listeners
        interstitial.OnAdLoaded -= HandleOnAdLoaded;
        interstitial.OnAdFailedToLoad -= HandleOnAdFailedToLoad;
        interstitial.OnAdClosed -= HandleOnAdClosed;
        interstitial.OnAdLeavingApplication -= HandleOnAdLeavingApplication;

        interstitial.Destroy();
    }

    #region Interstitial callback handlers
    private void HandleOnAdLoaded(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleAdLoaded event received");
#endif
    }

    private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
#if VERBOSE
        print("HandleInterstitialFailedToLoad event received with message: " + args.Message);
#endif
    }

    private void HandleOnAdOpened(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleAdOpened event received");
#endif
    }

    private void HandleOnAdClosed(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleInterstitialClosed event received");
#endif
    }

    private void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleInterstitialLeftApplication event received");
#endif
    }
    #endregion
    #endregion

    #region Rewarded Video Ads
    internal void RequestRewardBasedVideo()
    {
        if (!EnableRewardedAds)
            return;

        if (CheckRewardedVideoStatus())
            return;

        // Load the rewarded video ad with a request.
        rewardBasedVideo.LoadAd(CreateAdRequest(), ActiveRewardedVideoId);
    }

    internal void ShowRewardedAd()
    {
        if (CheckRewardedVideoStatus())
            rewardBasedVideo.Show();
    }

    internal bool CheckRewardedVideoStatus()
    {
        if (rewardBasedVideo == null)
            return false;

        if (rewardBasedVideo.IsLoaded())
            return true;
        else
            return false;
    }

    private void DestroyRewardBasedVideo()
    {
        if (!EnableRewardedAds)
            return;

        // Destroy the listeners
        rewardBasedVideo.OnAdLoaded -= HandleRewardBasedVideoLoaded;
        rewardBasedVideo.OnAdFailedToLoad -= HandleRewardBasedVideoFailedToLoad;
        rewardBasedVideo.OnAdOpening -= HandleRewardBasedVideoOpened;
        rewardBasedVideo.OnAdStarted -= HandleRewardBasedVideoStarted;
        rewardBasedVideo.OnAdRewarded -= HandleRewardBasedVideoRewarded;
        rewardBasedVideo.OnAdClosed -= HandleRewardBasedVideoClosed;
        rewardBasedVideo.OnAdLeavingApplication -= HandleRewardBasedVideoLeftApplication;
    }

    #region RewardBasedVideo callback handlers

    private void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoLoaded event received");
#endif
    }

    private void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
#endif
    }

    private void HandleRewardBasedVideoOpened(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoOpened event received");
#endif
    }

    private void HandleRewardBasedVideoStarted(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoStarted event received");
#endif
    }

    private void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoClosed event received");
#endif
    }

    private void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoRewarded event received");
#endif
        //Reward player here
        Controller.instance.IncreaseLives();
    }

    private void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
    {
#if VERBOSE
        print("HandleRewardBasedVideoLeftApplication event received");
#endif
    }
    #endregion
    #endregion

    private void OnDestroy()
    {
        DestroyInterstitial();
        DestroyRewardBasedVideo();
    }
#else
    private void Awake()
    {
        if (ConsentCanvas)
            ConsentCanvas.SetActive(false);
    }
#endif
}