using UnityEngine;

// Used for NPC Patrolling
public class Waypoint : MonoBehaviour
{
    [SerializeField] protected float debugDrawRadius = 0.5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}
