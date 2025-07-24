using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeEnemy : EnemyAI
{
    protected override void Awake()
    {
        base.Awake();
        agent.avoidancePriority = Random.Range(10, 30);
    }

    protected override void Update()
    {
       base.HandlePathing();
    }
}
