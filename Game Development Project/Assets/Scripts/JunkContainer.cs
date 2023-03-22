using UnityEngine;
using UnityEngine.UI;

public class JunkContainer : MonoBehaviour
{
    public int currAmmo = 0;
    public int minCapacity = 0, maxCapacity = 10;
    private Text ammoText = null;

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxCapacity;
        ammoText = transform.GetChild(1).transform.GetChild(0).GetComponent<Text>();
        ammoText.text = currAmmo.ToString() + "/" + maxCapacity.ToString();
    }

    public void UpdateContainerAmmo(int value)
    {
        currAmmo += value;
        ammoText.text = currAmmo.ToString() + "/" + maxCapacity.ToString();
    }
}