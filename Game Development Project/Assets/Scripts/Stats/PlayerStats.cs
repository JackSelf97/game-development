using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;
    [SerializeField] private GameObject mesh = null;
    [SerializeField] private Animator animator = null;
    private int respawnTime = 7;
    public GameObject capsuleMesh = null;
    public HealthBar healthBar = null;
    public int currHP = 0;
    public int maxHP = 100;
    const int zero = 0, one = 1;

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
        capsuleMesh = transform.GetChild(one).gameObject;
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

        // Update the UI & the character mesh
        capsuleMesh.SetActive(false);
        mesh.SetActive(true);
        playerController.UIController(true, false);
        animator.SetTrigger("Death");

        // Wait
        yield return new WaitForSeconds(respawnTime);

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}