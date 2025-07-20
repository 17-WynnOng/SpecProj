using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeEnemey : EnemyAI
{
    [SerializeField] private float tolerance = 0.3f;

    protected override void Update()
    {
        if (agent.pathPending || agent.remainingDistance > tolerance) return;

        currentIndex++;
        if (currentIndex < path.Length)
            agent.SetDestination(path[currentIndex].position);
        else
            Destroy(gameObject);
    }
}
