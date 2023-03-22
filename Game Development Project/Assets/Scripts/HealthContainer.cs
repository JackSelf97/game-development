using UnityEngine;
using UnityEngine.UI;

public class HealthContainer : MonoBehaviour
{
    public int uses = 1;
    private Text displayText = null;

    // Start is called before the first frame update
    void Start()
    {
        displayText = transform.GetChild(1).transform.GetChild(0).GetComponent<Text>();
        displayText.text = "MEDICAL CARE";
    }

    public void UpdateContainerStatus(int value)
    {
        uses += value;
        displayText.text = "OFFLINE";
        displayText.color = Color.red;
    }
}
