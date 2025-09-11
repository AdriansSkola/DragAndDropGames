using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene("City Scene");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}
