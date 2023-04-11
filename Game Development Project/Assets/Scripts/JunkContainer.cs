using UnityEngine;
using UnityEngine.UI;

public class JunkContainer : MonoBehaviour
{
    public GameObject instantiatedJunk = null;
    public int currAmmo = 0;
    public int minCapacity = 0, maxCapacity = 20;

    // UI
    private Text ammoText = null;
    private GameObject canvas = null;
    private const int zero = 0, one = 1;

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxCapacity;
        canvas = transform.GetChild(one).gameObject;
        ammoText = canvas.transform.GetChild(zero).GetComponent<Text>();
        ammoText.text = currAmmo.ToString() + "/" + maxCapacity.ToString();

        // Set the event camera
        Canvas containerCanvas = canvas.GetComponent<Canvas>();
        containerCanvas.worldCamera = Camera.main;
    }

    public void UpdateContainerAmmo(int value)
    {
        currAmmo += value;
        ammoText.text = currAmmo.ToString() + "/" + maxCapacity.ToString();
    }
}