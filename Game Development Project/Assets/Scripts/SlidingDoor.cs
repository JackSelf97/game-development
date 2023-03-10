using System.Collections;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Tooltip("Enter -1.5f for the door to slide left and +1.5f for the door to slide right")]
    [SerializeField] private float xValue = 0, zValue = 0;

    [SerializeField] private float doorSpeed = 1.2f;
    [SerializeField] private bool isClosed = false;
    [SerializeField] private Vector3 pointA, pointB;

    private void Start()
    {
        // get the positions of 'pointA' and 'pointB'
        pointA = transform.position;
        pointB = new Vector3(transform.position.x + xValue, transform.position.y, transform.position.z + zValue);
    }

    private void FixedUpdate()
    {
        if (isClosed)
        {
            StartCoroutine(MoveDoor(pointB));
        }
        else if (!isClosed)
        {
            StartCoroutine(MoveDoor(pointA));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            isClosed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Enemy"))
        {
            isClosed = false;
        }
    }

    IEnumerator MoveDoor(Vector3 endPos)
    {
        while (transform.position != endPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, Time.fixedDeltaTime * doorSpeed);
            yield return null;
        }
    }
}
