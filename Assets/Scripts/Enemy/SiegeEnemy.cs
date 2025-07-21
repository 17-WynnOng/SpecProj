using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeEnemy : EnemyAI
{
    protected override void Update()
    {
       base.HandlePathing();
    }
}
