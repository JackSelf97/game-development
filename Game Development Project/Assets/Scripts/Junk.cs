using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.UIElements;

public class Junk : MonoBehaviour
{
    public bool isWorldJunk = false;
    public bool shot = false;
    public int weight;
    private float lifeTime = 2f;

    private void Update()
    {
        if (shot)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime < 0)
            {
                Shrink.sMan.ShrinkItem(gameObject, true, lifeTime);
            }
            else
            {
                Shrink.sMan.GrowItem(gameObject);
            }
        }
    }
}
