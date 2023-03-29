using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController = null;
    private float maxHP = 100f;
    public float health = 0;
    public bool isAlive = false;

    private void Start()
    {
        isAlive = true;
        health = maxHP;
        enemyController = GetComponent<EnemyController>();
    }

    private void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log(gameObject.name + " died.");
    }
}