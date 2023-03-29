using UnityEngine;
using UnityEngine.UI;

public class HealthContainer : MonoBehaviour
{
    public int uses = 1;
    private Text displayText = null;
    private const int zero = 0, one = 1;

    // Start is called before the first frame update
    void Start()
    {
        displayText = transform.GetChild(one).transform.GetChild(zero).GetComponent<Text>();
        displayText.text = "HEALTH";
    }

    public void UpdateContainerStatus(int value)
    {
        uses += value;
        displayText.text = "OFFLINE";
        displayText.color = Color.red;
    }
}