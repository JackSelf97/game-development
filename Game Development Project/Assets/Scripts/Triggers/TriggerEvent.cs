using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private GameObject enemySpawner = null;
    private Spawner spawner = null;

    // Start is called before the first frame update
    void Start()
    {
        if (enemySpawner == null)
            return;
        else
            spawner = enemySpawner.GetComponent<Spawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && enemySpawner != null)
        {
            if (!spawner.complete)
                spawner.isOn = true;

            Destroy(this);
        }
    }
}