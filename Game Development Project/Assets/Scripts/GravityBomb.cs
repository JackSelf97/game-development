using UnityEngine;

public class GravityBomb : MonoBehaviour
{
    private LayerMask junkLayer = 6;
    private GravityGun gravityGun = null;

    // Start is called before the first frame update
    void Start()
    {
        gravityGun = GameObject.Find("GravityGun").GetComponent<GravityGun>(); // change later
        Destroy(gameObject, 10);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == junkLayer)
        {
            // move junk towards bomb
            other.transform.localPosition = Vector3.MoveTowards(other.transform.localPosition, transform.position, gravityGun.travelSpeed * Time.deltaTime);
        }
    }
}
