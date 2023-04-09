using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private GameObject mesh = null;
    [SerializeField] private Animator animator = null;
    public HealthBar healthBar = null;
    public int currHP = 0;
    public int maxHP = 100;
    private int respawnTime = 7;
    const int zero = 0;

    [Header("Cinemachine")]
    [SerializeField] private GameObject stateDrivenCam1;
    [SerializeField] private GameObject stateDrivenCam2;

    // Start is called before the first frame update
    void Start()
    {
        currHP = maxHP;
        healthBar.SetMaxHealth(maxHP);
        playerController = GetComponent<PlayerController>();
        mesh = transform.GetChild(zero).gameObject;
        animator = mesh.GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (!playerController.lockInput)
        {
            currHP -= damage;
            healthBar.SetHealth(currHP);

            if (currHP <= zero)
            {
                StartCoroutine(PlayerDeath());
            }
        }
    }

    public IEnumerator PlayerDeath()
    {
        // Update the StateDrivenCamera
        stateDrivenCam1.SetActive(false);
        stateDrivenCam2.SetActive(true);

        // Update the UI
        mesh.SetActive(true);
        playerController.UIController(true, false);
        animator.SetTrigger("Death");

        // Wait
        yield return new WaitForSeconds(respawnTime);

        // Load the scene from the beginning
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}