using UnityEngine;

public class Spawner : MonoBehaviour
{
    // What type of enemy will spawn
    public enum EnemyType
    {
        Zombie_Runner,
    }
    public EnemyType enemyType;

    // Enemy types
    [SerializeField] private GameObject zombieRunner = null;

    // How many enemy will be spawned
    [SerializeField] private int spawnCount = 5;

    // Whether you want the spawner to be automatic
    [SerializeField] private bool on = false;
    [SerializeField] private bool spawnOverTime = false;

    // Timers
    [SerializeField] private float timeLimit = 10;
    private float timer = 0;

    // Update is called once per frame
    void Update()
    {
        if (on)
        {
            if (spawnCount <= 0) { return; }

            // Spawns enemy every 'x' amount of seconds
            if (spawnOverTime)
            {
                timer += Time.deltaTime;
                if (timer > timeLimit)
                {
                    SpawnEnemies();
                    timer = 0; // reset
                }
            }
            else
            {
                SpawnEnemies(); // spawns the spawnCount total
            }
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < spawnCount;)
        {
            switch (enemyType)
            {
                case EnemyType.Zombie_Runner:
                    Instantiate(zombieRunner, transform.position, Quaternion.identity);
                    break;
                default:
                    break;
            }
            spawnCount--;
            break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}