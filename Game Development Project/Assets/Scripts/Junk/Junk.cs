using UnityEngine;

public class Junk : MonoBehaviour
{
    // SO
    public JunkData junkItemSO = null;

    [Header("Junk Properties")]
    public float originalXScale = 0f;
    public float originalYScale = 0f;
    public float originalZScale = 0f;
    public bool targeted = false;
    public bool shot = false;
    public float lifeTime = 2f;

    // Player Properties
    private SuckCannon suckCannon = null;
    private GameObject weaponHandler = null;
    [SerializeField] private Transform firePos = null;

    // Constants
    private const int zero = 0;

    private void Start()
    {
        // Get the starting scales
        if (junkItemSO.isWorldJunk)
        {
            originalXScale = transform.localScale.x;
            originalYScale = transform.localScale.y;
            originalZScale = transform.localScale.z;
        }
        else
        {
            transform.localScale = new Vector3(zero, zero, zero);
        }

        // Get the player refs
        suckCannon = PlayerManager.pMan.player.GetComponent<SuckCannon>();
        weaponHandler = Camera.main.transform.GetChild(1).gameObject;
        firePos = weaponHandler.transform.GetChild(zero).transform.GetChild(zero).transform;

        // Give them a little velocity...
        GetComponent<Rigidbody>().AddRelativeForce(Random.onUnitSphere * 5f);
    }

    private void Update()
    {
        if (targeted)
            SuckMovement();
              
        if (shot)
        {
            lifeTime -= Time.deltaTime;
            if (lifeTime > zero)
                Shrink.sMan.GrowItem(gameObject);
            else
                Shrink.sMan.ShrinkItem(gameObject, true, lifeTime);
        }
    }

    // NOTE - This does not add the gameObject to the list
    void SuckMovement()
    {
        transform.parent = null;
        GetComponent<Collider>().enabled = false;
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, firePos.transform.position, suckCannon.force * Time.deltaTime); // 'suckCannon.force' may be too strong?
        float distance = Vector3.Distance(transform.position, firePos.transform.position);

        if (distance <= 2f) // if the junk has reached the firePos
        {
            Shrink.sMan.ShrinkItem(gameObject, false, lifeTime); // shrink 
            if (!gameObject.activeSelf) // only add to the list once the 'hitObject' is no longer active
            {
                suckCannon.UpdateAmmo(1); // can't have update ammo and add gameobject in two different places...
                targeted = false;
            }
        }
    }
}