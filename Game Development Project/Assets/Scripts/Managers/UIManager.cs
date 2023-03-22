using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            Application.targetFrameRate = 60;
            Debug.Log("Capping FPS at " + Application.targetFrameRate);
        }
    }

    public void GoToScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name != "Main Menu")
            Time.timeScale = 1f; // remember you cannot call 'CheckPause' because that happens on a button press, so for now, copy over the logic

        SceneManager.LoadScene(sceneName);
        Debug.Log("Loading scene: " + sceneName);
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}