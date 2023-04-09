using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Users;

public class UIManager : MonoBehaviour
{
    [Header("Game Menu")]
    [SerializeField] private GameObject buttonPanel = null;
    [SerializeField] private GameObject controlPanel = null;
    [SerializeField] private Button controlButton = null;
    [SerializeField] private Button backButton = null;

    [Header("UI")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite gamepadSprite;
    [SerializeField] private Sprite keyboardSprite;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Application.targetFrameRate = 60;
            Debug.Log("Capping FPS at " + Application.targetFrameRate);
        }
    }

















    void Awake()
    {
        PlayerInput playerInput = PlayerManager.pMan.player.GetComponent<PlayerInput>();
        UpdateButtonUI(playerInput.currentControlScheme);
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
        {
            UpdateButtonUI(user.controlScheme.Value.name);
        }
    }

    void UpdateButtonUI(string schemeName)
    {
        // assuming you have only 2 schemes: keyboard and gamepad
        if (schemeName.Equals("Gamepad"))
        {
            buttonImage.sprite = gamepadSprite;
        }
        else
        {
            buttonImage.sprite = keyboardSprite;
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

    public void Resume()
    {
        
    }

    public void RestartLevel()
    {
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}