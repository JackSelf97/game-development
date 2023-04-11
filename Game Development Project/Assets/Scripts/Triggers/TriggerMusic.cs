using UnityEngine;

public class TriggerMusic : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<AudioManager>().Play("Theme");
            Destroy(this);
        }
    }
}