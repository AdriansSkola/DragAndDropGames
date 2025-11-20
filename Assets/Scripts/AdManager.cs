using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;

    private bool firstAdShown = false;
    private bool firstSceneLoad = false;
    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAd = false;

    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        if (adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitializer>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleAdsInitialized()
    {
        if (turnOffInterstitialAd)
            return;

        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        if (interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady;
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;

            Debug.Log("Loading first interstitial ad...");
            interstitialAd.LoadAd();
        }
        else
        {
            Debug.LogWarning("InterstitialAd reference not found in scene!");
        }

        if (!turnOffRewardedAds)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAd)
        {
            bannerAd.LoadBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        // Show ad only once when the game starts
        if (!firstAdShown)
        {
            Debug.Log("Showing first interstitial ad on startup.");
            interstitialAd.ShowAd();
            firstAdShown = true;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        Button interstitialButton = null;
        GameObject buttonObj = GameObject.FindGameObjectWithTag("InterstitialButton");
        if (buttonObj != null)
            interstitialButton = buttonObj.GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
            interstitialAd.SetButton(interstitialButton);

            

        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        Button rewardedAdButton = GameObject.FindGameObjectWithTag("RewardedButton")?.GetComponent<Button>();

        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        Button bannerButton = GameObject.FindGameObjectWithTag("BannerButton")?.GetComponent<Button>();

        if (bannerAd != null && bannerButton != null)
        {
            bannerAd.SetButton(bannerButton);
        }
        
        // Skip first load — ad already shown at startup
        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("Initial scene load complete (ad already shown).");
            return;
        }

        // From now on, every scene change shows an ad if ready
        Debug.Log($"Scene changed to: {scene.name}. Attempting to show ad...");

        if (!turnOffInterstitialAd && interstitialAd != null)
        {
            if (interstitialAd.isReady)
            {
                Debug.Log("Ad ready — showing now.");
                interstitialAd.ShowAd();      // Show immediately
                interstitialAd.LoadAd();      // Preload next one
            }
            else
            {
                Debug.Log("Ad not ready — loading new one for next scene.");
                interstitialAd.LoadAd();      // Load if missing
            }
        }
    }
}
