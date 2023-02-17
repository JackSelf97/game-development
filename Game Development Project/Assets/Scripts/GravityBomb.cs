using UnityEngine;

public class GravityBomb : MonoBehaviour
{
    [Header("Upgrades")]
    [SerializeField] private float travelSpeed = 1;

    private LayerMask junkLayer = 6;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 10);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == junkLayer)
        {
            // move junk towards bomb
            other.transform.localPosition = Vector3.MoveTowards(other.transform.localPosition, transform.position, Time.deltaTime * travelSpeed);
        }
    }
}
