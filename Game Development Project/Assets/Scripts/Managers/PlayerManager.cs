using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Singleton

    public static PlayerManager pMan;
    public GameObject player;
    private void Awake()
    {
        pMan = this;
    }

    #endregion
}