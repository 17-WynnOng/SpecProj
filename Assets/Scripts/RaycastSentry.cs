using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastSentry : Weapon
{
    [SerializeField] private Transform firePoint;

    protected override Ray GetFireRay()
    {
        // fire from the turret barrel, straight out its forward
        return new Ray(firePoint.position, firePoint.forward);
    }

    public override void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + weaponData.fireRate;
            PerformRaycast();
        }
    }
}
