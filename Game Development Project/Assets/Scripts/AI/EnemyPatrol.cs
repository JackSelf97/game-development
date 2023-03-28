using UnityEngine;
using UnityEngine.AI;

// https://www.youtube.com/watch?v=NK1TssMD5mE&t=501s&ab_channel=TableFlipGames
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    // Dictates whether the agent waits on each node
    [SerializeField] private bool patrolWaiting = false;

    // The total time the agent waits at each node
    [SerializeField] private float totalWaitTime = 3f;

    // The probability of switching direction
    [SerializeField] private float switchProbability = 0.2f;

    // Waypoint Radius
    public Spawner parentSpawner = null;

    // Private variables for base behaviour 
    private Animator animator = null;
    private NavMeshAgent navMeshAgent = null;
    private ConnectedWaypoint currWaypoint = null;
    private ConnectedWaypoint prevWaypoint = null;
    private float waitTimer = 0f;
    private int waypointsVisited = 0;
    public bool travelling = false;
    public bool waiting = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = 2; // change the speed

        if (currWaypoint == null)
        {
            // Set it at random
            // Grab all the waypoint objects in the scene
            if (parentSpawner == null) // meaning if the enemy was NOT instantiated
            {
                GameObject[] allWaypointsInScene = GameObject.FindGameObjectsWithTag("Waypoint"); // then have the freedom to move with every waypoint
                GetWaypoints(allWaypointsInScene);
            }
            else
            {
                GetWaypoints(parentSpawner.allWaypoints);
            }
        }

        SetDestination();
    }

    // Update is called once per frame
    void Update() // using FixedUpdate will mess with the animations
    {
        // Check if we're close to the destination
        if (travelling && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) // has to match the stopping distance
        {
            travelling = false;
            waypointsVisited++;
            
            // If we're going to wait, then wait
            if (patrolWaiting)
            {
                waiting = true;
                animator.SetBool("IsWalking", false);
                waitTimer = 0f;
            }
            else
            {
                SetDestination();
            }
        }

        // Instead if we're waiting
        if (waiting)
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= totalWaitTime)
            {
                waiting = false;
                SetDestination();
            }
        }
    }

    private void SetDestination()
    {
        if (waypointsVisited > 0)
        {
            ConnectedWaypoint nextWaypoint = currWaypoint.NextWaypoint(prevWaypoint);
            prevWaypoint = currWaypoint;
            currWaypoint = nextWaypoint;
        }

        Vector3 target = currWaypoint.transform.position;
        navMeshAgent.SetDestination(target);
        travelling = true;
        animator.SetBool("IsWalking", true);
    }

    private void GetWaypoints(GameObject[] allWaypoints)
    {
        if (allWaypoints.Length > 0)
        {
            while (currWaypoint == null)
            {
                int random = Random.Range(0, allWaypoints.Length);
                ConnectedWaypoint startingWaypoint = allWaypoints[random].GetComponent<ConnectedWaypoint>();

                // We found a waypoint
                if (startingWaypoint != null)
                {
                    currWaypoint = startingWaypoint;
                }
            }
        }
        else
        {
            Debug.LogError("Failed to find any waypoints for use in the scene!");
        }
    }
}