using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private GameObject enemySpawner = null;
    private Spawner spawner = null;

    [Header("Events")]
    public NPC npc = null;
    public bool killBox = false;
    public bool music = false;
    public bool exit = false;
    public GameObject exitObj = null;
    public GameObject countdownObj = null;

    // Start is called before the first frame update
    void Start()
    {
        // Spawner
        if (enemySpawner == null)
            return;
        else
            spawner = enemySpawner.GetComponent<Spawner>();

        // NPC
        if (npc != null)
            GetComponent<SlidingDoor>().enabled = false;
    }

    // Event Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Spawner
            if (spawner != null && npc == null)
            {
                if (!spawner.complete)
                    spawner.isOn = true;
            }

            // NPC
            if (npc != null)
            {
                if (npc.dialogueStarted)
                {
                    GetComponent<SlidingDoor>().enabled = true;
                    npc.gameObject.SetActive(false);

                    // Trigger Music
                    if (music)
                    {
                        FindObjectOfType<AudioManager>().Play("Theme");
                        FindObjectOfType<AudioManager>().Stop("Ambience");
                    }

                    // Trigger Exit
                    if (exit)
                    {
                        // Reset the spawners
                        Spawner[] spawners = FindObjectsOfType<Spawner>();
                        for (int i = 0; i < spawners.Length; i++)
                        {
                            spawners[i].spawnCount = spawners[i].maxSpawnCount;
                            spawners[i].isOn = true;
                        }

                        exitObj.GetComponent<Collider>().enabled = true;
                        countdownObj.SetActive(true);
                    }

                    // Trigger Spawner
                    if (spawner != null)
                    {
                        if (!spawner.complete)
                            spawner.isOn = true;
                    }

                    npc = null;
                }
            }

            // Instant Death
            if (killBox)
                PlayerManager.pMan.player.GetComponent<PlayerStats>().TakeDamage(100);

            // Destroy the script
            if (npc == null || npc != null && npc.dialogueStarted)
                Destroy(this);
        }
    }
}