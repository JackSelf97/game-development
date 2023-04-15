using UnityEngine;
using UnityEngine.UI;

public class GameEnding : MonoBehaviour
{
    [SerializeField] private Text countdownText = null;
    public float time = 45f, timeLimit = 0f;
    private PlayerStats playerStats = null;

    // Start is called before the first frame update
    void Start()
    {
        countdownText = transform.GetChild(0).GetComponent<Text>();
        playerStats = PlayerManager.pMan.player.GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStats.currHP > 0)
        {
            time -= Time.deltaTime;
            countdownText.text = time.ToString("F2");
            if (time < timeLimit)
            {
                time = timeLimit;
                playerStats.TakeDamage(100);
                Debug.Log("End.");
            }
        }
    }
}