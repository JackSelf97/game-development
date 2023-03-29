using UnityEngine;

public class Junk : MonoBehaviour
{
    [Header("Junk Properties")]
    public float originalXScale = 0f;
    public float originalYScale = 0f;
    public float originalZScale = 0f;
    public bool isWorldJunk = false;
    public bool targeted = false;
    public bool shot = false;
    public int weight;
    public float lifeTime = 2f;

    // Player Properties
    private SuckCannon suckCannon = null;
    private Transform firePos = null;

    private void Start()
    {
        // Get the starting scales
        if (isWorldJunk)
        {
            originalXScale = transform.localScale.x;
            originalYScale = transform.localScale.y;
            originalZScale = transform.localScale.z;
        }

        // Get the player refs
        suckCannon = PlayerManager.pMan.player.GetComponent<SuckCannon>();
        firePos = suckCannon.firePos;
    }

    private void Update()
    {
        if (targeted)
            SuckMovement();
              
        if (shot)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime > 0)
                Shrink.sMan.GrowItem(gameObject);
            else
                Shrink.sMan.ShrinkItem(gameObject, true, lifeTime);
        }
    }

    void SuckMovement()
    {
        GetComponent<Collider>().enabled = false;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, firePos.transform.position, suckCannon.force * Time.deltaTime); // 'suckCannon.force' may be too strong?
        float distance = Vector3.Distance(transform.position, firePos.transform.position);

        if (distance <= 2f) // if the junk has reached the firePos
        {
            Shrink.sMan.ShrinkItem(gameObject, false, lifeTime); // shrink 
            if (!gameObject.activeSelf) // only add to the list once the 'hitObject' is no longer active
            {
                suckCannon.currHitObject.Add(gameObject);
                suckCannon.UpdateAmmo(1);
                targeted = false;
            }
        }
    }
}