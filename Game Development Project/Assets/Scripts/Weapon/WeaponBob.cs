using UnityEngine;

// https://www.youtube.com/watch?v=DR4fTllQnXg&t=1s&ab_channel=BuffaLou
public class WeaponBob : MonoBehaviour
{
    private PlayerController playerController = null;
    private CharacterController controller = null;

    [Header("Bobbing")]
    public float smooth = 10f;
    float smoothRot = 12f;
    public float speedCurve;
    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Cos(speedCurve); }
    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;
    Vector3 bobPosition;
    Vector2 walkInput;
    public float bobExaggeration;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    Vector3 bobEulerRotation;

    private void Start()
    {
        playerController = PlayerManager.pMan.player.GetComponent<PlayerController>();
        controller = PlayerManager.pMan.player.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }

    void GetInput()
    {
        walkInput.x = playerController.GetPlayerMovement().x;
        walkInput.y = playerController.GetPlayerMovement().y;
        walkInput = walkInput.normalized;
    }

    void CompositePositionRotation()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, bobPosition, Time.deltaTime * smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot);
    }

    void BobOffset()
    {
        speedCurve += Time.deltaTime * (controller.isGrounded ? (walkInput.x + walkInput.y) * bobExaggeration : 1f) + 0.01f;

        bobPosition.x = (curveCos * bobLimit.x * (controller.isGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
        bobPosition.y = (curveSin * bobLimit.y) - (walkInput.y * travelLimit.y);
        bobPosition.z = -(walkInput.y * travelLimit.z);
    }

    void BobRotation()
    {
        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    }
}