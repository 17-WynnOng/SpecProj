using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Damageable
{
    protected override void Die()
    {
        base.Die();

        //Add in player specific death stuff here. Like UI popup and so on
    }
}
