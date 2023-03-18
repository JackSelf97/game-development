using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    private PlayerStats playerStats = null;

    private void Start()
    {
        playerStats = PlayerManager.pMan.player.GetComponent<PlayerStats>();
        GetComponent<Collider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            playerStats.TakeDamage(damage);
            Debug.Log("Player has taken damage! The player health is now: " + playerStats.currHP);
        }
    }

}
