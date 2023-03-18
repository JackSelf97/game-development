using System.Collections.Generic;
using System.Net;
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

    // Private variables for base behaviour 
    Animator animator = null;
    NavMeshAgent navMeshAgent = null;
    ConnectedWaypoint currWaypoint = null;
    ConnectedWaypoint prevWaypoint = null;
    public bool travelling = false;
    public bool waiting = false;
    float waitTimer = 0f;
    int waypointsVisited = 0;

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
            GameObject[] allWaypoints = GameObject.FindGameObjectsWithTag("Waypoint");

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

        SetDestination();
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // Check if we're close to the destination
        if (travelling && navMeshAgent.remainingDistance <= 1f) // has to match the stopping distance
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
        Debug.Log("On the move!");

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
    
}
