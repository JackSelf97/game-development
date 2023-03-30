using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private GameObject enemySpawner = null;
    private Spawner spawner = null;

    // Start is called before the first frame update
    void Start()
    {
        if (enemySpawner != null)
            spawner = enemySpawner.GetComponent<Spawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!spawner.complete)
                spawner.isOn = true;

            Destroy(this);
        }
    }
}