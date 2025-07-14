using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour
{
    protected NavMeshAgent agent;
    [SerializeField] protected Transform[] path;
    [SerializeField] protected Transform player;
    protected int currentIndex;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
    }

    protected virtual void Start()
    {
        path = GameManager.Instance.levelPath.waypoints;
        currentIndex = 0;
        if (path.Length > 0)
            agent.SetDestination(path[0].position);
    }

    protected virtual void Update()
    {
        if (ShouldChasePlayer())
            ChasePlayer();
        else
            PatrolPath();
    }

    protected abstract bool ShouldChasePlayer();
    protected abstract void ChasePlayer();
    protected abstract void PatrolPath();
}
