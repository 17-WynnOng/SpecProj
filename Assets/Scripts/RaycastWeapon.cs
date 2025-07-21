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
                UIManager.Instance.magazineTxt.text = currentMag.ToString();
                UIManager.Instance.reserveAmmoTxt.text = currentReserve.ToString();
            }
        }
    }

    public override void PerformRaycast()
    {
        Ray ray = GetFireRay();
        if (Physics.Raycast(ray, out var hit, weaponData.range, weaponData.hitLayers))
        {
            if (hit.collider.TryGetComponent<Damageable>(out var d))
            {
                d.TakeDamage(weaponData.damage);
                d.IfDamagedByPlayer();
            }
        }
    }
}
