using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Users;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Game Menu")]
    [SerializeField] private GameObject title = null;
    [SerializeField] private GameObject creditPanel = null;
    [SerializeField] private GameObject buttonPanel = null;
    [SerializeField] private GameObject controlPanel = null;
    [SerializeField] private Button controlButton = null;
    [SerializeField] private Button backButton = null;

    [Header("UI Sprites")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite xboxSprite;
    [SerializeField] private Sprite playstationSprite;
    [SerializeField] private Sprite keyboardSprite;

    private IEnumerator credits = null;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Application.targetFrameRate = 60;
            credits = Credits();
            Debug.Log("Capping FPS at " + Application.targetFrameRate);
        }

        if (SceneManager.GetActiveScene().buildIndex == 0 && GameManager.gMan.gameCompleted)
        {
            BeginCredits();
            GameManager.gMan.gameCompleted = false;
        }
    }

    #region Update UI Sprites Based On Input Device

    void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            PlayerInput playerInput = PlayerManager.pMan.player.GetComponent<PlayerInput>();
            UpdateButtonUI(playerInput.currentControlScheme);
        }
    }

    void OnEnable()
    {
        InputUser.onChange += OnInputDeviceChange;
    }

    void OnDisable()
    {
        InputUser.onChange -= OnInputDeviceChange;
    }

    void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
            UpdateButtonUI(user.controlScheme.Value.name);
    }

    void UpdateButtonUI(string schemeName)
    {
        if (schemeName.Equals("Xbox"))
            buttonImage.sprite = xboxSprite;
        else if (schemeName.Equals("Playstation"))
            buttonImage.sprite = playstationSprite;
        else
            buttonImage.sprite = keyboardSprite;
    }

    #endregion

    public void GoToScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            Time.timeScale = 1f; // remember you cannot call 'CheckPause' because that happens on a button press, so for now, copy over the logic
        }
        SceneManager.LoadScene(sceneName);
    }

    public void Controls()
    {
        buttonPanel.SetActive(false);
        controlPanel.SetActive(true);
        backButton.Select();
    }

    public void Back()
    {
        controlPanel.SetActive(false);
        buttonPanel.SetActive(true);
        controlButton.Select();
    }

    public void RestartLevel()
    {
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    public void BeginCredits()
    {
        StartCoroutine(credits);
    }

    public IEnumerator Credits()
    {
        // Deactivate the UI
        title.SetActive(false);
        buttonPanel.SetActive(false);
        creditPanel.SetActive(true);

        // Play the music
        FindObjectOfType<AudioManager>().Stop("Theme");
        FindObjectOfType<AudioManager>().Play("Theme-Ending");

        yield return new WaitForSeconds(55f);

        // Activate the UI
        creditPanel.SetActive(false);
        title.SetActive(true);
        buttonPanel.SetActive(true);

        // Stop the music
        FindObjectOfType<AudioManager>().Stop("Theme-Ending");
        FindObjectOfType<AudioManager>().Play("Theme");
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}