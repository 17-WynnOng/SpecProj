using System.Runtime.CompilerServices;
using UnityEngine;
public class RaycastWeapon : Weapon
{
    public override void Shoot()
    {
        if (currentMag > 0)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + weaponData.fireRate;
                PerformRaycast();

                currentMag--;
                UIManager.Instance.ammoTxt.text = currentMag + "/" + currentReserve;
            }
        }
    }
}
