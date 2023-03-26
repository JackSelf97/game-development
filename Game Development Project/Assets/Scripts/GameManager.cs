using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton & Awake
    public static GameManager gMan = null; // should always initilize

    private void Awake()
    {
        if (gMan == null)
        {
            DontDestroyOnLoad(gameObject);
            gMan = this;
        }
        else if (gMan != null)
        {
            Destroy(gameObject); // if its already there destroy it
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
