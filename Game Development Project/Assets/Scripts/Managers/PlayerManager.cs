using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton

    public static PlayerManager pMan;
    private void Awake()
    {
        pMan = this;
    }

    #endregion

    public GameObject player;
}