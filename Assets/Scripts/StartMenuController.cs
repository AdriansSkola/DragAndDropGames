using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("City Scene");
    }

    public void OnOtherClick()
    {
        SceneManager.LoadScene("Tower");
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        // Ielādē pašreizējo ainu no jauna
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToStartMenu()
    {
        // Ielādē “StartMenu” ainu
        SceneManager.LoadScene("StartMenu");
    }

}
