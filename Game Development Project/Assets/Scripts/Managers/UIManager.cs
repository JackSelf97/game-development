using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Application.targetFrameRate = 60;
            Debug.Log("Capping FPS at " + Application.targetFrameRate);
        }
    }

    public void GoToScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            Time.timeScale = 1f; // remember you cannot call 'CheckPause' because that happens on a button press, so for now, copy over the logic
        }
            
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}