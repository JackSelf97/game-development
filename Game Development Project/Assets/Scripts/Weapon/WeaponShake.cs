using UnityEngine;
using Cinemachine;

// https://www.youtube.com/watch?v=ACf1I27I6Tk&t=154s&ab_channel=CodeMonkey
public class WeaponShake : MonoBehaviour
{
    public static WeaponShake wsMan { get; private set; }
    private CinemachineVirtualCamera vCam = null;
    private float shakeTimer = 0;
    private float shakeTimerTotal = 0;
    private float startingIntensity = 0;

    // Start is called before the first frame update
    void Awake()
    {
        wsMan = this;
        vCam = GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0f)
        {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, (1 - (shakeTimer / shakeTimerTotal)));
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }
}