using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currHP = 0;
    private int maxHP = 100;
    public HealthBar healthBar = null;

    // Start is called before the first frame update
    void Start()
    {
        currHP = maxHP;
        healthBar.SetMaxHealth(maxHP);
    }

    public void TakeDamage(int damage)
    {
        currHP -= damage;
        healthBar.SetHealth(currHP);
    }
}
