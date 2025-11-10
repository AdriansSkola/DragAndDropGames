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
}
