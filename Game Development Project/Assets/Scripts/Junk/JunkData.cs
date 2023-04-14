using UnityEngine;

[CreateAssetMenu(fileName = "New Junk Item", menuName = "Junk")]
public class JunkData : ScriptableObject
{
    public new string name = null;
    public bool isWorldJunk = false;
    public int weight = 0;
}
