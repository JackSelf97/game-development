using UnityEngine;

public class Shrink : MonoBehaviour
{
    public static Shrink sMan { get; private set; }
    private float shrinkMultiplier = 1f, growMultiplier = 6f;
    private float xScale, yScale, zScale;

    // Start is called before the first frame update
    void Start()
    {
        sMan = this;
    }

    public void ShrinkItem(GameObject item, bool destroy = false, float lifeTime = 0f) // this function needs to be called in Update()
    {
        // Set a constant
        const int zero = 0;

        // Set the size
        xScale = item.transform.localScale.x; 
        yScale = item.transform.localScale.y;
        zScale = item.transform.localScale.z;

        xScale -= shrinkMultiplier * Time.deltaTime;
        yScale -= shrinkMultiplier * Time.deltaTime;
        zScale -= shrinkMultiplier * Time.deltaTime;
        item.transform.localScale = new Vector3(xScale, yScale, zScale); // update the size

        if (xScale <= zero)
        {
            item.transform.localScale = Vector3.zero;

            // Destroy or SetActive(false) ??
            if (!destroy)
                item.SetActive(false);
            else
                Destroy(item, lifeTime);
        }
    }

    public void GrowItem(GameObject item)
    {
        // Get the script
        Junk junkScript = item.GetComponent<Junk>();

        // Set the size
        xScale = item.transform.localScale.x;
        yScale = item.transform.localScale.y;
        zScale = item.transform.localScale.z;

        xScale += growMultiplier * Time.deltaTime;
        yScale += growMultiplier * Time.deltaTime;
        zScale += growMultiplier * Time.deltaTime;
        item.transform.localScale = new Vector3(xScale, yScale, zScale); // update the size

        // If the 'xScale' is bigger than the original size, then set the original size
        if (xScale > junkScript.originalXScale) // remember this statement does not apply to instantiated junk when the 'xScale' is the same as the original scale
        {
            item.transform.localScale = new Vector3(junkScript.originalXScale, junkScript.originalYScale, junkScript.originalZScale);
        }
    }
}