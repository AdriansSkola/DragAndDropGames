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

        if (adsInitializer == null)
        {
            Debug.LogWarning("AdManager: AdsInitializer not found at Awake. Ads will be initialized when AdsInitializer appears.");
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (adsInitializer != null)
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
        // Create/find the ad objects we need so ads work across all scenes
        if (!turnOffInterstitialAd)
        {
            if (interstitialAd == null)
                interstitialAd = FindFirstObjectByType<InterstitialAd>();

            if (interstitialAd == null)
            {
                // create a persistent InterstitialAd if none exists
                var go = new GameObject("InterstitialAd");
                interstitialAd = go.AddComponent<InterstitialAd>();
                DontDestroyOnLoad(go);
                Debug.Log("AdManager: Created fallback InterstitialAd object.");
            }

            interstitialAd.OnInterstitialAdReady -= HandleInterstitialReady;
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;

            Debug.Log("AdManager: Loading first interstitial ad...");
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds)
        {
            if (rewardedAds == null)
                rewardedAds = FindFirstObjectByType<RewardedAds>();

            if (rewardedAds == null)
            {
                var go2 = new GameObject("RewardedAds");
                rewardedAds = go2.AddComponent<RewardedAds>();
                DontDestroyOnLoad(go2);
                Debug.Log("AdManager: Created fallback RewardedAds object.");
            }

            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAd)
        {
            if (bannerAd == null)
                bannerAd = FindFirstObjectByType<BannerAd>();

            if (bannerAd == null)
            {
                var go3 = new GameObject("BannerAd");
                bannerAd = go3.AddComponent<BannerAd>();
                DontDestroyOnLoad(go3);
                Debug.Log("AdManager: Created fallback BannerAd object.");
            }

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

        // Try to bind buttons robustly (direct tag, children, or search by name). Buttons may be inactive or created later.
        TryBindButtonToAd("InterstitialButton", interstitialAd, (b) => interstitialAd.SetButton(b));

            

        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        TryBindButtonToAd("RewardedButton", rewardedAds, (b) => rewardedAds.SetButton(b));

        if (bannerAd == null)
            bannerAd = FindFirstObjectByType<BannerAd>();

        TryBindButtonToAd("BannerButton", bannerAd, (b) => bannerAd.SetButton(b));
        
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

    // Helper: attempt binding a button found by tag/name/children to a given ad object.
    private void TryBindButtonToAd<T>(string tag, T adObj, System.Action<UnityEngine.UI.Button> onFound) where T : class
    {
        if (adObj == null || onFound == null)
            return;

        // 1) Try direct tag lookup (active objects)
        var go = GameObject.FindGameObjectWithTag(tag);
        UnityEngine.UI.Button button = null;
        if (go != null)
            button = go.GetComponent<UnityEngine.UI.Button>() ?? go.GetComponentInChildren<UnityEngine.UI.Button>();

        if (button != null)
        {
            onFound(button);
            return;
        }

        // 2) Search all Buttons (including inactive) and match by tag or name heuristics
        var allButtons = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsSortMode.None);
        foreach (var b in allButtons)
        {
            if (b == null) continue;
            if (b.gameObject.CompareTag(tag) || b.gameObject.name.IndexOf(tag, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                onFound(b);
                return;
            }
        }

        // 3) Start a short retry coroutine in case the UI is created later (e.g., by another script)
        StartCoroutine(TryBindRetry(tag, adObj, onFound));
    }

    private System.Collections.IEnumerator TryBindRetry<T>(string tag, T adObj, System.Action<UnityEngine.UI.Button> onFound) where T : class
    {
        float timeout = 3f;
        float interval = 0.15f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (adObj == null) yield break;

            var go = GameObject.FindGameObjectWithTag(tag);
            UnityEngine.UI.Button b = null;
            if (go != null)
                b = go.GetComponent<UnityEngine.UI.Button>() ?? go.GetComponentInChildren<UnityEngine.UI.Button>();

            if (b == null)
            {
                var allButtons = UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>(UnityEngine.FindObjectsSortMode.None);
                foreach (var btn in allButtons)
                {
                    if (btn == null) continue;
                    if (btn.gameObject.CompareTag(tag) || btn.gameObject.name.IndexOf(tag, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        b = btn;
                        break;
                    }
                }
            }

            if (b != null)
            {
                onFound(b);
                yield break;
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        Debug.LogWarning($"AdManager: could not bind button with tag/name '{tag}' after retries. Please add a GameObject with tag '{tag}' and a Button component to the scene.");
    }
}
