    using UnityEngine;
public class RaycastWeapon : Weapon
{
    public override void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + weaponData.fireRate;
            PerformRaycast();
        }
    }
}
