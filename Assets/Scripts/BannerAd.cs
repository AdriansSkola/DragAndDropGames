using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _androidAdUnitId = "Banner_Android";
    string _adUnitId;
    [SerializeField] Button _bannerButton;
    public bool isBannerVisible = false;
    private bool isLoaded = false;
    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
        Advertisement.Banner.SetPosition(_bannerPosition);
    }

    public void LoadBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load banner ad before Unity ads was initialized.");
            return;
        }
        Debug.Log("Loading banner ad...");
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId, options);
    }

    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded!");
        isLoaded = true;
        if (_bannerButton != null)
        {
            _bannerButton.interactable = true;
        }
        else
        {
            Debug.LogWarning("BannerAd: OnBannerLoaded called but no _bannerButton is assigned. Set a button with tag 'BannerButton' or assign in inspector.");
        }
        // Auto-show the banner immediately after it has been loaded so banners appear across scenes
        BannerOptions showOptions = new BannerOptions
        {
            showCallback = OnBannerShown,
            hideCallback = OnBannerHidden,
            clickCallback = OnBannerClicked
        };

        Debug.Log("BannerAd: Showing banner after load.");
        Advertisement.Banner.Show(_adUnitId, showOptions);
    }

    // Call this if you want to ensure a banner is visible (no-op if already visible)
    public void EnsureBannerShown()
    {
        if (isBannerVisible)
            return;

        if (!isLoaded)
        {
            Debug.Log("BannerAd: not loaded yet, will load and show.");
            LoadBanner();
            return;
        }

        BannerOptions options = new BannerOptions
        {
            showCallback = OnBannerShown,
            hideCallback = OnBannerHidden,
            clickCallback = OnBannerClicked
        };

        Advertisement.Banner.Show(_adUnitId, options);
    }

    void OnBannerError(string message)
    {
        Debug.LogWarning("Failed to load banner ad: " + message);
        LoadBanner();
    }

    public void ShowBannerAd()
    {
        if (isBannerVisible)
        {
            HideBannerAd();

        }
        else
        {
            BannerOptions options = new BannerOptions
            {
                showCallback = OnBannerShown,
                hideCallback = OnBannerHidden,
                clickCallback = OnBannerClicked
            };

            Advertisement.Banner.Show(_adUnitId, options);
        }
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    void OnBannerShown()
    {
        Debug.Log("Banner ad shown!");
        isBannerVisible = true;
    }

    void OnBannerHidden()
    {
        Debug.Log("Banner ad hidden!");
        isBannerVisible = false;
    }

    void OnBannerClicked()
    {
        Debug.Log("User clicked on banner ad!");
    }

    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowBannerAd);
        _bannerButton = button;
        _bannerButton.interactable = isLoaded;
    }
}
