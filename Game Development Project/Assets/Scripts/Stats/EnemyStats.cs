using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float health = 0;
    private float maxHP = 100f;
    public bool isAlive = false;
    [SerializeField] private EnemyController enemyController = null;

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
