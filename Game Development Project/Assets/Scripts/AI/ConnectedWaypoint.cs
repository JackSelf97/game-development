using System.Collections.Generic;
using UnityEngine;

// Used for NPC Patrolling
public class ConnectedWaypoint : MonoBehaviour
{
    [SerializeField] protected float debugDrawRadius = 0.5f;
    [SerializeField] protected float connectivityRadius = 25f;

    List<ConnectedWaypoint> connections;

    // Start is called before the first frame update
    void Start()
    {
        // Grab all the waypoint objects in the scene
        GameObject[] allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");

        // Create a list of waypoints I can refer to later
        connections = new List<ConnectedWaypoint>();

        // Check if they're a connected waypoint
        for (int i = 0; i < allWaypoints.Length; i++)
        {
            ConnectedWaypoint nextWaypoint = allWaypoints[i].GetComponent<ConnectedWaypoint>();

            // We found a waypoint
            if (nextWaypoint != null)
            {
                if (Vector3.Distance(this.transform.position, nextWaypoint.transform.position) <= connectivityRadius && nextWaypoint != this)
                {
                    connections.Add(nextWaypoint);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, connectivityRadius);
    }

    public ConnectedWaypoint NextWaypoint(ConnectedWaypoint previousWaypoint)
    {
        if (connections.Count == 0)
        {
            // No waypoints? then return null
            Debug.LogError("Insufficient waypoint count! -- make sure the connectivityRadius is overlapping each other.");
            return null;
        }
        else if (connections.Count == 1 && connections.Contains(previousWaypoint))
        {
            // Only one waypoint? and it's the previous one? well, use that!
            return previousWaypoint;
        }
        else // Otherwise, find a random one that isn't the previous one
        {
            ConnectedWaypoint nextWaypoint;
            int nextIndex = 0;

            do
            {
                nextIndex = Random.Range(0, connections.Count);
                nextWaypoint = connections[nextIndex];
            } while (nextWaypoint == previousWaypoint);

            return nextWaypoint;
        }
    }
}
