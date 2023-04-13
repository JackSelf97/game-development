using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private GameObject enemySpawner = null;
    private Spawner spawner = null;

    [Header("Events")]
    public bool killBox = false;
    public bool music = false;
    public bool exit = false;
    public GameObject exitObj = null;
    public GameObject countdownObj = null;

    // Start is called before the first frame update
    void Start()
    {
        if (enemySpawner == null)
            return;
        else
            spawner = enemySpawner.GetComponent<Spawner>();
    }

    // Event Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (spawner != null)
            {
                if (!spawner.complete)
                    spawner.isOn = true;
            }

            // theme music
            if (music)
            {
                FindObjectOfType<AudioManager>().Play("Theme");
                FindObjectOfType<AudioManager>().Stop("Ambience");
            }
                
            // exit
            if (exit)
            {
                exitObj.GetComponent<Collider>().enabled = true;
                countdownObj.SetActive(true);
            }
                
            // instant death
            if (killBox)
                PlayerManager.pMan.player.GetComponent<PlayerStats>().TakeDamage(100);

            Destroy(this);
        }
    }
}