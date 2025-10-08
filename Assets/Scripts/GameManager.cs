using UnityEngine;
using UnityEngine.SceneManagement; // <- nepieciešams ainai pārlādēšanai

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public TimerScript timer;
    public GameObject winPanel;
    public GameObject losePanel;
    public TMPro.TextMeshProUGUI winTimeText;
    public TMPro.TextMeshProUGUI starsText;
    public ObjectScript objectScript;

    [Header("Vehicles")]
    public int totalVehicles;
    private int placedVehicles = 0;
    private bool gameEnded = false;

    [Header("Star thresholds (sekundēs)")]
    public float threeStarThreshold = 60f;
    public float twoStarThreshold = 85f;
    public float oneStarThreshold = 120f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        gameEnded = false;
        placedVehicles = 0;
    }

    public void OnVehiclePlaced()
    {
        if (gameEnded) return;

        placedVehicles++;
        if (placedVehicles >= totalVehicles)
        {
            Win();
        }
    }

    public void OnVehicleDestroyed()
    {
        objectScript.effects.PlayOneShot(objectScript.audioCli[17], 1f);
        if (gameEnded) return;

        gameEnded = true;
        if (timer != null) timer.StopTimer();

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    void Win()
    {
        gameEnded = true;
        objectScript.effects.PlayOneShot(objectScript.audioCli[16], 2f);
        if (timer != null) timer.StopTimer();

        if (winPanel != null) winPanel.SetActive(true);
        if (winTimeText != null) winTimeText.text = "Time: " + timer.GetFormattedTime();

        float finalTime = timer.GetElapsedTime();
        int stars = CalculateStars(finalTime);
        if (starsText != null) starsText.text = stars.ToString() + "/3 Stars";

    }

    int CalculateStars(float time)
    {
        if (time <= threeStarThreshold) return 3;
        if (time <= twoStarThreshold) return 2;
        if (time <= oneStarThreshold) return 1;
        return 0;
    }

    // =====================
    // 🔁 UI pogu metodes
    // =====================

    public void RestartGame()
    {
        // Ielādē pašreizējo ainu no jauna
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        // Ielādē sākuma izvēlni
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitToStartMenu()
    {
        // Ielādē “StartMenu” ainu
        SceneManager.LoadScene("StartMenu");
    }
}
