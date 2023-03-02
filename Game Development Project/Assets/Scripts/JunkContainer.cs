using UnityEngine;
using UnityEngine.UI;

public class JunkContainer : MonoBehaviour
{
    public int currAmmo = 0;
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] private Text ammoText = null;

    // Start is called before the first frame update
    void Start()
    {
        currAmmo = maxCapacity;
        ammoText = transform.GetChild(1).transform.GetChild(0).GetComponent<Text>();
        ammoText.text = currAmmo.ToString() + "/" + maxCapacity.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateContainerAmmo(int value = 0)
    {
        currAmmo += value;
        ammoText.text = currAmmo.ToString() + "/" + maxCapacity.ToString();
    }
}
