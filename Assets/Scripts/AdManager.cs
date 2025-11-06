using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstAdShown = false;

    // .......

    public static AdManager Instance { get; private set; }


    private void Awake()
    {
        if(adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitializer>();

        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if(!turnOffInterstitialAd)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing first interstitial ad");
            interstitialAd.ShowAd();
            firstAdShown = true;
        }
        else
        {
            Debug.Log("Next Interstitial ad is ready for manual showing");
        }
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool firstSceneLoad = false;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        Button interstitialButton = GameObject.FindGameObjectWithTag("InterstitialButton").GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }

        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("First time scene Loaded!");
            return;
        }

        Debug.Log("Scene loaded!");
        HandleAdsInitialized();
    }
}