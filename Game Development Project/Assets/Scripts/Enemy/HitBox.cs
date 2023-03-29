using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    private PlayerStats playerStats = null;

    private void Start()
    {
        playerStats = PlayerManager.pMan.player.GetComponent<PlayerStats>();
        GetComponent<Collider>().enabled = true; // ragdoll turns this off, so enable it after
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            playerStats.TakeDamage(damage);
        }
    }
}