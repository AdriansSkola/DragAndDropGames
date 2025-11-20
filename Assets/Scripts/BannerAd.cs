using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _androidAdUnitId = "Banner_Android";
    string _adUnitId;
    [SerializeField] Button _bannerButton;
    public bool isBannerVisible = false;
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
        _bannerButton.interactable = true;
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
        _bannerButton.interactable = false;
    }
}
