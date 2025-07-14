using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeEnemey : EnemyAI
{
    [SerializeField] private float tolerance = 0.3f;

    protected override bool ShouldChasePlayer() => false;

    protected override void ChasePlayer()
    {
        
    }

    protected override void PatrolPath()
    {
        if (agent.pathPending || agent.remainingDistance > tolerance) return;

        currentIndex++;
        if (currentIndex < path.Length)
            agent.SetDestination(path[currentIndex].position);
        else
            Destroy(gameObject);
    }
}
