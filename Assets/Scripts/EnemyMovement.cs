using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    private NavMeshAgent agent;
    private int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        waypoints = GameManager.Instance.levelPath.waypoints;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[currentIndex].position);
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            currentIndex++;
            if (currentIndex < waypoints.Length)
                agent.SetDestination(waypoints[currentIndex].position);
            else
                Destroy(gameObject);
        }
    }
}
