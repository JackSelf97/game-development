using UnityEngine;

public class GravityBomb : MonoBehaviour
{
    [SerializeField] private float force = 10;
    private Rigidbody rb;
    private float multiplier = 5f;
    private const int lifeTime = 3;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        rb.drag += multiplier * Time.deltaTime; // slows down the bomb after instantiating with impluse force
    }

    private void OnTriggerStay(Collider other)
    {
        const int junkLayer = 6;
        if (other.gameObject.layer == junkLayer)
        {
            // move junk towards bomb
            other.transform.localPosition = Vector3.MoveTowards(other.transform.localPosition, transform.position, force * Time.deltaTime);
        }
    }
}