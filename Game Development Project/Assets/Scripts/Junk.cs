using System.Collections;
using UnityEngine;

public class Junk : MonoBehaviour
{
    public bool isWorldJunk = false;
    public bool shot = false;
    public int weight;
    private bool shrink;
    private float xScale, yScale, zScale;
    private float lifeTime = 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        xScale = transform.localScale.x; // size of the objects
        yScale = transform.localScale.y;
        zScale = transform.localScale.z;
    }

    private void Update()
    {
        if (shot)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime < 0)
            {
                StartCoroutine(ShrinkDeath());
            }
        }

        if (shrink)
        {
            const int zero = 0;
            xScale -= Time.deltaTime;
            yScale -= Time.deltaTime;
            zScale -= Time.deltaTime;
            transform.localScale = new Vector3(xScale, yScale, zScale);

            if (xScale <= zero)
            {
                xScale = zero;
                yScale = zero;
                zScale = zero;
                shrink = false;
            }
        }
    }

    public IEnumerator ShrinkDeath()
    {
        shrink = true;
        gameObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
