using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// https://www.youtube.com/watch?v=5q4JHuJAAcQ&ab_channel=TableFlipGames
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    // Dictates whether the agent waits on each node
    [SerializeField] private bool patrolWaiting = false;

    // The total time the agent waits at each node
    [SerializeField] private float totalWaitTime = 3f;

    // The probability of switching direction
    [SerializeField] private float switchProbability = 0.2f;

    // The list of all patrol nodes the agent can vist
    [SerializeField] private List<Waypoint> patrolPoints = new List<Waypoint>();

    // Private variables for base behaviour 
    NavMeshAgent navMeshAgent = null;
    int currPatrolIndex = 0;
    bool travelling = false;
    bool waiting = false;
    bool patrolForward = false;
    float waitTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = 3; // change the speed

        if (patrolPoints != null && patrolPoints.Count >= 2)
        {
            currPatrolIndex = 0;
            SetDestination();
        }
        else
        {
            Debug.Log("Insufficient patrol points for basic patrolling behaviour!");
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if we're close to the destination
        if (travelling && navMeshAgent.remainingDistance <= 1f)
        {
            travelling = false;

            // If we're going to wait, then wait
            if (patrolWaiting)
            {
                waiting = true;
                waitTimer = 0f;
            }
            else
            {
                ChangePatrolPoint();
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
                ChangePatrolPoint();
                SetDestination();
            }
        }
    }

    private void SetDestination()
    {
        if (patrolPoints != null)
        {
            Vector3 target = patrolPoints[currPatrolIndex].transform.position;
            navMeshAgent.SetDestination(target);
            travelling = true;
        }
    }

    // Select a new patrol point in the available list, but also with a small probability allows for us to move forwards or backwards
    private void ChangePatrolPoint()
    {
        if (Random.Range(0f, 1f) <= switchProbability)
        {
            patrolForward = !patrolForward;
        }

        if (patrolForward)
        {
            //currPatrolIndex++;
            //if (currPatrolIndex >= patrolPoints.Count)
            //{
            //    currPatrolIndex = 0;
            //}

            // The code below does the same thing as the code above
            currPatrolIndex = (currPatrolIndex + 1) % patrolPoints.Count;
        }
        else if (--currPatrolIndex < 0) // decrement it at the start using '--'
        {
            currPatrolIndex = patrolPoints.Count - 1;
        }
    }
}
