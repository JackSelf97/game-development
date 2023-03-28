using Unity.VisualScripting;
using UnityEngine;

public class Shrink : MonoBehaviour
{
    public static Shrink sMan { get; private set; }
    private float xScale, yScale, zScale;

    // Start is called before the first frame update
    void Start()
    {
        sMan = this;
    }

    public void ShrinkItem(GameObject item, bool destroy = false, float lifeTime = 0f) // this function needs to be called in Update()
    {
        xScale = item.transform.localScale.x; // size of the objects
        yScale = item.transform.localScale.y;
        zScale = item.transform.localScale.z;

        const int zero = 0;
        xScale -= Time.deltaTime;
        yScale -= Time.deltaTime;
        zScale -= Time.deltaTime;
        item.transform.localScale = new Vector3(xScale, yScale, zScale);

        if (xScale <= zero)
        {
            xScale = zero;
            yScale = zero;
            zScale = zero;

            // Destroy or SetActive(false) ??
            if (!destroy)
                item.SetActive(false);
            else
            {
                item.GetComponent<Collider>().enabled = false;
                Destroy(item, lifeTime);
            }
        }
    }

    public void GrowItem(GameObject item)
    {
        xScale = item.transform.localScale.x; // size of the objects
        yScale = item.transform.localScale.y;
        zScale = item.transform.localScale.z;

        const float half = 0.5f;
        if (xScale >= half)
        {
            xScale = half;
            yScale = half;
            zScale = half;
        }
        else
        {
            xScale += Time.deltaTime;
            yScale += Time.deltaTime;
            zScale += Time.deltaTime;
            item.transform.localScale = new Vector3(xScale, yScale, zScale);
        }
    }
}