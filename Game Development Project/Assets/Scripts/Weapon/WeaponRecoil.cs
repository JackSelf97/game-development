using UnityEngine;

// https://www.youtube.com/watch?v=geieixA4Mqc&t=816s&ab_channel=Gilbert
public class WeaponRecoil : MonoBehaviour
{
    // Rotations
    private Vector3 currRotation = Vector3.zero;
    private Vector3 targetRotation = Vector3.zero;

    // Hip-Fire Recoil
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    // Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    // Update is called once per frame
    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currRotation = Vector3.Slerp(currRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currRotation);
    }

    public void Recoil()
    {
        // can include camera shake
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
    }
}
