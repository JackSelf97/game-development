using UnityEngine;

// https://www.youtube.com/watch?v=QIVN-T-1QBE&ab_channel=Plai
public class WeaponSway : MonoBehaviour
{
    [Header("Swaying")]
    [SerializeField] private float smooth;
    [SerializeField] private float swayMultiplier;
    private PlayerController playerController = null;

    private void Start()
    {
        playerController = PlayerManager.pMan.player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        float mouseX = playerController.GetMouseDelta().x * swayMultiplier;
        float mouseY = playerController.GetMouseDelta().y * swayMultiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}