using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;
    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;
    // How many moves to remove from TowerManager when the player watches a rewarded ad
    [SerializeField] int movesReductionAmount = 4;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;

        if (flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
    }

    private bool isLoaded = false;

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity ads was initialized.");
            return;
        }
        Debug.Log("Loading rewarded ad...");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewarded ad loaded!");
        if (placementId.Equals(_adUnitId))
        {
            isLoaded = true;
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = true;
            }
            else
            {
                Debug.LogWarning("RewardedAds: ad loaded but no _rewardedAdButton assigned. Button will be enabled if assigned later.");
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning("Failed to load rewarded ad!");
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning("Failed to show rewarded ad!");
        StartCoroutine(WaitAndLoad(5f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad!");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // Defensive null checks: placementId can be null in some callback edge cases.
        if (!string.IsNullOrEmpty(placementId) && placementId == _adUnitId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("Rewarded ad completed!");

            if (flyingObjectManager != null)
            {
                flyingObjectManager.DestroyAllFlyingObjects();
            }
            else
            {
                Debug.LogWarning("RewardedAds: flyingObjectManager is null, cannot destroy flying objects.");
            }

            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = false;
            }
            else
            {
                Debug.LogWarning("RewardedAds: _rewardedAdButton is null.");
            }

            StartCoroutine(WaitAndLoad(10f));

            // If a TowerManager exists in the scene, reduce moves there by the configured amount
            var towerManager = FindFirstObjectByType<TowerManager>();
            if (towerManager != null)
            {
                towerManager.ReduceMoves(movesReductionAmount);
                Debug.Log($"RewardedAds: applied move reduction of {movesReductionAmount} to TowerManager.");
            }
            else
            {
                Debug.LogWarning("RewardedAds: TowerManager not found in scene; cannot apply move reduction.");
            }
        }
        else
        {
            Debug.Log($"RewardedAds: OnUnityAdsShowComplete called for placement '{placementId}' with state {showCompletionState}.");
        }

        // Always restore time scale (defensive in case Start/Show changed it).
        Time.timeScale = 1f;
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        // If the ad was already loaded earlier, make the button interactable immediately
        _rewardedAdButton.interactable = isLoaded;
    }
    
    public void ShowAd()
    {
        _rewardedAdButton.interactable = false;
        Advertisement.Show(_adUnitId, this);
    }
}
