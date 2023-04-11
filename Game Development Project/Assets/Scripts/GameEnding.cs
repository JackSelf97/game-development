using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEnding : MonoBehaviour
{
    [SerializeField] private Text countdownText = null;
    public float time = 45f, timeLimit = 0f;

    // Start is called before the first frame update
    void Start()
    {
        countdownText = transform.GetChild(0).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        countdownText.text = time.ToString("F2");
        if (time < timeLimit)
        {
            time = timeLimit;
            PlayerManager.pMan.player.GetComponent<PlayerStats>().TakeDamage(100);
            Debug.Log("End.");
        }
    }
}
