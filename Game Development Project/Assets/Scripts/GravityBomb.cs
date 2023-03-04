using UnityEngine;

public class GravityBomb : MonoBehaviour
{
    [SerializeField] private float travelSpeed = 10;
    private const int lifeTime = 3;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerStay(Collider other)
    {
        const int junkLayer = 6;
        if (other.gameObject.layer == junkLayer)
        {
            // move junk towards bomb
            other.transform.localPosition = Vector3.MoveTowards(other.transform.localPosition, transform.position, travelSpeed * Time.deltaTime);
        }
    }
}